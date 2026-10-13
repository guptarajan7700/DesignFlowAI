using DesignFlow.Application.Ai;
using DesignFlow.Application.Workflows;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Tests;

public class WorkflowAndAiTests
{
    [Fact]
    public void Failed_validation_assigns_the_designer_and_flags_the_project()
    {
        var designerId = Guid.NewGuid();
        var project = new Project
        {
            ProjectId = Guid.NewGuid(),
            Status = ProjectStatus.Active,
            LeadDesignerId = designerId,
            ProjectManagerId = Guid.NewGuid()
        };
        var run = new ValidationRun
        {
            ValidationRunId = Guid.NewGuid(),
            RevitModelId = Guid.NewGuid(),
            ProjectId = project.ProjectId,
            Status = ValidationRunStatus.Failed
        };
        var failed = new List<ValidationResult>
        {
            new()
            {
                ValidationResultId = Guid.NewGuid(),
                CheckCode = "DoorsMissingFireRating",
                CheckName = "Doors missing fire rating",
                Status = ValidationResultStatus.Fail,
                ElementId = "345221",
                Message = "Door 345221 has an empty Fire Rating parameter."
            }
        };

        var outcome = ValidationFailureWorkflow.Apply(project, run, failed, Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(ProjectStatus.NeedsAttention, project.Status);
        var issue = Assert.Single(outcome.Issues);
        Assert.Equal(IssueStatus.Assigned, issue.Status);
        Assert.Equal(designerId, issue.AssignedToId);
        Assert.Equal(IssueSeverity.High, issue.Severity);
        Assert.Equal("345221", issue.ElementId);
        Assert.NotNull(outcome.Notification);
        Assert.Equal(designerId, outcome.Notification!.UserId);
        Assert.Equal(NotificationType.ValidationFailed, outcome.Notification.Type);
    }

    [Fact]
    public void Passed_validation_does_not_open_issues()
    {
        var project = new Project { ProjectId = Guid.NewGuid(), Status = ProjectStatus.Active };
        var run = new ValidationRun { ValidationRunId = Guid.NewGuid(), Status = ValidationRunStatus.Passed };

        var outcome = ValidationFailureWorkflow.Apply(project, run, Array.Empty<ValidationResult>(), null, DateTime.UtcNow);

        Assert.Empty(outcome.Issues);
        Assert.Null(outcome.Notification);
        Assert.Equal(ProjectStatus.Active, project.Status);
    }

    [Fact]
    public void Proposed_ai_tasks_become_todo_only_when_approved()
    {
        var reviewer = Guid.NewGuid();
        var tasks = new List<ProjectTask>
        {
            new() { Title = "Model kitchen", Status = WorkTaskStatus.Proposed, Source = TaskSource.AiProposed },
            new() { Title = "Manual task", Status = WorkTaskStatus.ToDo, Source = TaskSource.Manual }
        };

        var approved = TaskPlanWorkflow.Approve(tasks, reviewer, DateTime.UtcNow);

        Assert.Equal(1, approved);
        Assert.Equal(WorkTaskStatus.ToDo, tasks[0].Status);
        Assert.Equal(reviewer, tasks[0].ApprovedBy);
        Assert.Equal(WorkTaskStatus.ToDo, tasks[1].Status);
        Assert.Null(tasks[1].ApprovedBy);
    }

    [Fact]
    public void Summary_uses_only_stored_failures()
    {
        var results = new List<ValidationResult>
        {
            new()
            {
                CheckName = "Doors missing fire rating",
                Status = ValidationResultStatus.Fail,
                ElementId = "345221",
                Message = "Door 345221 has an empty Fire Rating parameter.",
                SortOrder = 1
            },
            new()
            {
                CheckName = "Unplaced rooms",
                Status = ValidationResultStatus.Pass,
                Message = "No unplaced rooms were found.",
                SortOrder = 2
            }
        };

        var summary = ValidationSummaryBuilder.Build("Sharma Residence", results);

        Assert.Contains("AI-generated from validation data", summary);
        Assert.Contains("345221", summary);
        Assert.Contains("1 failed", summary);
        Assert.DoesNotContain("999999", summary);
    }

    [Fact]
    public void Command_mapper_stays_on_the_whitelist()
    {
        var match = RevitCommandMapper.Match("Find doors missing a fire-rating parameter");
        Assert.NotNull(match);
        Assert.Equal("FindDoorsMissingFireRating", match!.Command);

        Assert.Null(RevitCommandMapper.Match("Delete every wall on level 1"));
    }

    [Fact]
    public void Business_answer_uses_the_snapshot_and_does_not_guess()
    {
        var snapshot = new Application.Contracts.BusinessSnapshot(
            new[] { new Application.Contracts.OverdueTaskItem("HB-LOFT-014", "Harbor cove lighting layout", new DateOnly(2026, 10, 8), "Lena Ortiz") },
            Array.Empty<Application.Contracts.OpenIssueItem>(),
            Array.Empty<Application.Contracts.ProjectStatusItem>(),
            75m,
            6,
            2);

        var answer = BusinessAnswerBuilder.Answer("Which projects have overdue tasks?", snapshot);
        var unknown = BusinessAnswerBuilder.Answer("What is the capital of France?", snapshot);

        Assert.Contains("HB-LOFT-014", answer);
        Assert.Contains("I will not guess", unknown);
        Assert.DoesNotContain("Paris", unknown);
    }

    [Fact]
    public void Brief_analyzer_reads_a_2bhk_kitchen_without_inventing_a_budget()
    {
        var analysis = BriefAnalyzer.Analyze("modern 2BHK with modular kitchen and storage");

        Assert.Contains("Kitchen", analysis.Rooms);
        Assert.Contains("Master bedroom", analysis.Rooms);
        Assert.Equal("Modern", analysis.Style);
        Assert.Null(analysis.Budget);
        Assert.Contains(analysis.SuggestedTasks, task => task.Title.Contains("kitchen", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(analysis.ReviewChecklist, item => item.Contains("fire-rating", StringComparison.OrdinalIgnoreCase));
    }
}
