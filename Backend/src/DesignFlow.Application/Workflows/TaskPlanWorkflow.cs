using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Workflows;

/// <summary>
/// AI task plans stay Proposed until a manager approves them. Approval is the only path to ToDo.
/// </summary>
public static class TaskPlanWorkflow
{
    public static int Approve(IEnumerable<ProjectTask> tasks, Guid reviewerId, DateTime utcNow)
    {
        var count = 0;
        foreach (var task in tasks)
        {
            if (task.Status != WorkTaskStatus.Proposed || task.Source != TaskSource.AiProposed)
            {
                continue;
            }

            task.Status = WorkTaskStatus.ToDo;
            task.ApprovedBy = reviewerId;
            task.ApprovedAt = utcNow;
            task.UpdatedAt = utcNow;
            count++;
        }

        return count;
    }

    public static int Reject(IEnumerable<ProjectTask> tasks, DateTime utcNow)
    {
        var count = 0;
        foreach (var task in tasks)
        {
            if (task.Status != WorkTaskStatus.Proposed || task.Source != TaskSource.AiProposed)
            {
                continue;
            }

            task.Status = WorkTaskStatus.Rejected;
            task.UpdatedAt = utcNow;
            count++;
        }

        return count;
    }

    public static void MarkApproval(Approval? pending, ApprovalStatus status, Guid reviewerId, string? comments, DateTime utcNow)
    {
        if (pending is null)
        {
            return;
        }

        pending.Status = status;
        pending.ReviewedById = reviewerId;
        pending.ReviewedAt = utcNow;
        pending.Comments = string.IsNullOrWhiteSpace(comments) ? pending.Comments : comments;
        pending.UpdatedAt = utcNow;
    }
}
