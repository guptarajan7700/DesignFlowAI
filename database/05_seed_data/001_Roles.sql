/*
    Seed roles. CreatedBy stays null because these rows are inserted before users.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Admin')
    INSERT INTO dbo.Roles (RoleId, Name, Description)
    VALUES ('A1000001-0000-4000-8000-000000000001', N'Admin', N'Firm settings, users, and approvals.');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'ProjectManager')
    INSERT INTO dbo.Roles (RoleId, Name, Description)
    VALUES ('A1000001-0000-4000-8000-000000000002', N'ProjectManager', N'Projects, milestones, and approval of AI task plans.');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Designer')
    INSERT INTO dbo.Roles (RoleId, Name, Description)
    VALUES ('A1000001-0000-4000-8000-000000000003', N'Designer', N'Modeling, validation issues, and drawing revisions.');

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Name = N'Client')
    INSERT INTO dbo.Roles (RoleId, Name, Description)
    VALUES ('A1000001-0000-4000-8000-000000000004', N'Client', N'Read-only portal access to their projects.');
GO
