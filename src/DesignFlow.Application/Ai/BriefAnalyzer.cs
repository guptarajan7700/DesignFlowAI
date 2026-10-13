using System.Text.RegularExpressions;
using DesignFlow.Application.Contracts;

namespace DesignFlow.Application.Ai;

/// <summary>
/// Deterministic brief parser used when no model provider is configured,
/// and as the fallback when a provider call fails.
/// </summary>
public static class BriefAnalyzer
{
    public static RequirementAnalysisDto Analyze(string brief)
    {
        var text = brief.Trim();
        var lower = text.ToLowerInvariant();
        var rooms = new List<string>();

        var bhk = Regex.Match(lower, @"(\d+)\s*bhk");
        if (bhk.Success && int.TryParse(bhk.Groups[1].Value, out var bedrooms) && bedrooms is > 0 and <= 8)
        {
            rooms.Add("Living and dining");
            rooms.Add("Master bedroom");
            for (var index = 2; index <= bedrooms; index++)
            {
                rooms.Add($"Bedroom {index}");
            }

            rooms.Add("Kitchen");
            rooms.Add(bedrooms >= 3 ? "Bathrooms" : "Two bathrooms");
        }

        AddRoom(rooms, lower, "lobby", "Lobby");
        AddRoom(rooms, lower, "kitchen", "Kitchen");
        AddRoom(rooms, lower, "living", "Living");

        var style = "Unspecified";
        if (lower.Contains("modern", StringComparison.Ordinal))
        {
            style = "Modern";
        }
        else if (lower.Contains("classic", StringComparison.Ordinal))
        {
            style = "Classic";
        }

        if (lower.Contains("warm", StringComparison.Ordinal) && style != "Unspecified")
        {
            style = "Warm " + style;
        }

        var materials = new List<string>();
        if (lower.Contains("oak", StringComparison.Ordinal))
        {
            materials.Add("Oak");
        }

        if (lower.Contains("modular", StringComparison.Ordinal) && lower.Contains("kitchen", StringComparison.Ordinal))
        {
            materials.Add("Modular kitchen carcass");
        }

        var constraints = new List<string>();
        if (lower.Contains("storage", StringComparison.Ordinal))
        {
            constraints.Add("Provide full-height storage");
        }

        var tasks = new List<SuggestedTaskDto>();
        if (lower.Contains("kitchen", StringComparison.Ordinal))
        {
            tasks.Add(new SuggestedTaskDto { Title = "Model modular kitchen", Priority = "Critical", Label = "Kitchen" });
        }

        if (lower.Contains("storage", StringComparison.Ordinal))
        {
            tasks.Add(new SuggestedTaskDto { Title = "Place storage walls and wardrobes", Priority = "High", Label = "Storage" });
        }

        tasks.Add(new SuggestedTaskDto { Title = "Prepare concept boards", Priority = "Medium", Label = "Concept" });

        return new RequirementAnalysisDto
        {
            Rooms = rooms,
            Style = style,
            Budget = null,
            Constraints = constraints,
            Materials = materials,
            SuggestedTasks = tasks,
            ReviewChecklist = new List<string>
            {
                "Doors have a fire-rating parameter",
                "Rooms have names and numbers",
                "No unplaced rooms",
                "Sheets have number, name, and revision"
            }
        };
    }

    public static IReadOnlyList<SuggestedTaskDto> Plan(string projectName)
    {
        return
        [
            new SuggestedTaskDto { Title = $"Concept design — {projectName}", Priority = "High", Label = "Concept" },
            new SuggestedTaskDto { Title = "Revit modeling", Priority = "High", Label = "Modeling" },
            new SuggestedTaskDto { Title = "Internal design review", Priority = "Medium", Label = "Review" },
            new SuggestedTaskDto { Title = "Client revisions", Priority = "Medium", Label = "Revisions" },
            new SuggestedTaskDto { Title = "Delivery and sheet issue", Priority = "High", Label = "Delivery" }
        ];
    }

    private static void AddRoom(List<string> rooms, string lower, string token, string room)
    {
        if (lower.Contains(token, StringComparison.Ordinal) && !rooms.Contains(room, StringComparer.Ordinal))
        {
            rooms.Add(room);
        }
    }
}
