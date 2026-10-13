using DesignFlow.Application.Contracts;

namespace DesignFlow.Application.Ai;

/// <summary>
/// Answers firm questions from a snapshot loaded by internal services.
/// If the question is outside that snapshot, it says so instead of guessing.
/// </summary>
public static class BusinessAnswerBuilder
{
    public static string Answer(string question, BusinessSnapshot snapshot)
    {
        var text = question.ToLowerInvariant();

        if (text.Contains("overdue", StringComparison.Ordinal))
        {
            if (snapshot.OverdueTasks.Count == 0)
            {
                return "No active tasks are overdue.";
            }

            var lines = snapshot.OverdueTasks.Select(task =>
                $"- {task.ProjectCode}: {task.Title} (due {task.DueDate:yyyy-MM-dd}, {task.AssigneeName ?? "unassigned"})");
            return "Overdue tasks from the project database:" + Environment.NewLine + string.Join(Environment.NewLine, lines);
        }

        if (text.Contains("issue", StringComparison.Ordinal) || text.Contains("validation", StringComparison.Ordinal) && text.Contains("open", StringComparison.Ordinal))
        {
            if (text.Contains("pass rate", StringComparison.Ordinal) || text.Contains("pass-rate", StringComparison.Ordinal))
            {
                return PassRate(snapshot);
            }

            if (snapshot.OpenIssues.Count == 0)
            {
                return "There are no open validation issues.";
            }

            var lines = snapshot.OpenIssues.Select(issue =>
                $"- {issue.ProjectCode}: {issue.Title} ({issue.Severity}, {issue.Status})");
            return "Open issues from the project database:" + Environment.NewLine + string.Join(Environment.NewLine, lines);
        }

        if (text.Contains("pass rate", StringComparison.Ordinal) || text.Contains("pass-rate", StringComparison.Ordinal))
        {
            return PassRate(snapshot);
        }

        if (text.Contains("project", StringComparison.Ordinal) && (text.Contains("status", StringComparison.Ordinal) || text.Contains("attention", StringComparison.Ordinal) || text.Contains("active", StringComparison.Ordinal)))
        {
            if (snapshot.Projects.Count == 0)
            {
                return "No projects are stored.";
            }

            var lines = snapshot.Projects.Select(project => $"- {project.Code}: {project.Name} ({project.Status})");
            return "Project status from the project database:" + Environment.NewLine + string.Join(Environment.NewLine, lines);
        }

        return "I can answer from live project data only: overdue tasks, open issues, project status, and the validation pass rate. Ask one of those. I will not guess.";
    }

    private static string PassRate(BusinessSnapshot snapshot) =>
        $"Validation pass rate is {snapshot.PassRatePercent:0.##}% from stored runs ({snapshot.ChecksPassed} passed, {snapshot.ChecksFailed} failed).";
}
