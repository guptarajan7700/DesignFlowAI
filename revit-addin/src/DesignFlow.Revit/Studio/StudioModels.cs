namespace DesignFlow.Revit.Studio;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
}

public sealed class ProjectListItem
{
    public Guid ProjectId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public sealed class RevitModelResponse
{
    public Guid RevitModelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public sealed class RequirementItem
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public sealed class ProjectDetailResponse
{
    public Guid ProjectId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<RequirementItem> Requirements { get; set; } = new();
}

public sealed class CheckResult
{
    public string CheckCode { get; set; } = string.Empty;

    public string CheckName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? ElementId { get; set; }

    public string? ElementCategory { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? SuggestedFix { get; set; }
}

public sealed class ValidationRunResponse
{
    public Guid ValidationRunId { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ChecksPassed { get; set; }

    public int ChecksFailed { get; set; }

    public int IssuesCreated { get; set; }
}

public sealed class AutomationJobResponse
{
    public Guid AutomationJobId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;
}
