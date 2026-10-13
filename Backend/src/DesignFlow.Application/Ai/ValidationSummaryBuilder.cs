using DesignFlow.Application.Contracts;
using DesignFlow.Domain.Entities;
using DesignFlow.Domain.Enums;

namespace DesignFlow.Application.Ai;

/// <summary>
/// Writes design-review text only from stored validation rows.
/// It does not add checks, element ids, or defects that are absent from <paramref name="results"/>.
/// </summary>
public static class ValidationSummaryBuilder
{
    public const string Label = "AI-generated from validation data";

    public static string Build(string projectName, IReadOnlyList<ValidationResult> results)
    {
        if (results.Count == 0)
        {
            return $"{Label}. No validation results are stored for {projectName}. No defects are reported because none have been inspected.";
        }

        var failed = results.Where(result => result.Status == ValidationResultStatus.Fail).ToList();
        var passed = results.Count(result => result.Status == ValidationResultStatus.Pass);
        var warnings = results.Count(result => result.Status == ValidationResultStatus.Warning);

        var lines = new List<string>
        {
            $"{Label} for {projectName}.",
            $"{passed} checks passed, {failed.Count} failed, {warnings} warnings. Counts use stored validation rows only."
        };

        if (failed.Count == 0)
        {
            lines.Add("Stored results do not contain a failed check.");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("Failed checks:");
        foreach (var result in failed.OrderBy(result => result.SortOrder))
        {
            var message = result.Message.Trim().TrimEnd('.', ' ');
            var element = string.IsNullOrWhiteSpace(result.ElementId) ? string.Empty : $" Element {result.ElementId}.";
            lines.Add($"- {result.CheckName}: {message}.{element}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
