using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace DesignFlow.Revit.Checks;

public static class ModelChecks
{
    public static List<Studio.CheckResult> Run(Document document)
    {
        var results = new List<Studio.CheckResult>();
        results.AddRange(DoorsMissingFireRating(document));
        results.AddRange(RoomsWithoutNames(document));
        results.AddRange(UnplacedRooms(document));
        results.AddRange(MissingSheetParameters(document));
        return results;
    }

    private static IEnumerable<Studio.CheckResult> DoorsMissingFireRating(Document document)
    {
        var doors = new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_Doors)
            .WhereElementIsNotElementType()
            .ToElements();

        var failed = 0;
        foreach (var door in doors)
        {
            if (HasText(door, BuiltInParameter.DOOR_FIRE_RATING) || HasText(door, BuiltInParameter.FIRE_RATING))
            {
                continue;
            }

            failed++;
            yield return Fail(
                "DoorsMissingFireRating",
                "Doors missing fire rating",
                door,
                "Door",
                "This door has no fire rating.",
                "Set the Fire Rating parameter on the door.");
        }

        if (failed == 0)
        {
            yield return Pass("DoorsMissingFireRating", "Doors missing fire rating", "Every door in this model has a fire rating.");
        }
    }

    private static IEnumerable<Studio.CheckResult> RoomsWithoutNames(Document document)
    {
        var rooms = CollectRooms(document);
        var failed = 0;
        foreach (var room in rooms)
        {
            var name = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
            if (!string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            failed++;
            yield return Fail(
                "RoomsWithoutNames",
                "Rooms without names",
                room,
                "Room",
                "This room has no name.",
                "Enter a room name.");
        }

        if (failed == 0)
        {
            yield return Pass("RoomsWithoutNames", "Rooms without names", "Every placed room has a name.");
        }
    }

    private static IEnumerable<Studio.CheckResult> UnplacedRooms(Document document)
    {
        var rooms = CollectRooms(document);
        var failed = 0;
        foreach (var room in rooms)
        {
            if (room.Location is not null && RoomArea(room) > 0)
            {
                continue;
            }

            failed++;
            yield return Fail(
                "UnplacedRooms",
                "Unplaced rooms",
                room,
                "Room",
                "This room is not placed in the model.",
                "Place the room or delete the unplaced room.");
        }

        if (failed == 0)
        {
            yield return Pass("UnplacedRooms", "Unplaced rooms", "No unplaced rooms were found.");
        }
    }

    private static IEnumerable<Studio.CheckResult> MissingSheetParameters(Document document)
    {
        var sheets = new FilteredElementCollector(document)
            .OfClass(typeof(ViewSheet))
            .Cast<ViewSheet>()
            .ToList();

        var failed = 0;
        foreach (var sheet in sheets)
        {
            var missing = new List<string>();
            if (!HasText(sheet, BuiltInParameter.SHEET_NUMBER))
            {
                missing.Add("sheet number");
            }

            if (!HasText(sheet, BuiltInParameter.SHEET_NAME))
            {
                missing.Add("sheet name");
            }

            if (!HasText(sheet, BuiltInParameter.SHEET_DRAWN_BY))
            {
                missing.Add("drawn by");
            }

            if (missing.Count == 0)
            {
                continue;
            }

            failed++;
            yield return Fail(
                "MissingSheetParameters",
                "Missing sheet parameters",
                sheet,
                "Sheet",
                "Sheet is missing " + string.Join(", ", missing) + ".",
                "Fill the sheet number, name, and drawn-by parameters.");
        }

        if (failed == 0)
        {
            yield return Pass("MissingSheetParameters", "Missing sheet parameters", "Required sheet parameters are filled.");
        }
    }

    private static List<Room> CollectRooms(Document document) =>
        new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Room>()
            .ToList();

    private static double RoomArea(Room room)
    {
        try
        {
            return room.Area;
        }
        catch (Autodesk.Revit.Exceptions.InvalidOperationException)
        {
            return 0;
        }
    }

    private static bool HasText(Element element, BuiltInParameter parameter)
    {
        var value = element.get_Parameter(parameter);
        if (value is null || !value.HasValue)
        {
            return false;
        }

        var text = value.AsString();
        if (string.IsNullOrWhiteSpace(text))
        {
            text = value.AsValueString();
        }

        return !string.IsNullOrWhiteSpace(text);
    }

    private static Studio.CheckResult Pass(string code, string name, string message) => new()
    {
        CheckCode = code,
        CheckName = name,
        Status = "Pass",
        Message = message
    };

    private static Studio.CheckResult Fail(string code, string name, Element element, string category, string message, string fix) => new()
    {
        CheckCode = code,
        CheckName = name,
        Status = "Fail",
        ElementId = element.Id.Value.ToString(),
        ElementCategory = category,
        Message = message,
        SuggestedFix = fix
    };
}
