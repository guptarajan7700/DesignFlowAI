/*
    Projects — one client engagement. Status moves to NeedsAttention when validation fails.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Projects', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Projects
    (
        ProjectId        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Projects PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ClientId         UNIQUEIDENTIFIER NOT NULL,
        Code             NVARCHAR(50)     NOT NULL,
        Name             NVARCHAR(200)    NOT NULL,
        Description      NVARCHAR(MAX)    NULL,
        Status           NVARCHAR(40)     NOT NULL,
        StartDate        DATE             NULL,
        DueDate          DATE             NULL,
        Budget           DECIMAL(18, 2)   NOT NULL CONSTRAINT DF_Projects_Budget DEFAULT (0),
        Spent            DECIMAL(18, 2)   NOT NULL CONSTRAINT DF_Projects_Spent DEFAULT (0),
        Currency         CHAR(3)          NOT NULL CONSTRAINT DF_Projects_Currency DEFAULT ('INR'),
        SiteAddress      NVARCHAR(300)    NULL,
        CoverImageUrl    NVARCHAR(500)    NULL,
        ProjectManagerId UNIQUEIDENTIFIER NULL,
        LeadDesignerId   UNIQUEIDENTIFIER NULL,
        CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_Projects_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt        DATETIME2        NOT NULL CONSTRAINT DF_Projects_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy        UNIQUEIDENTIFIER NULL
    );
END
GO
