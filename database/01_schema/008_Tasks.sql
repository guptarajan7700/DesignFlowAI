/*
    Tasks — delivery work.
    Source = AiProposed and Status = Proposed are not active work.
    A manager approval (usp_ApproveProposedTasks) moves them to ToDo.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Tasks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tasks
    (
        TaskId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Tasks PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProjectId       UNIQUEIDENTIFIER NOT NULL,
        MilestoneId     UNIQUEIDENTIFIER NULL,
        RequirementId   UNIQUEIDENTIFIER NULL,
        ParentTaskId    UNIQUEIDENTIFIER NULL,
        Title           NVARCHAR(250)    NOT NULL,
        Description     NVARCHAR(MAX)    NULL,
        Status          NVARCHAR(30)     NOT NULL,
        Priority        NVARCHAR(20)     NOT NULL,
        AssigneeId      UNIQUEIDENTIFIER NULL,
        DueDate         DATE             NULL,
        EstimatedHours  DECIMAL(8, 2)    NULL,
        Labels          NVARCHAR(400)    NULL,
        Source          NVARCHAR(40)     NOT NULL CONSTRAINT DF_Tasks_Source DEFAULT (N'Manual'),
        ApprovedBy      UNIQUEIDENTIFIER NULL,
        ApprovedAt      DATETIME2        NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2        NOT NULL CONSTRAINT DF_Tasks_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy       UNIQUEIDENTIFIER NULL
    );
END
GO
