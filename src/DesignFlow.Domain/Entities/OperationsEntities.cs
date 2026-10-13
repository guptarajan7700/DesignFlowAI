using DesignFlow.Domain.Enums;

namespace DesignFlow.Domain.Entities;

public class AutomationJob : AuditableEntity
{
    public Guid AutomationJobId { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? RevitModelId { get; set; }

    public string JobType { get; set; } = string.Empty;

    public AutomationJobStatus Status { get; set; } = AutomationJobStatus.Queued;

    public byte ProgressPercent { get; set; }

    public string? ExternalWorkItemId { get; set; }

    public string? RequestPayload { get; set; }

    public string? ResultPayload { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Project? Project { get; set; }

    public RevitModel? RevitModel { get; set; }
}

public class DrawingRevision : AuditableEntity
{
    public Guid DrawingRevisionId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? RevitModelId { get; set; }

    public string SheetNumber { get; set; } = string.Empty;

    public string SheetName { get; set; } = string.Empty;

    public string RevisionCode { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? IssuedAt { get; set; }

    public Guid? IssuedById { get; set; }

    public Project Project { get; set; } = null!;
}

public class Approval : AuditableEntity
{
    public Guid ApprovalId { get; set; }

    public Guid ProjectId { get; set; }

    public ApprovalEntityType EntityType { get; set; }

    public Guid EntityId { get; set; }

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public Guid RequestedById { get; set; }

    public Guid? ReviewedById { get; set; }

    public string? Comments { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Project Project { get; set; } = null!;
}

public class AiCommandLog : AuditableEntity
{
    public Guid AiCommandLogId { get; set; }

    public Guid UserId { get; set; }

    public Guid? ProjectId { get; set; }

    public string Prompt { get; set; } = string.Empty;

    public string? DetectedCommand { get; set; }

    public string? ParametersJson { get; set; }

    public string? PreviewJson { get; set; }

    public AiCommandStatus Status { get; set; } = AiCommandStatus.Previewed;

    public string Provider { get; set; } = "Mock";

    public string? ResponseJson { get; set; }

    public User User { get; set; } = null!;

    public Project? Project { get; set; }
}

public class AuditLog : AuditableEntity
{
    public Guid AuditLogId { get; set; }

    public Guid? UserId { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }
}

public class Notification : AuditableEntity
{
    public Guid NotificationId { get; set; }

    public Guid UserId { get; set; }

    public Guid? ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.Info;

    public bool IsRead { get; set; }

    public string? RelatedEntityType { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public User User { get; set; } = null!;

    public Project? Project { get; set; }
}
