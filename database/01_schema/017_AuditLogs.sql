/*
    AuditLogs — append-only record of entity changes.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLogs
    (
        AuditLogId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        UserId     UNIQUEIDENTIFIER NULL,
        EntityType NVARCHAR(80)     NOT NULL,
        EntityId   UNIQUEIDENTIFIER NULL,
        Action     NVARCHAR(50)     NOT NULL,
        OldValues  NVARCHAR(MAX)    NULL,
        NewValues  NVARCHAR(MAX)    NULL,
        IpAddress  NVARCHAR(64)     NULL,
        CreatedAt  DATETIME2        NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt  DATETIME2        NOT NULL CONSTRAINT DF_AuditLogs_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy  UNIQUEIDENTIFIER NULL
    );
END
GO
