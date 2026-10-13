using DesignFlow.Application.Contracts;
using DesignFlow.Domain.Entities;

namespace DesignFlow.Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken);

    Task<Guid?> GetRoleIdAsync(string roleName, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    Task AddRoleAsync(UserRole userRole, CancellationToken cancellationToken);
}

public interface IClientRepository
{
    Task<IReadOnlyList<ClientDto>> ListAsync(CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(Guid clientId, CancellationToken cancellationToken);

    Task AddAsync(Client client, CancellationToken cancellationToken);
}

public interface IProjectRepository
{
    Task<IReadOnlyList<ProjectListDto>> ListAsync(Guid? portalUserId, DateOnly today, CancellationToken cancellationToken);

    Task<Project?> GetDetailAsync(Guid projectId, CancellationToken cancellationToken);

    Task<Project?> GetTrackedAsync(Guid projectId, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);

    Task AddAsync(Project project, CancellationToken cancellationToken);

    Task AddRequirementAsync(ProjectRequirement requirement, CancellationToken cancellationToken);

    Task<int> NextRequirementSortOrderAsync(Guid projectId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProjectTask>> GetTrackedProposedAiTasksAsync(Guid projectId, CancellationToken cancellationToken);

    Task AddTasksAsync(IEnumerable<ProjectTask> tasks, CancellationToken cancellationToken);

    Task<Approval?> GetPendingTaskPlanAsync(Guid projectId, CancellationToken cancellationToken);

    Task AddApprovalAsync(Approval approval, CancellationToken cancellationToken);

    Task<bool> HasPendingTaskPlanAsync(Guid projectId, CancellationToken cancellationToken);
}

public interface IIssueRepository
{
    Task<IReadOnlyList<IssueDto>> ListAsync(Guid? projectId, string? severity, string? status, Guid? portalUserId, CancellationToken cancellationToken);

    Task<Issue?> GetTrackedAsync(Guid issueId, CancellationToken cancellationToken);

    Task<IReadOnlySet<Guid>> ExistingResultIdsAsync(IEnumerable<Guid> validationResultIds, CancellationToken cancellationToken);

    Task AddRangeAsync(IEnumerable<Issue> issues, CancellationToken cancellationToken);
}

public interface IRevitRepository
{
    Task<RevitModel?> FindByProjectAndFileAsync(Guid projectId, string fileName, CancellationToken cancellationToken);

    Task<RevitModel?> GetTrackedModelAsync(Guid revitModelId, CancellationToken cancellationToken);

    Task AddModelAsync(RevitModel model, CancellationToken cancellationToken);

    Task AddRunAsync(ValidationRun run, CancellationToken cancellationToken);

    Task AddResultsAsync(IEnumerable<ValidationResult> results, CancellationToken cancellationToken);

    Task<IReadOnlyList<ValidationResult>> GetResultsForCheckAsync(Guid? projectId, string checkCode, CancellationToken cancellationToken);

    Task<int> MarkReadyByUrnAsync(string urn, CancellationToken cancellationToken);

    Task<IReadOnlyList<ValidationResult>> GetProjectResultsAsync(Guid projectId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Approval>> GetApprovalsAsync(Guid projectId, CancellationToken cancellationToken);

    Task<IReadOnlyList<DrawingRevision>> GetRevisionsAsync(Guid projectId, CancellationToken cancellationToken);
}

public interface IAutomationRepository
{
    Task AddAsync(AutomationJob job, CancellationToken cancellationToken);

    Task<AutomationJob?> GetAsync(Guid jobId, CancellationToken cancellationToken);

    Task<AutomationJob?> GetNextQueuedTrackedAsync(CancellationToken cancellationToken);

    Task<RevitModel?> GetModelAsync(Guid revitModelId, CancellationToken cancellationToken);
}

public interface IAiCommandRepository
{
    Task AddAsync(AiCommandLog log, CancellationToken cancellationToken);

    Task<AiCommandLog?> GetTrackedAsync(Guid logId, CancellationToken cancellationToken);
}

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string type, Guid relatedEntityId, CancellationToken cancellationToken);
}

public interface IAuditRepository
{
    Task AddAsync(AuditLog log, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string entityType, Guid entityId, string action, CancellationToken cancellationToken);
}

public interface IBusinessReadStore
{
    Task<BusinessSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string hash, string password);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user, IReadOnlyList<string> roles);
}

public interface ICurrentUser
{
    Guid UserId { get; }

    IReadOnlyCollection<string> Roles { get; }
}
