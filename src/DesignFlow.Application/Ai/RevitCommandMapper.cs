using DesignFlow.Domain;

namespace DesignFlow.Application.Ai;

public sealed record CommandMatch(string Command, string CheckCode);

/// <summary>Maps free text onto the Revit command whitelist. Unknown text does not match.</summary>
public static class RevitCommandMapper
{
    public static CommandMatch? Match(string prompt)
    {
        var text = prompt.ToLowerInvariant();

        if (text.Contains("fire", StringComparison.Ordinal) && text.Contains("door", StringComparison.Ordinal))
        {
            return Known(RevitCommands.FindDoorsMissingFireRating);
        }

        if (text.Contains("unplaced", StringComparison.Ordinal))
        {
            return Known(RevitCommands.FindUnplacedRooms);
        }

        if (text.Contains("sheet", StringComparison.Ordinal))
        {
            return Known(RevitCommands.FindMissingSheetParameters);
        }

        if (text.Contains("room", StringComparison.Ordinal)
            && (text.Contains("name", StringComparison.Ordinal) || text.Contains("number", StringComparison.Ordinal)))
        {
            return Known(RevitCommands.FindRoomsWithoutNames);
        }

        return null;
    }

    private static CommandMatch Known(string command)
    {
        var checkCode = RevitCommands.CheckCodeFor(command)
            ?? throw new InvalidOperationException($"Whitelist command {command} has no check code.");
        return new CommandMatch(command, checkCode);
    }
}
