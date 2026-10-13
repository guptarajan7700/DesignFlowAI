/*
    Issues — model-validation defects and manual punch items.
    Rows for a failed run are inserted by usp_CreateIssuesFromFailedValidation.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Issues', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Issues
    (
        IssueId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Issues PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProjectId          UNIQUEIDENTIFIER NOT NULL,
        ValidationRunId    UNIQUEIDENTIFIER NULL,
        ValidationResultId UNIQUEIDENTIFIER NULL,
        RevitModelId       UNIQUEIDENTIFIER NULL,
        Title              NVARCHAR(250)    NOT NULL,
        Description        NVARCHAR(MAX)    NULL,
        Severity           NVARCHAR(20)     NOT NULL,
        Status             NVARCHAR(30)     NOT NULL,
        ElementId          NVARCHAR(100)    NULL,
        SuggestedFix       NVARCHAR(MAX)    NULL,
        AssignedToId       UNIQUEIDENTIFIER NULL,
        AssignedAt         DATETIME2        NULL,
        ResolvedAt         DATETIME2        NULL,
        CreatedAt          DATETIME2        NOT NULL CONSTRAINT DF_Issues_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt          DATETIME2        NOT NULL CONSTRAINT DF_Issues_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy          UNIQUEIDENTIFIER NULL
    );
END
GO
