/*
    AiCommandLogs — every preview, confirm, execute, and cancel of an AI command.
    DetectedCommand must be a whitelist entry. Previewed rows are not executions.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.AiCommandLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AiCommandLogs
    (
        AiCommandLogId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AiCommandLogs PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        UserId          UNIQUEIDENTIFIER NOT NULL,
        ProjectId       UNIQUEIDENTIFIER NULL,
        Prompt          NVARCHAR(MAX)    NOT NULL,
        DetectedCommand NVARCHAR(120)    NULL,
        ParametersJson  NVARCHAR(MAX)    NULL,
        PreviewJson     NVARCHAR(MAX)    NULL,
        Status          NVARCHAR(30)     NOT NULL,
        Provider        NVARCHAR(50)     NOT NULL,
        ResponseJson    NVARCHAR(MAX)    NULL,
        CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_AiCommandLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2        NOT NULL CONSTRAINT DF_AiCommandLogs_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy       UNIQUEIDENTIFIER NULL
    );
END
GO
