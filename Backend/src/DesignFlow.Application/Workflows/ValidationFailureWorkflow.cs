using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Workflows;

public sealed record ValidationFailureOutcome(
    IReadOnlyList<Issue> Issues,
    Notification? Notification,
    AuditLog Audit,
    bool ProjectFlagged);

/// <summary>
/// A failed validation run opens an issue per failed check, assigns the lead designer,
/// moves the project to NeedsAttention, and notifies that designer or the project manager.
/// </summary>
public static class ValidationFailureWorkflow
{
    public static ValidationFailureOutcome Apply(
        Project project,
        ValidationRun run,
        IReadOnlyList<ValidationResult> failedResults,
        Guid? actorUserId,
        DateTime utcNow)
    {
        if (run.Status != ValidationRunStatus.Failed)
        {
            return new ValidationFailureOutcome(Array.Empty<Issue>(), null, EmptyAudit(run, actorUserId, utcNow, 0), false);
        }

        var designerId = project.LeadDesignerId;
        var issues = failedResults.Select(result => new Issue
        {
            IssueId = Guid.NewGuid(),
            ProjectId = project.ProjectId,
            ValidationRunId = run.ValidationRunId,
            ValidationResultId = result.ValidationResultId,
            RevitModelId = run.RevitModelId,
            Title = result.CheckName,
            Description = result.Message,
            Severity = SeverityFor(result.CheckCode),
            Status = designerId is null ? IssueStatus.Open : IssueStatus.Assigned,
            ElementId = result.ElementId,
            SuggestedFix = result.SuggestedFix,
            AssignedToId = designerId,
            AssignedAt = designerId is null ? null : utcNow,
            CreatedBy = actorUserId
        }).ToList();

        var flagged = false;
        if (project.Status is not ProjectStatus.Completed and not ProjectStatus.Archived)
        {
            project.Status = ProjectStatus.NeedsAttention;
            project.UpdatedAt = utcNow;
            flagged = true;
        }

        var notifyUserId = designerId ?? project.ProjectManagerId;
        Notification? notification = null;
        if (notifyUserId is Guid userId)
        {
            notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                ProjectId = project.ProjectId,
                Title = "Validation failed",
                Message = $"A model validation run failed. {issues.Count} new issue(s) were opened and assigned for review.",
                Type = NotificationType.ValidationFailed,
                IsRead = false,
                RelatedEntityType = "ValidationRun",
                RelatedEntityId = run.ValidationRunId,
                CreatedBy = actorUserId
            };
        }

        return new ValidationFailureOutcome(issues, notification, EmptyAudit(run, actorUserId, utcNow, issues.Count), flagged);
    }

    public static IssueSeverity SeverityFor(string checkCode) => checkCode switch
    {
        "DoorsMissingFireRating" => IssueSeverity.High,
        "UnplacedRooms" => IssueSeverity.Medium,
        "RoomsWithoutNames" => IssueSeverity.Medium,
        "MissingSheetParameters" => IssueSeverity.Low,
        _ => IssueSeverity.Medium
    };

    private static AuditLog EmptyAudit(ValidationRun run, Guid? actorUserId, DateTime utcNow, int issuesCreated) =>
        new()
        {
            AuditLogId = Guid.NewGuid(),
            UserId = actorUserId,
            EntityType = "ValidationRun",
            EntityId = run.ValidationRunId,
            Action = "CreateIssuesFromFailure",
            NewValues = $"{{\"issuesCreated\":{issuesCreated}}}",
            CreatedBy = actorUserId,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
}
