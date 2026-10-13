/*
    ProjectRequirements — structured brief items. AiAnalysisJson holds the model output for that row.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.ProjectRequirements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectRequirements
    (
        RequirementId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ProjectRequirements PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProjectId      UNIQUEIDENTIFIER NOT NULL,
        Title          NVARCHAR(200)    NOT NULL,
        Description    NVARCHAR(MAX)    NULL,
        Category       NVARCHAR(80)     NOT NULL,
        Priority       NVARCHAR(20)     NOT NULL,
        Source         NVARCHAR(40)     NOT NULL,
        Status         NVARCHAR(30)     NOT NULL,
        AiAnalysisJson NVARCHAR(MAX)    NULL,
        SortOrder      INT              NOT NULL CONSTRAINT DF_ProjectRequirements_SortOrder DEFAULT (0),
        CreatedAt      DATETIME2        NOT NULL CONSTRAINT DF_ProjectRequirements_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt      DATETIME2        NOT NULL CONSTRAINT DF_ProjectRequirements_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy      UNIQUEIDENTIFIER NULL
    );
END
GO
