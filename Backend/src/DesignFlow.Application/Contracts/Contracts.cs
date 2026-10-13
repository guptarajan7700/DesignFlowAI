namespace DesignFlow.Application.Contracts;

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    string? Role);

public sealed record UserProfileDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    bool IsActive,
    IReadOnlyList<string> Roles);

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, UserProfileDto User);

public sealed record CreateClientRequest(
    string CompanyName,
    string ContactName,
    string Email,
    string? Phone,
    string? Website,
    string? AddressLine1,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? Status,
    string? Notes,
    Guid? PortalUserId);

public sealed record ClientDto(
    Guid ClientId,
    string CompanyName,
    string ContactName,
    string Email,
    string? Phone,
    string? City,
    string? Country,
    string Status,
    Guid? PortalUserId);

public sealed record CreateProjectRequest(
    Guid ClientId,
    string Code,
    string Name,
    string? Description,
    string? Status,
    DateOnly? StartDate,
    DateOnly? DueDate,
    decimal Budget,
    decimal Spent,
    string? Currency,
    string? SiteAddress,
    Guid? ProjectManagerId,
    Guid? LeadDesignerId);

public sealed record ProjectListDto(
    Guid ProjectId,
    string Code,
    string Name,
    string Status,
    string ClientName,
    DateOnly? DueDate,
    decimal Budget,
    decimal Spent,
    string Currency,
    string? ProjectManagerName,
    string? LeadDesignerName,
    int OpenIssueCount,
    int OverdueTaskCount,
    int ProposedTaskCount);

public sealed record RequirementDto(
    Guid RequirementId,
    string Title,
    string? Description,
    string Category,
    string Priority,
    string Source,
    string Status,
    int SortOrder);

public sealed record MilestoneDto(
    Guid MilestoneId,
    string Name,
    string? Description,
    DateOnly? DueDate,
    string Status,
    int SortOrder);

public sealed record TaskDto(
    Guid TaskId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    string Source,
    Guid? AssigneeId,
    string? AssigneeName,
    DateOnly? DueDate,
    string? Labels,
    DateTime? ApprovedAt);

public sealed record RevitModelDto(
    Guid RevitModelId,
    string Name,
    string FileName,
    string? VersionLabel,
    string? AutodeskUrn,
    DateTime? LastSyncedAt,
    string Status);

public sealed record ProjectDetailDto(
    Guid ProjectId,
    Guid ClientId,
    string Code,
    string Name,
    string? Description,
    string Status,
    DateOnly? StartDate,
    DateOnly? DueDate,
    decimal Budget,
    decimal Spent,
    string Currency,
    string? SiteAddress,
    string ClientName,
    string? ProjectManagerName,
    string? LeadDesignerName,
    Guid? LeadDesignerId,
    IReadOnlyList<RequirementDto> Requirements,
    IReadOnlyList<MilestoneDto> Milestones,
    IReadOnlyList<TaskDto> Tasks,
    IReadOnlyList<RevitModelDto> Models);

public sealed record CreateRequirementRequest(
    string Title,
    string? Description,
    string Category,
    string Priority,
    string? Source);

public sealed record ReviewTaskPlanRequest(string? Comments);

public sealed record IssueDto(
    Guid IssueId,
    Guid ProjectId,
    string ProjectCode,
    string Title,
    string? Description,
    string Severity,
    string Status,
    string? ElementId,
    string? SuggestedFix,
    Guid? AssignedToId,
    string? AssignedToName,
    Guid? ValidationRunId,
    Guid? RevitModelId,
    DateTime CreatedAt);

public sealed record AssignIssueRequest(Guid AssigneeId);

public sealed record SyncModelRequest(
    Guid ProjectId,
    string Name,
    string FileName,
    string? VersionLabel,
    string? AutodeskUrn,
    string? AutodeskProjectId,
    string? AutodeskItemId);

public sealed record ValidationCheckRequest(
    string CheckCode,
    string CheckName,
    string Status,
    string? ElementId,
    string? ElementCategory,
    string Message,
    string? SuggestedFix);

public sealed record CreateValidationRunRequest(
    Guid RevitModelId,
    IReadOnlyList<ValidationCheckRequest> Results);

public sealed record ValidationRunDto(
    Guid ValidationRunId,
    Guid ProjectId,
    Guid RevitModelId,
    string Status,
    int ChecksPassed,
    int ChecksFailed,
    string? Summary,
    int IssuesCreated);

