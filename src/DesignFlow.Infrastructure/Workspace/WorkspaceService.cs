using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Common;
using DesignFlow.Application.Contracts;
using DesignFlow.Application.Services;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;
using DesignFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DesignFlow.Infrastructure.Workspace;

/// <summary>Reads and writes the extra records the studio UI needs: people, alerts, jobs, and the task board.</summary>
public sealed class WorkspaceService : IWorkspaceService
{
    private static readonly HashSet<WorkTaskStatus> BoardStatuses = new()
    {
        WorkTaskStatus.ToDo,
        WorkTaskStatus.InProgress,
        WorkTaskStatus.InReview,
        WorkTaskStatus.Done,
        WorkTaskStatus.Cancelled
    };

    private readonly DesignFlowDbContext _db;

    public WorkspaceService(DesignFlowDbContext db) => _db = db;

    public async Task<IReadOnlyList<TeamMemberDto>> ListUsersAsync(CancellationToken cancellationToken)
    {
        var users = await _db.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
            .ThenInclude(link => link.Role)
            .Where(user => user.IsActive)
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .ToListAsync(cancellationToken);

        return users
            .Select(user => new TeamMemberDto(
                user.UserId,
                user.FullName,
                user.Email,
                user.UserRoles.Select(link => link.Role.Name).OrderBy(name => name).ToList()))
            .ToList();
    }

    public async Task<IReadOnlyList<NotificationDto>> ListNotificationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var rows = await _db.Notifications
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .Take(40)
            .ToListAsync(cancellationToken);

        return rows
            .Select(item => new NotificationDto(
                item.NotificationId,
                item.Title,
                item.Message,
                item.Type.ToString(),
                item.IsRead,
                item.ProjectId,
                item.CreatedAt))
            .ToList();
    }

    public async Task MarkNotificationReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken)
    {
        var row = await _db.Notifications.FirstOrDefaultAsync(
            item => item.NotificationId == notificationId && item.UserId == userId,
            cancellationToken) ?? throw new NotFoundException("Notification was not found.");
        row.IsRead = true;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AutomationJobDto>> ListJobsAsync(CancellationToken cancellationToken)
    {
        var jobs = await _db.AutomationJobs
            .AsNoTracking()
            .OrderByDescending(job => job.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);
        return jobs.Select(ProjectMapper.ToJob).ToList();
    }

    public async Task<TaskDto> CreateTaskAsync(Guid projectId, CreateTaskRequest request, Guid actorId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new AppValidationException("Title is required.");
        }

        var exists = await _db.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException("Project was not found.");
        }

        var task = new ProjectTask
        {
            TaskId = Guid.NewGuid(),
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = WorkTaskStatus.ToDo,
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? PriorityLevel.Medium : EnumParser.ParsePriority(request.Priority),
            Source = TaskSource.Manual,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate,
            CreatedBy = actorId
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.Entry(task).Reference(item => item.Assignee).LoadAsync(cancellationToken);
        return ProjectMapper.ToTask(task);
    }

    public async Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, string status, CancellationToken cancellationToken)
    {
        var next = EnumParser.Parse<WorkTaskStatus>(status, "Status");
        if (!BoardStatuses.Contains(next))
        {
            throw new AppValidationException("Proposed tasks stay off the board until a manager approves the plan.");
        }

        var task = await _db.Tasks
            .Include(item => item.Assignee)
            .FirstOrDefaultAsync(item => item.TaskId == taskId, cancellationToken)
            ?? throw new NotFoundException("Task was not found.");

        if (task.Status is WorkTaskStatus.Proposed or WorkTaskStatus.Rejected)
        {
            throw new AppValidationException("This task is waiting on plan approval and cannot be moved yet.");
        }

        task.Status = next;
        await _db.SaveChangesAsync(cancellationToken);
        return ProjectMapper.ToTask(task);
    }
}
