/*
    Seed users.
    Development password for every account, applied in Phase 2: DesignFlow123!
    PasswordHash is a placeholder. It is not an ASP.NET Identity hash and will not authenticate.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Hash NVARCHAR(500) = N'DEV_ONLY_NOT_A_REAL_HASH';
DECLARE @Admin UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000001';

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@designflow.ai')
    INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedBy)
    VALUES (@Admin, N'admin@designflow.ai', @Hash, N'Ava', N'Kapoor', N'+91 98200 11001', 1, NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'maya.chen@designflow.ai')
    INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedBy)
    VALUES ('B2000001-0000-4000-8000-000000000002', N'maya.chen@designflow.ai', @Hash, N'Maya', N'Chen', N'+91 98200 11002', 1, @Admin);

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'arjun.mehta@designflow.ai')
    INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedBy)
    VALUES ('B2000001-0000-4000-8000-000000000003', N'arjun.mehta@designflow.ai', @Hash, N'Arjun', N'Mehta', N'+91 98200 11003', 1, @Admin);

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'lena.ortiz@designflow.ai')
    INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedBy)
    VALUES ('B2000001-0000-4000-8000-000000000004', N'lena.ortiz@designflow.ai', @Hash, N'Lena', N'Ortiz', N'+91 98200 11004', 1, @Admin);

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'rahul.sharma@northwind.example')
    INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedBy)
    VALUES ('B2000001-0000-4000-8000-000000000005', N'rahul.sharma@northwind.example', @Hash, N'Rahul', N'Sharma', N'+91 98200 22001', 1, @Admin);

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'sofia.alvarez@harborandco.example')
    INSERT INTO dbo.Users (UserId, Email, PasswordHash, FirstName, LastName, Phone, IsActive, CreatedBy)
    VALUES ('B2000001-0000-4000-8000-000000000006', N'sofia.alvarez@harborandco.example', @Hash, N'Sofia', N'Alvarez', N'+91 98200 22002', 1, @Admin);
GO
