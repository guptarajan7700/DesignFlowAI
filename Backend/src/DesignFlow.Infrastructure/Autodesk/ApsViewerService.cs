using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DesignFlow.Application.Abstractions;
using DesignFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DesignFlow.Infrastructure.Autodesk;

/// <summary>Gives the browser a view token and a translated model for the Autodesk Viewer.</summary>
public sealed class ApsViewerService : IApsViewerService
{
    private const string ViewerScope = "data:read data:write data:create bucket:create bucket:read viewables:read";
    private const string ObjectKey = "studio-room.obj";
    private const string CacheKey = "aps-demo-viewer-urn";

    private readonly HttpClient _http;
    private readonly IApsTokenProvider _tokens;
    private readonly DesignFlowDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly AutodeskOptions _options;

    public ApsViewerService(
        HttpClient http,
        IApsTokenProvider tokens,
        DesignFlowDbContext db,
        IMemoryCache cache,
        Microsoft.Extensions.Options.IOptions<AutodeskOptions> options)
    {
        _http = http;
        _tokens = tokens;
        _db = db;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<ViewerSessionDto> GetSessionAsync(Guid? projectId, CancellationToken cancellationToken)
    {
        var token = await _tokens.GetTokenAsync(ViewerScope, cancellationToken);
        var owned = await FindProjectModelAsync(projectId, cancellationToken);
        if (owned is not null)
        {
            return Ready(token, EncodeUrn(owned.Value.Urn), owned.Value.Name, "This is the translated model stored for the project.");
        }

        if (_cache.TryGetValue(CacheKey, out string? cached) && !string.IsNullOrWhiteSpace(cached))
        {
            return Ready(token, cached, "Sample studio room", "Autodesk Viewer is showing the sample room. A synced Revit URN replaces it.");
        }

        var encoded = await EnsureSampleAsync(token.AccessToken, cancellationToken);
        if (string.IsNullOrWhiteSpace(encoded))
        {
            return new ViewerSessionDto(null, 0, null, "Sample studio room", "Preparing", "Autodesk is still translating the sample room.");
        }

        _cache.Set(CacheKey, encoded, TimeSpan.FromHours(12));
        return Ready(token, encoded, "Sample studio room", "Autodesk Viewer is showing the sample room. A synced Revit URN replaces it.");
    }

    private async Task<(string Urn, string Name)?> FindProjectModelAsync(Guid? projectId, CancellationToken cancellationToken)
    {
        if (projectId is not Guid id)
        {
            return null;
        }

        var model = await _db.RevitModels
            .AsNoTracking()
            .Where(item => item.ProjectId == id && item.AutodeskUrn != null && item.AutodeskUrn != "")
            .OrderByDescending(item => item.LastSyncedAt)
            .Select(item => new { item.AutodeskUrn, item.Name })
            .FirstOrDefaultAsync(cancellationToken);

        return model is null || string.IsNullOrWhiteSpace(model.AutodeskUrn)
            ? null
            : (model.AutodeskUrn, model.Name);
    }

    private async Task<string?> EnsureSampleAsync(string accessToken, CancellationToken cancellationToken)
    {
        var bucketKey = BucketKey();
        await CreateBucketAsync(accessToken, bucketKey, cancellationToken);
        var urn = await UploadSampleAsync(accessToken, bucketKey, cancellationToken);
        var encoded = EncodeUrn(urn);
        var state = await ManifestStatusAsync(accessToken, encoded, cancellationToken);
        if (!string.Equals(state, "success", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(state, "inprogress", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(state, "pending", StringComparison.OrdinalIgnoreCase))
            {
                await SubmitTranslationAsync(accessToken, encoded, cancellationToken);
            }

            for (var attempt = 0; attempt < 20; attempt++)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                state = await ManifestStatusAsync(accessToken, encoded, cancellationToken);
                if (string.Equals(state, "success", StringComparison.OrdinalIgnoreCase))
                {
                    return encoded;
                }

                if (string.Equals(state, "failed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Autodesk could not translate the sample room.");
                }
            }

            return null;
        }

        return encoded;
    }

    private string BucketKey()
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(_options.ClientId)));
        return "designflowai_" + hash[..12].ToLowerInvariant();
    }

    private async Task CreateBucketAsync(string accessToken, string bucketKey, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "oss/v2/buckets");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { bucketKey, policyKey = "persistent" }),
            Encoding.UTF8,
            "application/json");
        using var response = await _http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return;
        }

        throw new InvalidOperationException(await FailureAsync("Creating the model bucket", response, cancellationToken));
    }

    private async Task<string> UploadSampleAsync(string accessToken, string bucketKey, CancellationToken cancellationToken)
    {
        var bytes = ReadSample();
        using var signRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"oss/v2/buckets/{bucketKey}/objects/{ObjectKey}/signeds3upload?minutesExpiration=15");
        signRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var signResponse = await _http.SendAsync(signRequest, cancellationToken);
        var signBody = await signResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!signResponse.IsSuccessStatusCode)
        {
            var detail = signBody.Length > 280 ? signBody[..280] : signBody;
            throw new InvalidOperationException($"Preparing the upload failed ({(int)signResponse.StatusCode}). {detail}");
        }

        using var signed = JsonDocument.Parse(signBody);
        var uploadKey = signed.RootElement.GetProperty("uploadKey").GetString()
            ?? throw new InvalidOperationException("Autodesk did not return an upload key.");
        var uploadUrl = signed.RootElement.GetProperty("urls")[0].GetString()
            ?? throw new InvalidOperationException("Autodesk did not return an upload URL.");

        using var storage = new HttpClient();
        using var put = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
        put.Content = new ByteArrayContent(bytes);
        using var putResponse = await storage.SendAsync(put, cancellationToken);
        if (!putResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Sending the sample room failed ({(int)putResponse.StatusCode}).");
        }

        using var complete = new HttpRequestMessage(HttpMethod.Post, $"oss/v2/buckets/{bucketKey}/objects/{ObjectKey}/signeds3upload");
        complete.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        complete.Content = new StringContent(JsonSerializer.Serialize(new { uploadKey }), Encoding.UTF8, "application/json");
        using var completeResponse = await _http.SendAsync(complete, cancellationToken);
        var completeBody = await completeResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!completeResponse.IsSuccessStatusCode)
        {
            var detail = completeBody.Length > 280 ? completeBody[..280] : completeBody;
            throw new InvalidOperationException($"Finishing the upload failed ({(int)completeResponse.StatusCode}). {detail}");
        }

        using var document = JsonDocument.Parse(completeBody);
        if (document.RootElement.TryGetProperty("objectId", out var objectId))
        {
            var urn = objectId.GetString();
            if (!string.IsNullOrWhiteSpace(urn))
            {
                return urn;
            }
        }

        return $"urn:adsk.objects:os.object:{bucketKey}/{ObjectKey}";
    }

    private async Task SubmitTranslationAsync(string accessToken, string encodedUrn, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "modelderivative/v2/designdata/job");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            input = new { urn = encodedUrn },
            output = new
            {
                formats = new[]
                {
                    new { type = "svf2", views = new[] { "2d", "3d" } }
                }
            }
        }), Encoding.UTF8, "application/json");
        using var response = await _http.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            return;
        }

        throw new InvalidOperationException(await FailureAsync("Starting model translation", response, cancellationToken));
    }

    private async Task<string?> ManifestStatusAsync(string accessToken, string encodedUrn, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"modelderivative/v2/designdata/{encodedUrn}/manifest");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await _http.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await FailureAsync("Reading the translation", response, cancellationToken));
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.TryGetProperty("status", out var status) ? status.GetString() : null;
    }

    private static byte[] ReadSample()
    {
        var name = typeof(ApsViewerService).Assembly.GetManifestResourceNames()
            .FirstOrDefault(item => item.EndsWith("studio-room.obj", StringComparison.OrdinalIgnoreCase));
        if (name is null)
        {
            throw new InvalidOperationException("The sample room file is missing from the API.");
        }

        using var stream = typeof(ApsViewerService).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException("The sample room file could not be opened.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static async Task<string> FailureAsync(string action, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var detail = body.Length > 280 ? body[..280] : body;
        return $"{action} failed ({(int)response.StatusCode}). {detail}";
    }

    private static string EncodeUrn(string urn)
    {
        if (!urn.Contains(':'))
        {
            return urn;
        }

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(urn)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static ViewerSessionDto Ready(ApsToken token, string urn, string name, string message) =>
        new(token.AccessToken, token.ExpiresInSeconds, urn, name, "Ready", message);
}

public sealed class UnavailableApsViewerService : IApsViewerService
{
    public Task<ViewerSessionDto> GetSessionAsync(Guid? projectId, CancellationToken cancellationToken) =>
        Task.FromResult(new ViewerSessionDto(
            null,
            0,
            null,
            "Model studio",
            "Unavailable",
            "Autodesk credentials are not configured on the API."));
}
