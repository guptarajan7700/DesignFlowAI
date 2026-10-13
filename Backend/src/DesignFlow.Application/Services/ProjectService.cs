using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Common;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Services;

public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IClientRepository _clients;
    private readonly IUserRepository _users;
    private readonly IRevitRepository _revit;
    private readonly INotificationRepository _notifications;
    private readonly IAuditRepository _audit;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(
        IProjectRepository projects,
        IClientRepository clients,
        IUserRepository users,
        IRevitRepository revit,
        INotificationRepository notifications,
        IAuditRepository audit,
        IUnitOfWork unitOfWork)
    {
        _projects = projects;
        _clients = clients;
        _users = users;
        _revit = revit;
        _notifications = notifications;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<ProjectListDto>> ListAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        var portalUserId = RoleNames.IsClientOnly(roles) ? userId : (Guid?)null;
        return _projects.ListAsync(portalUserId, DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
    }

    public async Task<ProjectDetailDto> GetAsync(Guid projectId, Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        var project = await _projects.GetDetailAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");
        EnsureCanRead(project, userId, roles);
        return ProjectMapper.ToDetail(project);
    }

    public async Task<ProjectDetailDto> CreateAsync(CreateProjectRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        if (!await _clients.ExistsAsync(request.ClientId, cancellationToken))
        {
            throw new NotFoundException("Client was not found.");
        }

        var code = request.Code.Trim();
        if (await _projects.CodeExistsAsync(code, cancellationToken))
        {
            throw new ConflictException("A project with that code already exists.");
        }

        await EnsureUserExists(request.ProjectManagerId, "Project manager", cancellationToken);
        await EnsureUserExists(request.LeadDesignerId, "Lead designer", cancellationToken);

        var status = string.IsNullOrWhiteSpace(request.Status)
            ? ProjectStatus.Active
            : EnumParser.Parse<ProjectStatus>(request.Status, "Status");
        if (status is ProjectStatus.NeedsAttention or ProjectStatus.Completed or ProjectStatus.Archived)
        {
            throw new AppValidationException("New projects start as Draft, Active, OnHold, or InReview.");
        }

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "INR" : request.Currency.Trim().ToUpperInvariant();
        if (currency.Length != 3)
        {
            throw new AppValidationException("Currency must be a 3-letter code.");
        }

        var project = new Project
        {
            ProjectId = Guid.NewGuid(),
            ClientId = request.ClientId,
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Status = status,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            Budget = request.Budget,
            Spent = request.Spent,
            Currency = currency,
            SiteAddress = request.SiteAddress?.Trim(),
            ProjectManagerId = request.ProjectManagerId,
            LeadDesignerId = request.LeadDesignerId,
            CreatedBy = actorId
        };

        await _projects.AddAsync(project, cancellationToken);
        await _audit.AddAsync(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            UserId = actorId,
            EntityType = "Project",
            EntityId = project.ProjectId,
            Action = "Created",
            NewValues = $"{{\"code\":\"{project.Code}\"}}",
            CreatedBy = actorId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _projects.GetDetailAsync(project.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found after save.");
        return ProjectMapper.ToDetail(created);
    }

    public async Task<RequirementDto> AddRequirementAsync(
        Guid projectId,
        CreateRequirementRequest request,
        Guid actorId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        var project = await _projects.GetTrackedAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");
        EnsureCanRead(project, actorId, roles);

        var sortOrder = await _projects.NextRequirementSortOrderAsync(projectId, cancellationToken);
        var requirement = new ProjectRequirement
        {
            RequirementId = Guid.NewGuid(),
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Category = EnumParser.Parse<RequirementCategory>(request.Category, "Category"),
            Priority = EnumParser.ParsePriority(request.Priority),
            Source = string.IsNullOrWhiteSpace(request.Source)
                ? RequirementSource.Manual
                : EnumParser.Parse<RequirementSource>(request.Source, "Source"),
            Status = RequirementStatus.Approved,
            SortOrder = sortOrder + 1,
            CreatedBy = actorId
        };

        await _projects.AddRequirementAsync(requirement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ProjectMapper.ToRequirement(requirement);
    }

    public Task<PlanProjectResponse> ApproveTaskPlanAsync(Guid projectId, Guid reviewerId, string? comments, CancellationToken cancellationToken) =>
        ReviewAsync(projectId, reviewerId, comments, approve: true, cancellationToken);

    public Task<PlanProjectResponse> RejectTaskPlanAsync(Guid projectId, Guid reviewerId, string? comments, CancellationToken cancellationToken) =>
        ReviewAsync(projectId, reviewerId, comments, approve: false, cancellationToken);

    public async Task<ProjectReportDto> GetReportAsync(Guid projectId, Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        var project = await _projects.GetDetailAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");
        EnsureCanRead(project, userId, roles);

        var results = await _revit.GetProjectResultsAsync(projectId, cancellationToken);
        var approvals = await _revit.GetApprovalsAsync(projectId, cancellationToken);
        var revisions = await _revit.GetRevisionsAsync(projectId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var openIssues = project.Issues.Count(issue => issue.Status is IssueStatus.Open or IssueStatus.Assigned or IssueStatus.InProgress);
        var overdue = project.Tasks.Count(task =>
            task.Status is WorkTaskStatus.ToDo or WorkTaskStatus.InProgress or WorkTaskStatus.InReview
            && task.DueDate is DateOnly due
            && due < today);
        var passed = results.Count(result => result.Status == ValidationResultStatus.Pass);
        var failed = results.Count(result => result.Status == ValidationResultStatus.Fail);
        var total = passed + failed;
        var passRate = total == 0 ? 0m : Math.Round(100m * passed / total, 2);

        return new ProjectReportDto(
            project.ProjectId,
            project.Code,
            project.Name,
            project.Status.ToString(),
            openIssues,
            overdue,
            passRate,
            Ai.ValidationSummaryBuilder.Label,
            Ai.ValidationSummaryBuilder.Build(project.Name, results),
            results.Select(ProjectMapper.ToStoredCheck).ToList(),
            approvals.Select(ProjectMapper.ToApproval).ToList(),
            revisions.Select(ProjectMapper.ToRevision).ToList());
    }

    private async Task<PlanProjectResponse> ReviewAsync(
        Guid projectId,
        Guid reviewerId,
        string? comments,
        bool approve,
        CancellationToken cancellationToken)
    {
        var roles = await _users.GetRoleNamesAsync(reviewerId, cancellationToken);
        if (!RoleNames.IsManager(roles))
        {
            throw new ForbiddenException("Only an Admin or ProjectManager can review proposed tasks.");
        }

        var project = await _projects.GetTrackedAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");
        var tasks = await _projects.GetTrackedProposedAiTasksAsync(projectId, cancellationToken);
        var utcNow = DateTime.UtcNow;
        var count = approve
            ? Workflows.TaskPlanWorkflow.Approve(tasks, reviewerId, utcNow)
            : Workflows.TaskPlanWorkflow.Reject(tasks, utcNow);
        if (count == 0)
        {
            throw new ConflictException("This project has no proposed AI tasks to review.");
        }

        var pending = await _projects.GetPendingTaskPlanAsync(projectId, cancellationToken);
        Workflows.TaskPlanWorkflow.MarkApproval(
            pending,
            approve ? ApprovalStatus.Approved : ApprovalStatus.Rejected,
            reviewerId,
            comments,
            utcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var status = approve ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
        return new PlanProjectResponse(project.ProjectId, status.ToString(), tasks.Select(ProjectMapper.ToTask).ToList());
    }

    private async Task EnsureUserExists(Guid? userId, string label, CancellationToken cancellationToken)
    {
        if (userId is Guid id && await _users.FindByIdAsync(id, cancellationToken) is null)
        {
            throw new NotFoundException($"{label} was not found.");
        }
    }

    internal static void EnsureCanRead(Project project, Guid userId, IReadOnlyCollection<string> roles)
    {
        if (RoleNames.IsClientOnly(roles) && project.Client.PortalUserId != userId)
        {
            throw new NotFoundException("Project was not found.");
        }
    }
}
