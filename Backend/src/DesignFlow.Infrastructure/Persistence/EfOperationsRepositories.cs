using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DesignFlow.Infrastructure.Persistence;

public sealed class EfIssueRepository : IIssueRepository
{
    private readonly DesignFlowDbContext _db;

    public EfIssueRepository(DesignFlowDbContext db) => _db = db;

    public async Task<IReadOnlyList<IssueDto>> ListAsync(
        Guid? projectId,
        string? severity,
        string? status,
        Guid? portalUserId,
        CancellationToken cancellationToken)
    {
        var query = _db.Issues.AsNoTracking().AsQueryable();
        if (projectId is Guid selectedProject)
        {
            query = query.Where(issue => issue.ProjectId == selectedProject);
        }

        if (!string.IsNullOrWhiteSpace(severity) && Enum.TryParse<IssueSeverity>(severity, true, out var parsedSeverity))
        {
            query = query.Where(issue => issue.Severity == parsedSeverity);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<IssueStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(issue => issue.Status == parsedStatus);
        }

        if (portalUserId is Guid userId)
        {
            query = query.Where(issue => issue.Project.Client.PortalUserId == userId);
        }

        return await query
            .OrderByDescending(issue => issue.CreatedAt)
            .Select(issue => new IssueDto(
                issue.IssueId,
                issue.ProjectId,
                issue.Project.Code,
                issue.Title,
                issue.Description,
                issue.Severity.ToString(),
                issue.Status.ToString(),
                issue.ElementId,
                issue.SuggestedFix,
                issue.AssignedToId,
                issue.AssignedTo == null ? null : issue.AssignedTo.FirstName + " " + issue.AssignedTo.LastName,
                issue.ValidationRunId,
                issue.RevitModelId,
                issue.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public Task<Issue?> GetTrackedAsync(Guid issueId, CancellationToken cancellationToken) =>
        _db.Issues.Include(issue => issue.Project).Include(issue => issue.AssignedTo)
            .FirstOrDefaultAsync(issue => issue.IssueId == issueId, cancellationToken);

    public async Task<IReadOnlySet<Guid>> ExistingResultIdsAsync(IEnumerable<Guid> validationResultIds, CancellationToken cancellationToken)
    {
        var ids = validationResultIds.ToList();
        var existing = await _db.Issues.AsNoTracking()
            .Where(issue => issue.ValidationResultId != null && ids.Contains(issue.ValidationResultId.Value))
            .Select(issue => issue.ValidationResultId!.Value)
            .ToListAsync(cancellationToken);
        return existing.ToHashSet();
    }

    public Task AddRangeAsync(IEnumerable<Issue> issues, CancellationToken cancellationToken)
    {
        _db.Issues.AddRange(issues);
        return Task.CompletedTask;
    }
}

public sealed class EfRevitRepository : IRevitRepository
{
    private readonly DesignFlowDbContext _db;

    public EfRevitRepository(DesignFlowDbContext db) => _db = db;

    public Task<RevitModel?> FindByProjectAndFileAsync(Guid projectId, string fileName, CancellationToken cancellationToken) =>
        _db.RevitModels.FirstOrDefaultAsync(model => model.ProjectId == projectId && model.FileName == fileName, cancellationToken);

    public Task<RevitModel?> GetTrackedModelAsync(Guid revitModelId, CancellationToken cancellationToken) =>
        _db.RevitModels.Include(model => model.Project)
            .FirstOrDefaultAsync(model => model.RevitModelId == revitModelId, cancellationToken);

    public Task AddModelAsync(RevitModel model, CancellationToken cancellationToken) =>
        _db.RevitModels.AddAsync(model, cancellationToken).AsTask();

    public Task AddRunAsync(ValidationRun run, CancellationToken cancellationToken) =>
        _db.ValidationRuns.AddAsync(run, cancellationToken).AsTask();

    public Task AddResultsAsync(IEnumerable<ValidationResult> results, CancellationToken cancellationToken)
    {
        _db.ValidationResults.AddRange(results);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<ValidationResult>> GetResultsForCheckAsync(Guid? projectId, string checkCode, CancellationToken cancellationToken)
    {
        var query = _db.ValidationResults.AsNoTracking().Where(result => result.CheckCode == checkCode);
        if (projectId is Guid id)
        {
            query = query.Where(result => result.ValidationRun.ProjectId == id);
        }

        return await query.OrderBy(result => result.SortOrder).ToListAsync(cancellationToken);
    }

    public async Task<int> MarkReadyByUrnAsync(string urn, CancellationToken cancellationToken)
    {
        var models = await _db.RevitModels.Where(model => model.AutodeskUrn == urn).ToListAsync(cancellationToken);
        foreach (var model in models)
        {
            model.Status = RevitModelStatus.Ready;
        }

        return models.Count;
    }

    public async Task<IReadOnlyList<ValidationResult>> GetProjectResultsAsync(Guid projectId, CancellationToken cancellationToken) =>
        await _db.ValidationResults.AsNoTracking()
            .Where(result => result.ValidationRun.ProjectId == projectId)
            .OrderByDescending(result => result.CreatedAt)
            .ThenBy(result => result.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Approval>> GetApprovalsAsync(Guid projectId, CancellationToken cancellationToken) =>
        await _db.Approvals.AsNoTracking()
            .Where(approval => approval.ProjectId == projectId)
            .OrderByDescending(approval => approval.RequestedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DrawingRevision>> GetRevisionsAsync(Guid projectId, CancellationToken cancellationToken) =>
        await _db.DrawingRevisions.AsNoTracking()
            .Where(revision => revision.ProjectId == projectId)
            .OrderByDescending(revision => revision.IssuedAt)
            .ToListAsync(cancellationToken);
}

public sealed class EfAutomationRepository : IAutomationRepository
{
    private readonly DesignFlowDbContext _db;

    public EfAutomationRepository(DesignFlowDbContext db) => _db = db;

    public Task AddAsync(AutomationJob job, CancellationToken cancellationToken) =>
        _db.AutomationJobs.AddAsync(job, cancellationToken).AsTask();

    public Task<AutomationJob?> GetAsync(Guid jobId, CancellationToken cancellationToken) =>
        _db.AutomationJobs.AsNoTracking().FirstOrDefaultAsync(job => job.AutomationJobId == jobId, cancellationToken);

    public Task<AutomationJob?> GetNextQueuedTrackedAsync(CancellationToken cancellationToken) =>
        _db.AutomationJobs
            .Where(job => job.Status == AutomationJobStatus.Queued)
            .OrderBy(job => job.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<RevitModel?> GetModelAsync(Guid revitModelId, CancellationToken cancellationToken) =>
        _db.RevitModels.AsNoTracking().FirstOrDefaultAsync(model => model.RevitModelId == revitModelId, cancellationToken);
}

public sealed class EfAiCommandRepository : IAiCommandRepository
{
    private readonly DesignFlowDbContext _db;

    public EfAiCommandRepository(DesignFlowDbContext db) => _db = db;

    public Task AddAsync(AiCommandLog log, CancellationToken cancellationToken) =>
        _db.AiCommandLogs.AddAsync(log, cancellationToken).AsTask();

    public Task<AiCommandLog?> GetTrackedAsync(Guid logId, CancellationToken cancellationToken) =>
        _db.AiCommandLogs.FirstOrDefaultAsync(log => log.AiCommandLogId == logId, cancellationToken);
}

public sealed class EfNotificationRepository : INotificationRepository
{
    private readonly DesignFlowDbContext _db;

    public EfNotificationRepository(DesignFlowDbContext db) => _db = db;

    public Task AddAsync(Notification notification, CancellationToken cancellationToken) =>
        _db.Notifications.AddAsync(notification, cancellationToken).AsTask();

    public Task<bool> ExistsAsync(string type, Guid relatedEntityId, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<NotificationType>(type, out var parsed))
        {
            return Task.FromResult(false);
        }

        return _db.Notifications.AnyAsync(
            notification => notification.Type == parsed && notification.RelatedEntityId == relatedEntityId,
            cancellationToken);
    }
}

public sealed class EfAuditRepository : IAuditRepository
{
    private readonly DesignFlowDbContext _db;

    public EfAuditRepository(DesignFlowDbContext db) => _db = db;

    public Task AddAsync(AuditLog log, CancellationToken cancellationToken) =>
        _db.AuditLogs.AddAsync(log, cancellationToken).AsTask();

    public Task<bool> ExistsAsync(string entityType, Guid entityId, string action, CancellationToken cancellationToken) =>
        _db.AuditLogs.AnyAsync(
            log => log.EntityType == entityType && log.EntityId == entityId && log.Action == action,
            cancellationToken);
}

public sealed class EfBusinessReadStore : IBusinessReadStore
{
    private readonly DesignFlowDbContext _db;

    public EfBusinessReadStore(DesignFlowDbContext db) => _db = db;

    public async Task<BusinessSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdue = await _db.Tasks.AsNoTracking()
            .Where(task =>
                (task.Status == WorkTaskStatus.ToDo || task.Status == WorkTaskStatus.InProgress || task.Status == WorkTaskStatus.InReview)
                && task.DueDate < today)
            .OrderBy(task => task.DueDate)
            .Select(task => new OverdueTaskItem(
                task.Project.Code,
                task.Title,
                task.DueDate,
                task.Assignee == null ? null : task.Assignee.FirstName + " " + task.Assignee.LastName))
            .ToListAsync(cancellationToken);

        var issues = await _db.Issues.AsNoTracking()
            .Where(issue => issue.Status == IssueStatus.Open || issue.Status == IssueStatus.Assigned || issue.Status == IssueStatus.InProgress)
            .OrderByDescending(issue => issue.CreatedAt)
            .Select(issue => new OpenIssueItem(issue.Project.Code, issue.Title, issue.Severity.ToString(), issue.Status.ToString()))
            .ToListAsync(cancellationToken);

        var projects = await _db.Projects.AsNoTracking()
            .OrderBy(project => project.Code)
            .Select(project => new ProjectStatusItem(project.Code, project.Name, project.Status.ToString()))
            .ToListAsync(cancellationToken);

        var runs = await _db.ValidationRuns.AsNoTracking()
            .Where(run => run.Status == ValidationRunStatus.Passed || run.Status == ValidationRunStatus.Failed)
            .Select(run => new { run.ChecksPassed, run.ChecksFailed })
            .ToListAsync(cancellationToken);
        var passed = runs.Sum(run => run.ChecksPassed);
        var failed = runs.Sum(run => run.ChecksFailed);
        var total = passed + failed;
        var rate = total == 0 ? 0m : Math.Round(100m * passed / total, 2);

        return new BusinessSnapshot(overdue, issues, projects, rate, passed, failed);
    }
}
