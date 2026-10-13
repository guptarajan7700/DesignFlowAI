/*
    Clients — design-firm accounts. PortalUserId links a client-role login.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Clients', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clients
    (
        ClientId     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Clients PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        CompanyName  NVARCHAR(200)    NOT NULL,
        ContactName  NVARCHAR(200)    NOT NULL,
        Email        NVARCHAR(256)    NOT NULL,
        Phone        NVARCHAR(30)     NULL,
        Website      NVARCHAR(300)    NULL,
        AddressLine1 NVARCHAR(200)    NULL,
        City         NVARCHAR(100)    NULL,
        State        NVARCHAR(100)    NULL,
        PostalCode   NVARCHAR(20)     NULL,
        Country      NVARCHAR(100)    NULL,
        Status       NVARCHAR(30)     NOT NULL,
        Notes        NVARCHAR(MAX)    NULL,
        PortalUserId UNIQUEIDENTIFIER NULL,
        CreatedAt    DATETIME2        NOT NULL CONSTRAINT DF_Clients_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2        NOT NULL CONSTRAINT DF_Clients_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy    UNIQUEIDENTIFIER NULL
    );
END
GO
