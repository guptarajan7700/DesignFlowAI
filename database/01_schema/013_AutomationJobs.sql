/*
    AutomationJobs — Model Derivative, Design Automation, sheet export, and validation queue.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.AutomationJobs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AutomationJobs
    (
        AutomationJobId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AutomationJobs PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProjectId          UNIQUEIDENTIFIER NULL,
        RevitModelId       UNIQUEIDENTIFIER NULL,
        JobType            NVARCHAR(80)     NOT NULL,
        Status             NVARCHAR(30)     NOT NULL,
        ProgressPercent    TINYINT          NOT NULL CONSTRAINT DF_AutomationJobs_ProgressPercent DEFAULT (0),
        ExternalWorkItemId NVARCHAR(200)    NULL,
        RequestPayload     NVARCHAR(MAX)    NULL,
        ResultPayload      NVARCHAR(MAX)    NULL,
        ErrorMessage       NVARCHAR(MAX)    NULL,
        StartedAt          DATETIME2        NULL,
        CompletedAt        DATETIME2        NULL,
        CreatedAt          DATETIME2        NOT NULL CONSTRAINT DF_AutomationJobs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt          DATETIME2        NOT NULL CONSTRAINT DF_AutomationJobs_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy          UNIQUEIDENTIFIER NULL
    );
END
GO
