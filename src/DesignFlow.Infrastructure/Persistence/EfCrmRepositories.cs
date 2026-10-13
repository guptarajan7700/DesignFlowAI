using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DesignFlow.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DesignFlowDbContext _db;

    public UnitOfWork(DesignFlowDbContext db) => _db = db;

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new Application.Common.ConflictException("A record with the same unique value already exists.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class EfUserRepository : IUserRepository
{
    private readonly DesignFlowDbContext _db;

    public EfUserRepository(DesignFlowDbContext db) => _db = db;

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> FindByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken) =>
        await _db.UserRoles.AsNoTracking()
            .Where(link => link.UserId == userId)
            .Select(link => link.Role.Name)
            .ToListAsync(cancellationToken);

    public async Task<Guid?> GetRoleIdAsync(string roleName, CancellationToken cancellationToken) =>
        await _db.Roles.AsNoTracking()
            .Where(role => role.Name == roleName)
            .Select(role => (Guid?)role.RoleId)
            .FirstOrDefaultAsync(cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken) =>
        _db.Users.AddAsync(user, cancellationToken).AsTask();

    public Task AddRoleAsync(UserRole userRole, CancellationToken cancellationToken) =>
        _db.UserRoles.AddAsync(userRole, cancellationToken).AsTask();
}

public sealed class EfClientRepository : IClientRepository
{
    private readonly DesignFlowDbContext _db;

    public EfClientRepository(DesignFlowDbContext db) => _db = db;

    public async Task<IReadOnlyList<ClientDto>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Clients.AsNoTracking()
            .OrderBy(client => client.CompanyName)
            .Select(client => new ClientDto(
                client.ClientId,
                client.CompanyName,
                client.ContactName,
                client.Email,
                client.Phone,
                client.City,
                client.Country,
                client.Status.ToString(),
                client.PortalUserId))
            .ToListAsync(cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        _db.Clients.AnyAsync(client => client.Email == email, cancellationToken);

    public Task<bool> ExistsAsync(Guid clientId, CancellationToken cancellationToken) =>
        _db.Clients.AnyAsync(client => client.ClientId == clientId, cancellationToken);

    public Task AddAsync(Client client, CancellationToken cancellationToken) =>
        _db.Clients.AddAsync(client, cancellationToken).AsTask();
}

public sealed class EfProjectRepository : IProjectRepository
{
    private readonly DesignFlowDbContext _db;

    public EfProjectRepository(DesignFlowDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProjectListDto>> ListAsync(Guid? portalUserId, DateOnly today, CancellationToken cancellationToken)
    {
        var query = _db.Projects.AsNoTracking().AsQueryable();
        if (portalUserId is Guid userId)
        {
            query = query.Where(project => project.Client.PortalUserId == userId);
        }

        return await query
            .OrderBy(project => project.Code)
            .Select(project => new ProjectListDto(
                project.ProjectId,
                project.Code,
                project.Name,
                project.Status.ToString(),
                project.Client.CompanyName,
                project.DueDate,
                project.Budget,
                project.Spent,
                project.Currency,
                project.ProjectManager == null ? null : project.ProjectManager.FirstName + " " + project.ProjectManager.LastName,
                project.LeadDesigner == null ? null : project.LeadDesigner.FirstName + " " + project.LeadDesigner.LastName,
                project.Issues.Count(issue => issue.Status == IssueStatus.Open || issue.Status == IssueStatus.Assigned || issue.Status == IssueStatus.InProgress),
                project.Tasks.Count(task =>
                    (task.Status == WorkTaskStatus.ToDo || task.Status == WorkTaskStatus.InProgress || task.Status == WorkTaskStatus.InReview)
                    && task.DueDate < today),
                project.Tasks.Count(task => task.Status == WorkTaskStatus.Proposed)))
            .ToListAsync(cancellationToken);
    }

    public Task<Project?> GetDetailAsync(Guid projectId, CancellationToken cancellationToken) =>
        DetailQuery().AsNoTracking().FirstOrDefaultAsync(project => project.ProjectId == projectId, cancellationToken);

    public Task<Project?> GetTrackedAsync(Guid projectId, CancellationToken cancellationToken) =>
        _db.Projects.Include(project => project.Client)
            .FirstOrDefaultAsync(project => project.ProjectId == projectId, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        _db.Projects.AnyAsync(project => project.Code == code, cancellationToken);

    public Task AddAsync(Project project, CancellationToken cancellationToken) =>
        _db.Projects.AddAsync(project, cancellationToken).AsTask();

    public Task AddRequirementAsync(ProjectRequirement requirement, CancellationToken cancellationToken) =>
        _db.ProjectRequirements.AddAsync(requirement, cancellationToken).AsTask();

    public async Task<int> NextRequirementSortOrderAsync(Guid projectId, CancellationToken cancellationToken) =>
        await _db.ProjectRequirements.Where(item => item.ProjectId == projectId)
            .Select(item => (int?)item.SortOrder)
            .MaxAsync(cancellationToken) ?? 0;

    public async Task<IReadOnlyList<ProjectTask>> GetTrackedProposedAiTasksAsync(Guid projectId, CancellationToken cancellationToken) =>
        await _db.Tasks
            .Where(task => task.ProjectId == projectId && task.Status == WorkTaskStatus.Proposed && task.Source == TaskSource.AiProposed)
            .ToListAsync(cancellationToken);

    public Task AddTasksAsync(IEnumerable<ProjectTask> tasks, CancellationToken cancellationToken)
    {
        _db.Tasks.AddRange(tasks);
        return Task.CompletedTask;
    }

    public Task<Approval?> GetPendingTaskPlanAsync(Guid projectId, CancellationToken cancellationToken) =>
        _db.Approvals.FirstOrDefaultAsync(
            approval => approval.ProjectId == projectId
                && approval.EntityType == ApprovalEntityType.TaskPlan
                && approval.Status == ApprovalStatus.Pending,
            cancellationToken);

    public Task AddApprovalAsync(Approval approval, CancellationToken cancellationToken) =>
        _db.Approvals.AddAsync(approval, cancellationToken).AsTask();

    public Task<bool> HasPendingTaskPlanAsync(Guid projectId, CancellationToken cancellationToken) =>
        _db.Approvals.AnyAsync(
            approval => approval.ProjectId == projectId
                && approval.EntityType == ApprovalEntityType.TaskPlan
                && approval.Status == ApprovalStatus.Pending,
            cancellationToken);

    private IQueryable<Project> DetailQuery() =>
        _db.Projects
            .Include(project => project.Client)
            .Include(project => project.ProjectManager)
            .Include(project => project.LeadDesigner)
            .Include(project => project.Requirements)
            .Include(project => project.Milestones)
            .Include(project => project.Tasks).ThenInclude(task => task.Assignee)
            .Include(project => project.Models)
            .Include(project => project.Issues);
}
