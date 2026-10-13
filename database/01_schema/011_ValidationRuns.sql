/*
    ValidationRuns — one execution of the model checks against a Revit model.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.ValidationRuns', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ValidationRuns
    (
        ValidationRunId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ValidationRuns PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        RevitModelId    UNIQUEIDENTIFIER NOT NULL,
        ProjectId       UNIQUEIDENTIFIER NOT NULL,
        TriggeredById   UNIQUEIDENTIFIER NULL,
        Status          NVARCHAR(30)     NOT NULL,
        StartedAt       DATETIME2        NOT NULL CONSTRAINT DF_ValidationRuns_StartedAt DEFAULT (SYSUTCDATETIME()),
        CompletedAt     DATETIME2        NULL,
        Summary         NVARCHAR(1000)   NULL,
        ChecksPassed    INT              NOT NULL CONSTRAINT DF_ValidationRuns_ChecksPassed DEFAULT (0),
        ChecksFailed    INT              NOT NULL CONSTRAINT DF_ValidationRuns_ChecksFailed DEFAULT (0),
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_ValidationRuns_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2        NOT NULL CONSTRAINT DF_ValidationRuns_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy       UNIQUEIDENTIFIER NULL
    );
END
GO
