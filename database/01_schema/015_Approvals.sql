/*
    Approvals — manager decisions for task plans, requirements, revisions, and reports.
    EntityId is polymorphic. It is not a foreign key.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Approvals', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Approvals
    (
        ApprovalId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Approvals PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProjectId     UNIQUEIDENTIFIER NOT NULL,
        EntityType    NVARCHAR(50)     NOT NULL,
        EntityId      UNIQUEIDENTIFIER NOT NULL,
        Status        NVARCHAR(30)     NOT NULL,
        RequestedById UNIQUEIDENTIFIER NOT NULL,
        ReviewedById  UNIQUEIDENTIFIER NULL,
        Comments      NVARCHAR(MAX)    NULL,
        RequestedAt   DATETIME2        NOT NULL CONSTRAINT DF_Approvals_RequestedAt DEFAULT (SYSUTCDATETIME()),
        ReviewedAt    DATETIME2        NULL,
        CreatedAt     DATETIME2        NOT NULL CONSTRAINT DF_Approvals_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt     DATETIME2        NOT NULL CONSTRAINT DF_Approvals_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy     UNIQUEIDENTIFIER NULL
    );
END
GO