public sealed record CreateAutomationJobRequest(
    Guid? ProjectId,
    Guid? RevitModelId,
    string JobType,
    string? RequestPayload);

public sealed record AutomationJobDto(
    Guid AutomationJobId,
    Guid? ProjectId,
    Guid? RevitModelId,
    string JobType,
    string Status,
    int ProgressPercent,
    string? ErrorMessage,
    string? ResultPayload,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt);

public sealed record SuggestedTaskDto
{
    public string Title { get; set; } = string.Empty;

    public string Priority { get; set; } = "Medium";

    public string Label { get; set; } = "General";
}

public sealed record RequirementAnalysisDto
{
    public List<string> Rooms { get; set; } = new();

    public string Style { get; set; } = "Unspecified";

    public decimal? Budget { get; set; }

    public List<string> Constraints { get; set; } = new();

    public List<string> Materials { get; set; } = new();

    public List<SuggestedTaskDto> SuggestedTasks { get; set; } = new();

    public List<string> ReviewChecklist { get; set; } = new();
}

public sealed record AnalyzeBriefRequest(string Brief, Guid? ProjectId);

public sealed record AnalyzeBriefResponse(
    RequirementAnalysisDto Analysis,
    bool SavedToProject,
    IReadOnlyList<RequirementDto> SavedRequirements);

public sealed record PlanProjectRequest(string? Brief);

public sealed record PlanProjectResponse(Guid ProjectId, string ApprovalStatus, IReadOnlyList<TaskDto> ProposedTasks);

public sealed record PreviewCommandRequest(string Prompt, Guid? ProjectId);

public sealed record CommandPreviewDto(
    Guid PreviewId,
    string? DetectedCommand,
    string Provider,
    bool Allowed,
    string Impact,
    bool RequiresConfirmation,
    string? ParametersJson);

public sealed record ExecuteCommandRequest(Guid PreviewId, bool Confirm);

public sealed record StoredCheckDto(
    string CheckCode,
    string CheckName,
    string Status,
    string? ElementId,
    string Message,
    string? SuggestedFix);

public sealed record ExecuteCommandResponse(
    Guid PreviewId,
    string Command,
    string Status,
    string Message,
    IReadOnlyList<StoredCheckDto> StoredResults);

public sealed record AskAiRequest(string Question);

public sealed record AskAiResponse(string Answer, string Provider, string Source);

public sealed record ApprovalDto(
    Guid ApprovalId,
    string EntityType,
    string Status,
    string? Comments,
    DateTime RequestedAt,
    DateTime? ReviewedAt);

public sealed record DrawingRevisionDto(
    Guid DrawingRevisionId,
    string SheetNumber,
    string SheetName,
    string RevisionCode,
    string? Description,
    DateTime? IssuedAt);

public sealed record ProjectReportDto(
    Guid ProjectId,
    string Code,
    string Name,
    string Status,
    int OpenIssueCount,
    int OverdueTaskCount,
    decimal PassRatePercent,
    string AiSummaryLabel,
    string AiSummary,
    IReadOnlyList<StoredCheckDto> ValidationResults,
    IReadOnlyList<ApprovalDto> Approvals,
    IReadOnlyList<DrawingRevisionDto> Revisions);

public sealed record OverdueTaskItem(string ProjectCode, string Title, DateOnly? DueDate, string? AssigneeName);

public sealed record OpenIssueItem(string ProjectCode, string Title, string Severity, string Status);

public sealed record ProjectStatusItem(string Code, string Name, string Status);

public sealed record BusinessSnapshot(
    IReadOnlyList<OverdueTaskItem> OverdueTasks,
    IReadOnlyList<OpenIssueItem> OpenIssues,
    IReadOnlyList<ProjectStatusItem> Projects,
    decimal PassRatePercent,
    int ChecksPassed,
    int ChecksFailed);

public sealed record IntegrationStatusDto(
    string AiProvider,
    bool AutodeskConfigured,
    string Authentication,
    string ModelDerivative,
    string DataManagement,
    string DesignAutomation);

public sealed record ApsWebhookAccepted(string Status, int ModelsUpdated);

public sealed record TeamMemberDto(Guid UserId, string FullName, string Email, IReadOnlyList<string> Roles);

public sealed record NotificationDto(
    Guid NotificationId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    Guid? ProjectId,
    DateTime CreatedAt);

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    string? Priority,
    DateOnly? DueDate,
    Guid? AssigneeId);

public sealed record UpdateTaskStatusRequest(string Status);
