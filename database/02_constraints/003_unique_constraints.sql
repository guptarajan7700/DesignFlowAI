/*
    Unique constraints. Primary keys stay on the CREATE TABLE scripts.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_Roles_Name')
    ALTER TABLE dbo.Roles ADD CONSTRAINT UQ_Roles_Name UNIQUE (Name);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_Users_Email')
    ALTER TABLE dbo.Users ADD CONSTRAINT UQ_Users_Email UNIQUE (Email);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_UserRoles_User_Role')
    ALTER TABLE dbo.UserRoles ADD CONSTRAINT UQ_UserRoles_User_Role UNIQUE (UserId, RoleId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_Projects_Code')
    ALTER TABLE dbo.Projects ADD CONSTRAINT UQ_Projects_Code UNIQUE (Code);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_DrawingRevisions_Sheet')
    ALTER TABLE dbo.DrawingRevisions ADD CONSTRAINT UQ_DrawingRevisions_Sheet
        UNIQUE (ProjectId, SheetNumber, RevisionCode);
GO
