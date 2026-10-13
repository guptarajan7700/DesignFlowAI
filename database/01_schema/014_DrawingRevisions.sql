/*
    DrawingRevisions — issued sheet revisions.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.DrawingRevisions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DrawingRevisions
    (
        DrawingRevisionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DrawingRevisions PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProjectId         UNIQUEIDENTIFIER NOT NULL,
        RevitModelId      UNIQUEIDENTIFIER NULL,
        SheetNumber       NVARCHAR(50)     NOT NULL,
        SheetName         NVARCHAR(200)    NOT NULL,
        RevisionCode      NVARCHAR(20)     NOT NULL,
        Description       NVARCHAR(1000)   NULL,
        IssuedAt          DATETIME2        NULL,
        IssuedById        UNIQUEIDENTIFIER NULL,
        CreatedAt         DATETIME2        NOT NULL CONSTRAINT DF_DrawingRevisions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt         DATETIME2        NOT NULL CONSTRAINT DF_DrawingRevisions_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy         UNIQUEIDENTIFIER NULL
    );
END
GO
