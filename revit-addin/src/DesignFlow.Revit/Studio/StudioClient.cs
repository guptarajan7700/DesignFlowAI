using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DesignFlow.Revit.Studio;

public sealed class StudioClient
{
    private static readonly HttpClient Http = new();
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly StudioSettings _settings;
    private string? _token;

    public StudioClient(StudioSettings settings) => _settings = settings;

    public async Task<IReadOnlyList<ProjectListItem>> ProjectsAsync()
    {
        await EnsureTokenAsync();
        var response = await SendAsync(HttpMethod.Get, "/api/projects").ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<List<ProjectListItem>>(Json).ConfigureAwait(false) ?? new List<ProjectListItem>();
    }

    public async Task<RevitModelResponse> SyncAsync(object body)
    {
        await EnsureTokenAsync();
        var response = await SendAsync(HttpMethod.Post, "/api/revit/models/sync", body).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<RevitModelResponse>(Json).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The studio did not return a model id.");
    }

    public async Task<ValidationRunResponse> ValidateAsync(object body)
    {
        await EnsureTokenAsync();
        var response = await SendAsync(HttpMethod.Post, "/api/revit/validation-runs", body).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<ValidationRunResponse>(Json).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The studio did not return a validation run.");
    }

    public async Task<ProjectDetailResponse> ProjectAsync(string projectId)
    {
        await EnsureTokenAsync();
        var response = await SendAsync(HttpMethod.Get, $"/api/projects/{projectId}").ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<ProjectDetailResponse>(Json).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The studio did not return the project.");
    }

    public async Task<AutomationJobResponse> EnqueueAsync(object body)
    {
        await EnsureTokenAsync();
        var response = await SendAsync(HttpMethod.Post, "/api/automation/jobs", body).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<AutomationJobResponse>(Json).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The studio did not return a job.");
    }

    private async Task EnsureTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_token))
        {
            return;
        }

        var url = _settings.ApiUrl.TrimEnd('/') + "/api/auth/login";
        using var response = await Http.PostAsJsonAsync(url, new { email = _settings.Email, password = _settings.Password }, Json).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await ProblemAsync(response).ConfigureAwait(false));
        }

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(Json).ConfigureAwait(false);
        _token = login?.AccessToken;
        if (string.IsNullOrWhiteSpace(_token))
        {
            throw new InvalidOperationException("Login did not return a token.");
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null)
    {
        var request = new HttpRequestMessage(method, _settings.ApiUrl.TrimEnd('/') + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: Json);
        }

        var response = await Http.SendAsync(request).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await ProblemAsync(response).ConfigureAwait(false));
        }

        return response;
    }

    private static async Task<string> ProblemAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        try
        {
            using var document = JsonDocument.Parse(text);
            if (document.RootElement.TryGetProperty("detail", out var detail))
            {
                var message = detail.GetString();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }
        }
        catch
        {
            // The body was not problem+json.
        }

        return string.IsNullOrWhiteSpace(text)
            ? $"The studio returned {(int)response.StatusCode}."
            : text;
    }
}
