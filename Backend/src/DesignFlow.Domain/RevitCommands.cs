namespace DesignFlow.Domain;

/// <summary>
/// Natural-language Revit actions the API is allowed to run.
/// Anything outside this set is rejected before it is logged as executable.
/// </summary>
public static class RevitCommands
{
    public const string FindDoorsMissingFireRating = "FindDoorsMissingFireRating";
    public const string FindRoomsWithoutNames = "FindRoomsWithoutNames";
    public const string FindUnplacedRooms = "FindUnplacedRooms";
    public const string FindMissingSheetParameters = "FindMissingSheetParameters";

    public static readonly IReadOnlySet<string> Whitelist = new HashSet<string>(StringComparer.Ordinal)
    {
        FindDoorsMissingFireRating,
        FindRoomsWithoutNames,
        FindUnplacedRooms,
        FindMissingSheetParameters
    };

    public static string? CheckCodeFor(string command) => command switch
    {
        FindDoorsMissingFireRating => "DoorsMissingFireRating",
        FindRoomsWithoutNames => "RoomsWithoutNames",
        FindUnplacedRooms => "UnplacedRooms",
        FindMissingSheetParameters => "MissingSheetParameters",
        _ => null
    };

    public static bool IsAllowed(string? command) =>
        command is not null && Whitelist.Contains(command);
}
