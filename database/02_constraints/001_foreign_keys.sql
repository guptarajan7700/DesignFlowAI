/*
    Foreign keys. Applied after every table exists.
    ON DELETE NO ACTION avoids multiple cascade paths in SQL Server.
    UserRoles is the exception: removing a user or role removes the assignment.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Users_CreatedBy')
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Roles_CreatedBy')
    ALTER TABLE dbo.Roles ADD CONSTRAINT FK_Roles_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UserRoles_Users')
    ALTER TABLE dbo.UserRoles ADD CONSTRAINT FK_UserRoles_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UserRoles_Roles')
    ALTER TABLE dbo.UserRoles ADD CONSTRAINT FK_UserRoles_Roles
        FOREIGN KEY (RoleId) REFERENCES dbo.Roles (RoleId) ON DELETE CASCADE;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UserRoles_CreatedBy')
    ALTER TABLE dbo.UserRoles ADD CONSTRAINT FK_UserRoles_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Clients_PortalUser')
    ALTER TABLE dbo.Clients ADD CONSTRAINT FK_Clients_PortalUser
        FOREIGN KEY (PortalUserId) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Clients_CreatedBy')
    ALTER TABLE dbo.Clients ADD CONSTRAINT FK_Clients_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Projects_Clients')
    ALTER TABLE dbo.Projects ADD CONSTRAINT FK_Projects_Clients
        FOREIGN KEY (ClientId) REFERENCES dbo.Clients (ClientId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Projects_ProjectManager')
    ALTER TABLE dbo.Projects ADD CONSTRAINT FK_Projects_ProjectManager
        FOREIGN KEY (ProjectManagerId) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Projects_LeadDesigner')
    ALTER TABLE dbo.Projects ADD CONSTRAINT FK_Projects_LeadDesigner
        FOREIGN KEY (LeadDesignerId) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Projects_CreatedBy')
    ALTER TABLE dbo.Projects ADD CONSTRAINT FK_Projects_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProjectRequirements_Projects')
    ALTER TABLE dbo.ProjectRequirements ADD CONSTRAINT FK_ProjectRequirements_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProjectRequirements_CreatedBy')
    ALTER TABLE dbo.ProjectRequirements ADD CONSTRAINT FK_ProjectRequirements_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Milestones_Projects')
    ALTER TABLE dbo.Milestones ADD CONSTRAINT FK_Milestones_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Milestones_CreatedBy')
    ALTER TABLE dbo.Milestones ADD CONSTRAINT FK_Milestones_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_Projects')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT FK_Tasks_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_Milestones')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT FK_Tasks_Milestones
        FOREIGN KEY (MilestoneId) REFERENCES dbo.Milestones (MilestoneId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_Requirements')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT FK_Tasks_Requirements
        FOREIGN KEY (RequirementId) REFERENCES dbo.ProjectRequirements (RequirementId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_ParentTask')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT FK_Tasks_ParentTask
        FOREIGN KEY (ParentTaskId) REFERENCES dbo.Tasks (TaskId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_Assignee')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT FK_Tasks_Assignee
        FOREIGN KEY (AssigneeId) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_ApprovedBy')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT FK_Tasks_ApprovedBy
        FOREIGN KEY (ApprovedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Tasks_CreatedBy')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT FK_Tasks_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RevitModels_Projects')
    ALTER TABLE dbo.RevitModels ADD CONSTRAINT FK_RevitModels_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RevitModels_CreatedBy')
    ALTER TABLE dbo.RevitModels ADD CONSTRAINT FK_RevitModels_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ValidationRuns_RevitModels')
    ALTER TABLE dbo.ValidationRuns ADD CONSTRAINT FK_ValidationRuns_RevitModels
        FOREIGN KEY (RevitModelId) REFERENCES dbo.RevitModels (RevitModelId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ValidationRuns_Projects')
    ALTER TABLE dbo.ValidationRuns ADD CONSTRAINT FK_ValidationRuns_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ValidationRuns_TriggeredBy')
    ALTER TABLE dbo.ValidationRuns ADD CONSTRAINT FK_ValidationRuns_TriggeredBy
        FOREIGN KEY (TriggeredById) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ValidationRuns_CreatedBy')
    ALTER TABLE dbo.ValidationRuns ADD CONSTRAINT FK_ValidationRuns_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ValidationResults_ValidationRuns')
    ALTER TABLE dbo.ValidationResults ADD CONSTRAINT FK_ValidationResults_ValidationRuns
        FOREIGN KEY (ValidationRunId) REFERENCES dbo.ValidationRuns (ValidationRunId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ValidationResults_CreatedBy')
    ALTER TABLE dbo.ValidationResults ADD CONSTRAINT FK_ValidationResults_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Issues_Projects')
    ALTER TABLE dbo.Issues ADD CONSTRAINT FK_Issues_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Issues_ValidationRuns')
    ALTER TABLE dbo.Issues ADD CONSTRAINT FK_Issues_ValidationRuns
        FOREIGN KEY (ValidationRunId) REFERENCES dbo.ValidationRuns (ValidationRunId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Issues_ValidationResults')
    ALTER TABLE dbo.Issues ADD CONSTRAINT FK_Issues_ValidationResults
        FOREIGN KEY (ValidationResultId) REFERENCES dbo.ValidationResults (ValidationResultId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Issues_RevitModels')
    ALTER TABLE dbo.Issues ADD CONSTRAINT FK_Issues_RevitModels
        FOREIGN KEY (RevitModelId) REFERENCES dbo.RevitModels (RevitModelId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Issues_AssignedTo')
    ALTER TABLE dbo.Issues ADD CONSTRAINT FK_Issues_AssignedTo
        FOREIGN KEY (AssignedToId) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Issues_CreatedBy')
    ALTER TABLE dbo.Issues ADD CONSTRAINT FK_Issues_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AutomationJobs_Projects')
    ALTER TABLE dbo.AutomationJobs ADD CONSTRAINT FK_AutomationJobs_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AutomationJobs_RevitModels')
    ALTER TABLE dbo.AutomationJobs ADD CONSTRAINT FK_AutomationJobs_RevitModels
        FOREIGN KEY (RevitModelId) REFERENCES dbo.RevitModels (RevitModelId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AutomationJobs_CreatedBy')
    ALTER TABLE dbo.AutomationJobs ADD CONSTRAINT FK_AutomationJobs_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DrawingRevisions_Projects')
    ALTER TABLE dbo.DrawingRevisions ADD CONSTRAINT FK_DrawingRevisions_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DrawingRevisions_RevitModels')
    ALTER TABLE dbo.DrawingRevisions ADD CONSTRAINT FK_DrawingRevisions_RevitModels
        FOREIGN KEY (RevitModelId) REFERENCES dbo.RevitModels (RevitModelId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DrawingRevisions_IssuedBy')
    ALTER TABLE dbo.DrawingRevisions ADD CONSTRAINT FK_DrawingRevisions_IssuedBy
        FOREIGN KEY (IssuedById) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DrawingRevisions_CreatedBy')
    ALTER TABLE dbo.DrawingRevisions ADD CONSTRAINT FK_DrawingRevisions_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Approvals_Projects')
    ALTER TABLE dbo.Approvals ADD CONSTRAINT FK_Approvals_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Approvals_RequestedBy')
    ALTER TABLE dbo.Approvals ADD CONSTRAINT FK_Approvals_RequestedBy
        FOREIGN KEY (RequestedById) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Approvals_ReviewedBy')
    ALTER TABLE dbo.Approvals ADD CONSTRAINT FK_Approvals_ReviewedBy
        FOREIGN KEY (ReviewedById) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Approvals_CreatedBy')
    ALTER TABLE dbo.Approvals ADD CONSTRAINT FK_Approvals_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AiCommandLogs_Users')
    ALTER TABLE dbo.AiCommandLogs ADD CONSTRAINT FK_AiCommandLogs_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AiCommandLogs_Projects')
    ALTER TABLE dbo.AiCommandLogs ADD CONSTRAINT FK_AiCommandLogs_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AiCommandLogs_CreatedBy')
    ALTER TABLE dbo.AiCommandLogs ADD CONSTRAINT FK_AiCommandLogs_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AuditLogs_Users')
    ALTER TABLE dbo.AuditLogs ADD CONSTRAINT FK_AuditLogs_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AuditLogs_CreatedBy')
    ALTER TABLE dbo.AuditLogs ADD CONSTRAINT FK_AuditLogs_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Notifications_Users')
    ALTER TABLE dbo.Notifications ADD CONSTRAINT FK_Notifications_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users (UserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Notifications_Projects')
    ALTER TABLE dbo.Notifications ADD CONSTRAINT FK_Notifications_Projects
        FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (ProjectId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Notifications_CreatedBy')
    ALTER TABLE dbo.Notifications ADD CONSTRAINT FK_Notifications_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (UserId);
GO
