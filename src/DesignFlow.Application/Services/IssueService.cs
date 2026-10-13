using DesignFlow.Application.Abstractions;
using DesignFlow.Application.Common;
using DesignFlow.Application.Contracts;
using DesignFlow.Domain;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Services;

public sealed class IssueService : IIssueService
{
    private readonly IIssueRepository _issues;
    private readonly IUserRepository _users;
    private readonly INotificationRepository _notifications;
    private readonly IAuditRepository _audit;
    private readonly IUnitOfWork _unitOfWork;

    public IssueService(
        IIssueRepository issues,
        IUserRepository users,
        INotificationRepository notifications,
        IAuditRepository audit,
        IUnitOfWork unitOfWork)
    {
        _issues = issues;
        _users = users;
        _notifications = notifications;
        _audit = audit;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<IssueDto>> ListAsync(
        Guid? projectId,
        string? severity,
        string? status,
        Guid userId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(severity))
        {
            EnumParser.Parse<IssueSeverity>(severity, "Severity");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            EnumParser.Parse<IssueStatus>(status, "Status");
        }

        var portalUserId = RoleNames.IsClientOnly(roles) ? userId : (Guid?)null;
        return _issues.ListAsync(projectId, severity, status, portalUserId, cancellationToken);
    }

    public async Task<IssueDto> AssignAsync(Guid issueId, Guid assigneeId, Guid actorId, CancellationToken cancellationToken)
    {
        var issue = await _issues.GetTrackedAsync(issueId, cancellationToken)
            ?? throw new NotFoundException("Issue was not found.");
        var assignee = await _users.FindByIdAsync(assigneeId, cancellationToken)
            ?? throw new NotFoundException("Assignee was not found.");
        if (!assignee.IsActive)
        {
            throw new AppValidationException("Assignee is not an active user.");
        }

        issue.AssignedToId = assignee.UserId;
        issue.AssignedAt = DateTime.UtcNow;
        if (issue.Status is IssueStatus.Open)
        {
            issue.Status = IssueStatus.Assigned;
        }

        await _notifications.AddAsync(new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = assignee.UserId,
            ProjectId = issue.ProjectId,
            Title = "Issue assigned",
            Message = issue.Title,
            Type = NotificationType.IssueAssigned,
            RelatedEntityType = "Issue",
            RelatedEntityId = issue.IssueId,
            CreatedBy = actorId
        }, cancellationToken);
        await _audit.AddAsync(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            UserId = actorId,
            EntityType = "Issue",
            EntityId = issue.IssueId,
            Action = "Assigned",
            NewValues = $"{{\"assigneeId\":\"{assignee.UserId}\"}}",
            CreatedBy = actorId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new IssueDto(
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
            assignee.FullName,
            issue.ValidationRunId,
            issue.RevitModelId,
            issue.CreatedAt);
    }
}
