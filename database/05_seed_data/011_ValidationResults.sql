/*
    Check results. Only Status = Fail becomes an issue in the next script.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Arjun UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000003';
DECLARE @Lena  UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000004';
DECLARE @FailedRun UNIQUEIDENTIFIER = 'C9000001-0000-4000-8000-000000000001';
DECLARE @PassedRun UNIQUEIDENTIFIER = 'C9000001-0000-4000-8000-000000000002';

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationResults WHERE ValidationResultId = 'D9100001-0000-4000-8000-000000000001')
    INSERT INTO dbo.ValidationResults
        (ValidationResultId, ValidationRunId, CheckCode, CheckName, Status, ElementId, ElementCategory, Message, SuggestedFix, SortOrder, CreatedBy)
    VALUES
    (
        'D9100001-0000-4000-8000-000000000001', @FailedRun,
        N'DoorsMissingFireRating', N'Doors missing fire rating', N'Fail',
        N'345221', N'Doors',
        N'Door 345221 (entry) has an empty Fire Rating parameter.',
        N'Set Fire Rating to 60 min on the entry and kitchen doors, or load the shared parameter into the family.',
        1, @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationResults WHERE ValidationResultId = 'D9100001-0000-4000-8000-000000000002')
    INSERT INTO dbo.ValidationResults
        (ValidationResultId, ValidationRunId, CheckCode, CheckName, Status, ElementId, ElementCategory, Message, SuggestedFix, SortOrder, CreatedBy)
    VALUES
    (
        'D9100001-0000-4000-8000-000000000002', @FailedRun,
        N'RoomsWithoutNames', N'Rooms without names or numbers', N'Fail',
        N'88102', N'Rooms',
        N'Room 88102 has no name and no number.',
        N'Name the room and assign a number before sheets are issued.',
        2, @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationResults WHERE ValidationResultId = 'D9100001-0000-4000-8000-000000000003')
    INSERT INTO dbo.ValidationResults
        (ValidationResultId, ValidationRunId, CheckCode, CheckName, Status, ElementCategory, Message, SortOrder, CreatedBy)
    VALUES
    (
        'D9100001-0000-4000-8000-000000000003', @FailedRun,
        N'UnplacedRooms', N'Unplaced rooms', N'Pass',
        N'Rooms', N'No unplaced rooms were found.', 3, @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationResults WHERE ValidationResultId = 'D9100001-0000-4000-8000-000000000004')
    INSERT INTO dbo.ValidationResults
        (ValidationResultId, ValidationRunId, CheckCode, CheckName, Status, ElementCategory, Message, SortOrder, CreatedBy)
    VALUES
    (
        'D9100001-0000-4000-8000-000000000004', @FailedRun,
        N'MissingSheetParameters', N'Missing sheet parameters', N'Pass',
        N'Sheets', N'Required sheet number, name, and revision parameters are filled.', 4, @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationResults WHERE ValidationResultId = 'D9100001-0000-4000-8000-000000000005')
    INSERT INTO dbo.ValidationResults
        (ValidationResultId, ValidationRunId, CheckCode, CheckName, Status, ElementCategory, Message, SortOrder, CreatedBy)
    VALUES
        ('D9100001-0000-4000-8000-000000000005', @PassedRun, N'DoorsMissingFireRating', N'Doors missing fire rating', N'Pass', N'Doors', N'Every door has a Fire Rating value.', 1, @Lena);

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationResults WHERE ValidationResultId = 'D9100001-0000-4000-8000-000000000006')
    INSERT INTO dbo.ValidationResults
        (ValidationResultId, ValidationRunId, CheckCode, CheckName, Status, ElementCategory, Message, SortOrder, CreatedBy)
    VALUES
        ('D9100001-0000-4000-8000-000000000006', @PassedRun, N'RoomsWithoutNames', N'Rooms without names or numbers', N'Pass', N'Rooms', N'Every placed room has a name and a number.', 2, @Lena);

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationResults WHERE ValidationResultId = 'D9100001-0000-4000-8000-000000000007')
    INSERT INTO dbo.ValidationResults
        (ValidationResultId, ValidationRunId, CheckCode, CheckName, Status, ElementCategory, Message, SortOrder, CreatedBy)
    VALUES
        ('D9100001-0000-4000-8000-000000000007', @PassedRun, N'UnplacedRooms', N'Unplaced rooms', N'Pass', N'Rooms', N'No unplaced rooms were found.', 3, @Lena);

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationResults WHERE ValidationResultId = 'D9100001-0000-4000-8000-000000000008')
    INSERT INTO dbo.ValidationResults
        (ValidationResultId, ValidationRunId, CheckCode, CheckName, Status, ElementCategory, Message, SortOrder, CreatedBy)
    VALUES
        ('D9100001-0000-4000-8000-000000000008', @PassedRun, N'MissingSheetParameters', N'Missing sheet parameters', N'Pass', N'Sheets', N'Required sheet parameters are filled.', 4, @Lena);
GO
