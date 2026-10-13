/*
    Milestones — dated gates on a project.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Milestones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Milestones
    (
        MilestoneId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Milestones PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProjectId   UNIQUEIDENTIFIER NOT NULL,
        Name        NVARCHAR(200)    NOT NULL,
        Description NVARCHAR(1000)   NULL,
        DueDate     DATE             NULL,
        CompletedAt DATETIME2        NULL,
        Status      NVARCHAR(30)     NOT NULL,
        SortOrder   INT              NOT NULL CONSTRAINT DF_Milestones_SortOrder DEFAULT (0),
        CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_Milestones_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2        NOT NULL CONSTRAINT DF_Milestones_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy   UNIQUEIDENTIFIER NULL
    );
END
GO
