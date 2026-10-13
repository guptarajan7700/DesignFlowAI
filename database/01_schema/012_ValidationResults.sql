/*
    ValidationResults — one check outcome inside a run.
    CheckCode values used by the Revit add-in:
      DoorsMissingFireRating, RoomsWithoutNames, UnplacedRooms, MissingSheetParameters.
    Design-review summaries may describe these rows only.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.ValidationResults', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ValidationResults
    (
        ValidationResultId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ValidationResults PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ValidationRunId    UNIQUEIDENTIFIER NOT NULL,
        CheckCode          NVARCHAR(80)     NOT NULL,
        CheckName          NVARCHAR(200)    NOT NULL,
        Status             NVARCHAR(20)     NOT NULL,
        ElementId          NVARCHAR(100)    NULL,
        ElementCategory    NVARCHAR(80)     NULL,
        Message            NVARCHAR(1000)   NOT NULL,
        SuggestedFix       NVARCHAR(MAX)    NULL,
        SortOrder          INT              NOT NULL CONSTRAINT DF_ValidationResults_SortOrder DEFAULT (0),
        CreatedAt          DATETIME2        NOT NULL CONSTRAINT DF_ValidationResults_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt          DATETIME2        NOT NULL CONSTRAINT DF_ValidationResults_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy          UNIQUEIDENTIFIER NULL
    );
END
GO
