namespace DesignFlow.Domain.Enums;

public enum ClientStatus
{
    Prospect,
    Active,
    Inactive
}

public enum ProjectStatus
{
    Draft,
    Active,
    OnHold,
    InReview,
    NeedsAttention,
    Completed,
    Archived
}

public enum RequirementCategory
{
    Room,
    Style,
    Budget,
    Constraint,
    Material,
    Storage,
    General
}

public enum PriorityLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum RequirementSource
{
    Manual,
    AiProposed,
    ClientBrief
}

public enum RequirementStatus
{
    Proposed,
    Approved,
    Rejected,
    Implemented
}

public enum MilestoneStatus
{
    Pending,
    InProgress,
    Completed,
    Overdue
}

public enum WorkTaskStatus
{
    Proposed,
    ToDo,
    InProgress,
    InReview,
    Done,
    Rejected,
    Cancelled
}

public enum TaskSource
{
    Manual,
    AiProposed
}

public enum IssueSeverity
{
    Critical,
    High,
    Medium,
    Low
}

public enum IssueStatus
{
    Open,
    Assigned,
    InProgress,
    Resolved,
    Closed,
    WontFix
}

public enum RevitModelStatus
{
    Draft,
    Synced,
    Translating,
    Ready,
    Failed
}

public enum ValidationRunStatus
{
    Queued,
    Running,
    Passed,
    Failed,
    Error
}

public enum ValidationResultStatus
{
    Pass,
    Fail,
    Warning,
    Skipped
}

public enum AutomationJobStatus
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Cancelled
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public enum ApprovalEntityType
{
    TaskPlan,
    DrawingRevision,
    Requirement,
    Report
}

public enum AiCommandStatus
{
    Previewed,
    Confirmed,
    Executed,
    Cancelled,
    Failed
}

public enum NotificationType
{
    ValidationFailed,
    TaskAssigned,
    ApprovalRequested,
    IssueAssigned,
    Info
}
