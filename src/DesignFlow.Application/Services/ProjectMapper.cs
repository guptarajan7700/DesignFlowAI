using DesignFlow.Application.Contracts;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Services;

public static class ProjectMapper
{
    public static ProjectDetailDto ToDetail(Project project) => new(
        project.ProjectId,
        project.ClientId,
        project.Code,
        project.Name,
        project.Description,
        project.Status.ToString(),
        project.StartDate,
        project.DueDate,
        project.Budget,
        project.Spent,
        project.Currency,
        project.SiteAddress,
        project.Client.CompanyName,
        project.ProjectManager?.FullName,
        project.LeadDesigner?.FullName,
        project.LeadDesignerId,
        project.Requirements.OrderBy(item => item.SortOrder).Select(ToRequirement).ToList(),
        project.Milestones.OrderBy(item => item.SortOrder).Select(item => new MilestoneDto(
            item.MilestoneId, item.Name, item.Description, item.DueDate, item.Status.ToString(), item.SortOrder)).ToList(),
        project.Tasks.OrderBy(item => item.CreatedAt).Select(ToTask).ToList(),
        project.Models.OrderBy(item => item.Name).Select(item => new RevitModelDto(
            item.RevitModelId, item.Name, item.FileName, item.VersionLabel, item.AutodeskUrn, item.LastSyncedAt, item.Status.ToString())).ToList());

    public static RequirementDto ToRequirement(ProjectRequirement item) => new(
        item.RequirementId,
        item.Title,
        item.Description,
        item.Category.ToString(),
        item.Priority.ToString(),
        item.Source.ToString(),
        item.Status.ToString(),
        item.SortOrder);

    public static TaskDto ToTask(ProjectTask item) => new(
        item.TaskId,
        item.Title,
        item.Description,
        item.Status.ToString(),
        item.Priority.ToString(),
        item.Source.ToString(),
        item.AssigneeId,
        item.Assignee?.FullName,
        item.DueDate,
        item.Labels,
        item.ApprovedAt);

    public static StoredCheckDto ToStoredCheck(ValidationResult result) => new(
        result.CheckCode,
        result.CheckName,
        result.Status.ToString(),
        result.ElementId,
        result.Message,
        result.SuggestedFix);

    public static ApprovalDto ToApproval(Approval approval) => new(
        approval.ApprovalId,
        approval.EntityType.ToString(),
        approval.Status.ToString(),
        approval.Comments,
        approval.RequestedAt,
        approval.ReviewedAt);

    public static DrawingRevisionDto ToRevision(DrawingRevision revision) => new(
        revision.DrawingRevisionId,
        revision.SheetNumber,
        revision.SheetName,
        revision.RevisionCode,
        revision.Description,
        revision.IssuedAt);

    public static AutomationJobDto ToJob(AutomationJob job) => new(
        job.AutomationJobId,
        job.ProjectId,
        job.RevitModelId,
        job.JobType,
        job.Status.ToString(),
        job.ProgressPercent,
        job.ErrorMessage,
        job.ResultPayload,
        job.StartedAt,
        job.CompletedAt,
        job.CreatedAt);

    public static RevitModelDto ToModel(RevitModel model) => new(
        model.RevitModelId,
        model.Name,
        model.FileName,
        model.VersionLabel,
        model.AutodeskUrn,
        model.LastSyncedAt,
        model.Status.ToString());
}
