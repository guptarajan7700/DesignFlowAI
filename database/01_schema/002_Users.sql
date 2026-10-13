/*
    Users — staff and client portal accounts.
    PasswordHash is an ASP.NET Identity hash. Phase 1 seed stores a placeholder only.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Email        NVARCHAR(256)    NOT NULL,
        PasswordHash NVARCHAR(500)    NOT NULL,
        FirstName    NVARCHAR(100)    NOT NULL,
        LastName     NVARCHAR(100)    NOT NULL,
        Phone        NVARCHAR(30)     NULL,
        IsActive     BIT              NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        LastLoginAt  DATETIME2        NULL,
        CreatedAt    DATETIME2        NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2        NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy    UNIQUEIDENTIFIER NULL
    );
END
GO
