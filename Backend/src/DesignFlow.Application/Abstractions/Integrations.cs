using DesignFlow.Application.Contracts;

namespace DesignFlow.Application.Abstractions;

public interface IAiService
{
    string ProviderName { get; }

    Task<RequirementAnalysisDto> AnalyzeBriefAsync(string brief, CancellationToken cancellationToken);

    Task<IReadOnlyList<SuggestedTaskDto>> PlanTasksAsync(string projectName, CancellationToken cancellationToken);

    Task<string?> ClassifyCommandAsync(string prompt, CancellationToken cancellationToken);
}

public interface IApsTokenProvider
{
    Task<ApsToken> GetTwoLeggedTokenAsync(CancellationToken cancellationToken);

    Task<ApsToken> GetTokenAsync(string scope, CancellationToken cancellationToken);

    Task<ApsToken> ExchangeAuthorizationCodeAsync(string code, CancellationToken cancellationToken);
}

public sealed record ApsToken(string AccessToken, int ExpiresInSeconds, string Source);

public interface IModelDerivativeClient
{
    Task<ModelTranslationResult> TranslateAsync(string urn, CancellationToken cancellationToken);

    Task<ModelMetadataResult> GetMetadataAsync(string urn, CancellationToken cancellationToken);
}

public sealed record ModelTranslationResult(string Urn, string Status, string Source, string? Detail);

public sealed record ModelMetadataResult(string Urn, string Source, IReadOnlyList<string> Viewables, string? Detail);

public interface IDataManagementClient
{
    Task<IReadOnlyList<string>> ListHubsAsync(CancellationToken cancellationToken);
}

public interface IDesignAutomationClient
{
    Task<string> SubmitWorkItemAsync(string activity, string? payload, CancellationToken cancellationToken);
}

public interface IApsWebhookRegistry
{
    Task<string> RegisterAsync(string callbackUrl, CancellationToken cancellationToken);
}

public interface IApsViewerService
{
    Task<ViewerSessionDto> GetSessionAsync(Guid? projectId, CancellationToken cancellationToken);
}

public sealed record ViewerSessionDto(
    string? AccessToken,
    int ExpiresIn,
    string? Urn,
    string ModelName,
    string Status,
    string? Message);

public interface IAutomationJobProcessor
{
    Task<bool> ProcessNextAsync(CancellationToken cancellationToken);
}
