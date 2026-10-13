using DesignFlow.Application.Contracts;

namespace DesignFlow.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<UserProfileDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
}

public interface IClientService
{
    Task<IReadOnlyList<ClientDto>> ListAsync(CancellationToken cancellationToken);

    Task<ClientDto> CreateAsync(CreateClientRequest request, Guid actorId, CancellationToken cancellationToken);
}

public interface IProjectService
{
    Task<IReadOnlyList<ProjectListDto>> ListAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);

    Task<ProjectDetailDto> GetAsync(Guid projectId, Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);

    Task<ProjectDetailDto> CreateAsync(CreateProjectRequest request, Guid actorId, CancellationToken cancellationToken);

    Task<RequirementDto> AddRequirementAsync(Guid projectId, CreateRequirementRequest request, Guid actorId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);

    Task<PlanProjectResponse> ApproveTaskPlanAsync(Guid projectId, Guid reviewerId, string? comments, CancellationToken cancellationToken);

    Task<PlanProjectResponse> RejectTaskPlanAsync(Guid projectId, Guid reviewerId, string? comments, CancellationToken cancellationToken);

    Task<ProjectReportDto> GetReportAsync(Guid projectId, Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);
}

public interface IIssueService
{
    Task<IReadOnlyList<IssueDto>> ListAsync(Guid? projectId, string? severity, string? status, Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);

    Task<IssueDto> AssignAsync(Guid issueId, Guid assigneeId, Guid actorId, CancellationToken cancellationToken);
}

public interface IRevitService
{
    Task<RevitModelDto> SyncAsync(SyncModelRequest request, Guid actorId, CancellationToken cancellationToken);

    Task<ValidationRunDto> CreateValidationRunAsync(CreateValidationRunRequest request, Guid actorId, CancellationToken cancellationToken);
}

public interface IAutomationService
{
    Task<AutomationJobDto> EnqueueAsync(CreateAutomationJobRequest request, Guid actorId, CancellationToken cancellationToken);

    Task<AutomationJobDto> GetAsync(Guid jobId, CancellationToken cancellationToken);
}

public interface IAssistantService
{
    Task<AnalyzeBriefResponse> AnalyzeAsync(AnalyzeBriefRequest request, Guid actorId, CancellationToken cancellationToken);

    Task<PlanProjectResponse> PlanAsync(Guid projectId, PlanProjectRequest request, Guid actorId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken);

    Task<CommandPreviewDto> PreviewCommandAsync(PreviewCommandRequest request, Guid actorId, CancellationToken cancellationToken);

    Task<ExecuteCommandResponse> ExecuteCommandAsync(ExecuteCommandRequest request, Guid actorId, CancellationToken cancellationToken);

    Task<AskAiResponse> AskAsync(AskAiRequest request, Guid actorId, CancellationToken cancellationToken);
}

public interface IIntegrationStatusService
{
    IntegrationStatusDto GetStatus();
}

public interface IApsWebhookService
{
    Task<ApsWebhookAccepted> ReceiveAsync(string payload, CancellationToken cancellationToken);
}

/// <summary>Studio screens that need lists and board moves beyond the core workflow.</summary>
public interface IWorkspaceService
{
    Task<IReadOnlyList<TeamMemberDto>> ListUsersAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<NotificationDto>> ListNotificationsAsync(Guid userId, CancellationToken cancellationToken);

    Task MarkNotificationReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AutomationJobDto>> ListJobsAsync(CancellationToken cancellationToken);

    Task<TaskDto> CreateTaskAsync(Guid projectId, CreateTaskRequest request, Guid actorId, CancellationToken cancellationToken);

    Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, string status, CancellationToken cancellationToken);
}
