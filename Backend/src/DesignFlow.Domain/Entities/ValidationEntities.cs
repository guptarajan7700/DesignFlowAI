using DesignFlow.Domain.Enums;

namespace DesignFlow.Domain.Entities;

public class RevitModel : AuditableEntity
{
    public Guid RevitModelId { get; set; }

    public Guid ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? VersionLabel { get; set; }

    public string? AutodeskUrn { get; set; }

    public string? AutodeskProjectId { get; set; }

    public string? AutodeskItemId { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public RevitModelStatus Status { get; set; } = RevitModelStatus.Draft;

    public Project Project { get; set; } = null!;

    public ICollection<ValidationRun> ValidationRuns { get; set; } = new List<ValidationRun>();
}

public class ValidationRun : AuditableEntity
{
    public Guid ValidationRunId { get; set; }

    public Guid RevitModelId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? TriggeredById { get; set; }

    public ValidationRunStatus Status { get; set; } = ValidationRunStatus.Queued;

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Summary { get; set; }

    public int ChecksPassed { get; set; }

    public int ChecksFailed { get; set; }

    public RevitModel Model { get; set; } = null!;

    public Project Project { get; set; } = null!;

    public User? TriggeredBy { get; set; }

    public ICollection<ValidationResult> Results { get; set; } = new List<ValidationResult>();
}

public class ValidationResult : AuditableEntity
{
    public Guid ValidationResultId { get; set; }

    public Guid ValidationRunId { get; set; }

    public string CheckCode { get; set; } = string.Empty;

    public string CheckName { get; set; } = string.Empty;

    public ValidationResultStatus Status { get; set; }

    public string? ElementId { get; set; }

    public string? ElementCategory { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? SuggestedFix { get; set; }

    public int SortOrder { get; set; }

    public ValidationRun ValidationRun { get; set; } = null!;
}

public class Issue : AuditableEntity
{
    public Guid IssueId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? ValidationRunId { get; set; }

    public Guid? ValidationResultId { get; set; }

    public Guid? RevitModelId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public IssueSeverity Severity { get; set; } = IssueSeverity.Medium;

    public IssueStatus Status { get; set; } = IssueStatus.Open;

    public string? ElementId { get; set; }

    public string? SuggestedFix { get; set; }

    public Guid? AssignedToId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public Project Project { get; set; } = null!;

    public ValidationRun? ValidationRun { get; set; }

    public ValidationResult? ValidationResult { get; set; }

    public RevitModel? RevitModel { get; set; }

    public User? AssignedTo { get; set; }
}
