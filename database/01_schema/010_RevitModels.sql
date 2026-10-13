/*
    RevitModels — a published or synced Revit file for a project.
    AutodeskUrn is filled when APS translation is configured. It may be null locally.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.RevitModels', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RevitModels
    (
        RevitModelId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RevitModels PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProjectId         UNIQUEIDENTIFIER NOT NULL,
        Name              NVARCHAR(200)    NOT NULL,
        FileName          NVARCHAR(260)    NOT NULL,
        VersionLabel      NVARCHAR(50)     NULL,
        AutodeskUrn       NVARCHAR(500)    NULL,
        AutodeskProjectId NVARCHAR(100)    NULL,
        AutodeskItemId    NVARCHAR(100)    NULL,
        LastSyncedAt      DATETIME2        NULL,
        Status            NVARCHAR(30)     NOT NULL,
        CreatedAt         DATETIME2        NOT NULL CONSTRAINT DF_RevitModels_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt         DATETIME2        NOT NULL CONSTRAINT DF_RevitModels_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy         UNIQUEIDENTIFIER NULL
    );
END
GO
