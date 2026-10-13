using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DesignFlow.Application.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesignFlow.Infrastructure.Autodesk;

public sealed class AutodeskOptions
{
    public const string SectionName = "Autodesk";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string CallbackUrl { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ClientSecret);
}

public sealed class ApsTokenProvider : IApsTokenProvider
{
    private readonly HttpClient _http;
    private readonly AutodeskOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ApsTokenProvider> _logger;

    public ApsTokenProvider(HttpClient http, IOptions<AutodeskOptions> options, IMemoryCache cache, ILogger<ApsTokenProvider> logger)
    {
        _http = http;
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    public Task<ApsToken> GetTwoLeggedTokenAsync(CancellationToken cancellationToken) =>
        GetTokenAsync("data:read data:write bucket:read", cancellationToken);

    public async Task<ApsToken> GetTokenAsync(string scope, CancellationToken cancellationToken)
    {
        var cacheKey = "aps-token:" + scope;
        if (_cache.TryGetValue(cacheKey, out ApsToken? cached) && cached is not null)
        {
            return cached;
        }

        var token = await RequestTokenAsync(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["grant_type"] = "client_credentials",
            ["scope"] = scope
        }, "APS", cancellationToken);

        _cache.Set(cacheKey, token, TimeSpan.FromSeconds(Math.Max(30, token.ExpiresInSeconds - 60)));
        return token;
    }

    public Task<ApsToken> ExchangeAuthorizationCodeAsync(string code, CancellationToken cancellationToken) =>
        RequestTokenAsync(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.CallbackUrl
        }, "APS", cancellationToken);

    private async Task<ApsToken> RequestTokenAsync(Dictionary<string, string> form, string source, CancellationToken cancellationToken)
    {
        using var response = await _http.PostAsync("/authentication/v2/token", new FormUrlEncodedContent(form), cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("APS token request failed with status {StatusCode}.", (int)response.StatusCode);
            throw new InvalidOperationException("Autodesk authentication failed.");
        }

        using var document = JsonDocument.Parse(body);
        var accessToken = document.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
        var expires = document.RootElement.TryGetProperty("expires_in", out var expiresElement) ? expiresElement.GetInt32() : 3600;
        return new ApsToken(accessToken, expires, source);
    }
}

public sealed class ModelDerivativeClient : IModelDerivativeClient
{
    private readonly HttpClient _http;
    private readonly IApsTokenProvider _tokens;

    public ModelDerivativeClient(HttpClient http, IApsTokenProvider tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    public async Task<ModelTranslationResult> TranslateAsync(string urn, CancellationToken cancellationToken)
    {
        var encoded = Base64Url(urn);
        var token = await _tokens.GetTwoLeggedTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/modelderivative/v2/designdata/job");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            input = new { urn = encoded },
            output = new
            {
                formats = new[]
                {
                    new { type = "svf2", views = new[] { "2d", "3d" } }
                }
            }
        }), Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Model Derivative translation failed ({(int)response.StatusCode}).");
        }

        return new ModelTranslationResult(encoded, "accepted", "APS", body);
    }

    public async Task<ModelMetadataResult> GetMetadataAsync(string urn, CancellationToken cancellationToken)
    {
        var encoded = Base64Url(urn);
        var token = await _tokens.GetTwoLeggedTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/modelderivative/v2/designdata/{encoded}/metadata");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        using var response = await _http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Model Derivative metadata failed ({(int)response.StatusCode}).");
        }

        return new ModelMetadataResult(encoded, "APS", new[] { "metadata" }, body);
    }

    private static string Base64Url(string urn)
    {
        if (urn.All(character => char.IsLetterOrDigit(character) || character is '-' or '_'))
        {
            return urn;
        }

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(urn)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}

public sealed class MockApsTokenProvider : IApsTokenProvider
{
    public Task<ApsToken> GetTwoLeggedTokenAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new ApsToken("mock-two-legged-token", 3600, "Mock"));

    public Task<ApsToken> GetTokenAsync(string scope, CancellationToken cancellationToken) =>
        Task.FromResult(new ApsToken("mock-scoped-token", 3600, "Mock"));

    public Task<ApsToken> ExchangeAuthorizationCodeAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(new ApsToken("mock-three-legged-token", 3600, "Mock"));
}

public sealed class MockModelDerivativeClient : IModelDerivativeClient
{
    public Task<ModelTranslationResult> TranslateAsync(string urn, CancellationToken cancellationToken) =>
        Task.FromResult(new ModelTranslationResult(urn, "mock-accepted", "Mock", "Autodesk credentials are not configured. No model was translated."));

    public Task<ModelMetadataResult> GetMetadataAsync(string urn, CancellationToken cancellationToken) =>
        Task.FromResult(new ModelMetadataResult(urn, "Mock", Array.Empty<string>(), "No geometry was inspected. Configure APS credentials to fetch metadata."));
}

public sealed class StubDataManagementClient : IDataManagementClient
{
    public Task<IReadOnlyList<string>> ListHubsAsync(CancellationToken cancellationToken)
    {
        // TODO: GET https://developer.api.autodesk.com/project/v1/hubs once Data Management is in scope.
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }
}

public sealed class StubDesignAutomationClient : IDesignAutomationClient
{
    public Task<string> SubmitWorkItemAsync(string activity, string? payload, CancellationToken cancellationToken)
    {
        // TODO: POST a Design Automation work item and poll its status.
        return Task.FromResult($"Design Automation adapter is a stub. Activity '{activity}' was accepted locally and was not sent to Autodesk.");
    }
}

public sealed class StubApsWebhookRegistry : IApsWebhookRegistry
{
    public Task<string> RegisterAsync(string callbackUrl, CancellationToken cancellationToken)
    {
        // TODO: POST https://developer.api.autodesk.com/webhooks/v1/systems/data/events/.../hooks
        return Task.FromResult($"Webhook registration is a stub. Point APS at {callbackUrl} manually until registration is implemented.");
    }
}
