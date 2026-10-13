using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Ai;
using DesignFlow.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesignFlow.Infrastructure.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string Provider { get; set; } = "Mock";

    public string Endpoint { get; set; } = "https://api.openai.com/v1";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-4o-mini";

    public string ApiVersion { get; set; } = "2024-02-01";

    public bool UseRemote =>
        !string.IsNullOrWhiteSpace(ApiKey)
        && (Provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase)
            || Provider.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase));
}

public sealed class OpenAiCompatibleService : IAiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly AiOptions _options;
    private readonly ILogger<OpenAiCompatibleService> _logger;

    public OpenAiCompatibleService(HttpClient http, IOptions<AiOptions> options, ILogger<OpenAiCompatibleService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public string ProviderName => _options.Provider;

    public async Task<RequirementAnalysisDto> AnalyzeBriefAsync(string brief, CancellationToken cancellationToken)
    {
        var system = """
            You extract interior-design requirements. Reply with JSON only, no markdown.
            Shape: {"rooms":[],"style":"","budget":null,"constraints":[],"materials":[],"suggestedTasks":[{"title":"","priority":"Medium","label":""}],"reviewChecklist":[]}
            Use only facts present in the brief. Do not invent a budget. Priority is Low, Medium, High, or Critical.
            """;
        return await CompleteOrFallback(
            system,
            brief,
            json => JsonSerializer.Deserialize<RequirementAnalysisDto>(json, JsonOptions),
            () => BriefAnalyzer.Analyze(brief),
            cancellationToken);
    }

    public async Task<IReadOnlyList<SuggestedTaskDto>> PlanTasksAsync(string projectName, CancellationToken cancellationToken)
    {
        var system = """
            Create an interior-design delivery plan. Reply with a JSON array only.
            Each item is {"title":"","priority":"Medium","label":""}.
            Include concept, modeling, review, revisions, and delivery. Do not invent site facts.
            """;
        return await CompleteOrFallback(
            system,
            projectName,
            json => JsonSerializer.Deserialize<List<SuggestedTaskDto>>(json, JsonOptions),
            () => BriefAnalyzer.Plan(projectName).ToList(),
            cancellationToken) ?? BriefAnalyzer.Plan(projectName);
    }

    public async Task<string?> ClassifyCommandAsync(string prompt, CancellationToken cancellationToken)
    {
        var allowed = string.Join(", ", Domain.RevitCommands.Whitelist);
        var system = $"Reply with exactly one of these tokens, or NONE: {allowed}. Do not explain.";
        var text = await CompleteAsync(system, prompt, cancellationToken);
        if (text is null)
        {
            return null;
        }

        var token = text.Trim().Trim('"');
        return Domain.RevitCommands.IsAllowed(token) ? token : null;
    }

    private async Task<T> CompleteOrFallback<T>(
        string system,
        string user,
        Func<string, T?> parse,
        Func<T> fallback,
        CancellationToken cancellationToken)
    {
        var text = await CompleteAsync(system, user, cancellationToken);
        if (text is null)
        {
            return fallback();
        }

        try
        {
            var parsed = parse(StripFences(text));
            return parsed is null ? fallback() : parsed;
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "AI response was not valid JSON. Using the local fallback.");
            return fallback();
        }
    }

    private async Task<string?> CompleteAsync(string system, string user, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, ChatPath());
            if (_options.Provider.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.Add("api-key", _options.ApiKey);
            }
            else
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            }

            var body = JsonSerializer.Serialize(new
            {
                model = _options.Model,
                temperature = 0,
                messages = new object[]
                {
                    new { role = "system", content = system },
                    new { role = "user", content = user }
                }
            });
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            using var response = await _http.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI provider returned {StatusCode}. Using the local fallback.", (int)response.StatusCode);
                return null;
            }

            using var document = JsonDocument.Parse(payload);
            return document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "AI provider call failed. Using the local fallback.");
            return null;
        }
    }

    private string ChatPath()
    {
        if (_options.Provider.Equals("AzureOpenAI", StringComparison.OrdinalIgnoreCase))
        {
            return $"openai/deployments/{_options.Model}/chat/completions?api-version={_options.ApiVersion}";
        }

        return "chat/completions";
    }

    private static string StripFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewLine = trimmed.IndexOf('\n');
        var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        if (firstNewLine < 0 || lastFence <= firstNewLine)
        {
            return trimmed;
        }

        return trimmed[(firstNewLine + 1)..lastFence].Trim();
    }
}
