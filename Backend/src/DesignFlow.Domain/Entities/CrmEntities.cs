using DesignFlow.Domain.Enums;

namespace DesignFlow.Domain.Entities;

public class Client : AuditableEntity
{
    public Guid ClientId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public string? AddressLine1 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public ClientStatus Status { get; set; } = ClientStatus.Prospect;

    public string? Notes { get; set; }

    public Guid? PortalUserId { get; set; }

    public User? PortalUser { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}

public class Project : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public Guid ClientId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    public DateOnly? StartDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal Budget { get; set; }

    public decimal Spent { get; set; }

    public string Currency { get; set; } = "INR";

    public string? SiteAddress { get; set; }

    public string? CoverImageUrl { get; set; }

    public Guid? ProjectManagerId { get; set; }

    public Guid? LeadDesignerId { get; set; }

    public Client Client { get; set; } = null!;

    public User? ProjectManager { get; set; }

    public User? LeadDesigner { get; set; }

    public ICollection<ProjectRequirement> Requirements { get; set; } = new List<ProjectRequirement>();

    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();

    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

    public ICollection<Issue> Issues { get; set; } = new List<Issue>();

    public ICollection<RevitModel> Models { get; set; } = new List<RevitModel>();

    public ICollection<DrawingRevision> DrawingRevisions { get; set; } = new List<DrawingRevision>();

    public ICollection<Approval> Approvals { get; set; } = new List<Approval>();
}

public class ProjectRequirement : AuditableEntity
{
    public Guid RequirementId { get; set; }

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public RequirementCategory Category { get; set; } = RequirementCategory.General;

    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    public RequirementSource Source { get; set; } = RequirementSource.Manual;

    public RequirementStatus Status { get; set; } = RequirementStatus.Proposed;

    public string? AiAnalysisJson { get; set; }

    public int SortOrder { get; set; }

    public Project Project { get; set; } = null!;
}

public class Milestone : AuditableEntity
{
    public Guid MilestoneId { get; set; }

    public Guid ProjectId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    public int SortOrder { get; set; }

    public Project Project { get; set; } = null!;
}

public class ProjectTask : AuditableEntity
{
    public Guid TaskId { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? MilestoneId { get; set; }

    public Guid? RequirementId { get; set; }

    public Guid? ParentTaskId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public WorkTaskStatus Status { get; set; } = WorkTaskStatus.ToDo;

    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    public Guid? AssigneeId { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal? EstimatedHours { get; set; }

    public string? Labels { get; set; }

    public TaskSource Source { get; set; } = TaskSource.Manual;

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Project Project { get; set; } = null!;

    public Milestone? Milestone { get; set; }

    public ProjectRequirement? Requirement { get; set; }

    public User? Assignee { get; set; }
}
