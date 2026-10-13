/*
    Nonclustered indexes on foreign keys, status columns, and common filters.
    SQL Server does not index foreign keys automatically.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_CreatedBy' AND object_id = OBJECT_ID(N'dbo.Users'))
    CREATE INDEX IX_Users_CreatedBy ON dbo.Users (CreatedBy);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_IsActive' AND object_id = OBJECT_ID(N'dbo.Users'))
    CREATE INDEX IX_Users_IsActive ON dbo.Users (IsActive);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserRoles_UserId' AND object_id = OBJECT_ID(N'dbo.UserRoles'))
    CREATE INDEX IX_UserRoles_UserId ON dbo.UserRoles (UserId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserRoles_RoleId' AND object_id = OBJECT_ID(N'dbo.UserRoles'))
    CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles (RoleId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Clients_Status' AND object_id = OBJECT_ID(N'dbo.Clients'))
    CREATE INDEX IX_Clients_Status ON dbo.Clients (Status);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Clients_PortalUserId' AND object_id = OBJECT_ID(N'dbo.Clients'))
    CREATE INDEX IX_Clients_PortalUserId ON dbo.Clients (PortalUserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Projects_ClientId' AND object_id = OBJECT_ID(N'dbo.Projects'))
    CREATE INDEX IX_Projects_ClientId ON dbo.Projects (ClientId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Projects_Status' AND object_id = OBJECT_ID(N'dbo.Projects'))
    CREATE INDEX IX_Projects_Status ON dbo.Projects (Status) INCLUDE (DueDate, LeadDesignerId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Projects_ProjectManagerId' AND object_id = OBJECT_ID(N'dbo.Projects'))
    CREATE INDEX IX_Projects_ProjectManagerId ON dbo.Projects (ProjectManagerId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Projects_LeadDesignerId' AND object_id = OBJECT_ID(N'dbo.Projects'))
    CREATE INDEX IX_Projects_LeadDesignerId ON dbo.Projects (LeadDesignerId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Projects_DueDate' AND object_id = OBJECT_ID(N'dbo.Projects'))
    CREATE INDEX IX_Projects_DueDate ON dbo.Projects (DueDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProjectRequirements_ProjectId' AND object_id = OBJECT_ID(N'dbo.ProjectRequirements'))
    CREATE INDEX IX_ProjectRequirements_ProjectId ON dbo.ProjectRequirements (ProjectId, SortOrder);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProjectRequirements_Status' AND object_id = OBJECT_ID(N'dbo.ProjectRequirements'))
    CREATE INDEX IX_ProjectRequirements_Status ON dbo.ProjectRequirements (Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Milestones_ProjectId' AND object_id = OBJECT_ID(N'dbo.Milestones'))
    CREATE INDEX IX_Milestones_ProjectId ON dbo.Milestones (ProjectId, SortOrder);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Milestones_Status' AND object_id = OBJECT_ID(N'dbo.Milestones'))
    CREATE INDEX IX_Milestones_Status ON dbo.Milestones (Status, DueDate);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_ProjectId' AND object_id = OBJECT_ID(N'dbo.Tasks'))
    CREATE INDEX IX_Tasks_ProjectId ON dbo.Tasks (ProjectId, Status);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_Status_DueDate' AND object_id = OBJECT_ID(N'dbo.Tasks'))
    CREATE INDEX IX_Tasks_Status_DueDate ON dbo.Tasks (Status, DueDate) INCLUDE (AssigneeId, ProjectId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_AssigneeId' AND object_id = OBJECT_ID(N'dbo.Tasks'))
    CREATE INDEX IX_Tasks_AssigneeId ON dbo.Tasks (AssigneeId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_MilestoneId' AND object_id = OBJECT_ID(N'dbo.Tasks'))
    CREATE INDEX IX_Tasks_MilestoneId ON dbo.Tasks (MilestoneId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_RequirementId' AND object_id = OBJECT_ID(N'dbo.Tasks'))
    CREATE INDEX IX_Tasks_RequirementId ON dbo.Tasks (RequirementId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tasks_ParentTaskId' AND object_id = OBJECT_ID(N'dbo.Tasks'))
    CREATE INDEX IX_Tasks_ParentTaskId ON dbo.Tasks (ParentTaskId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Issues_ProjectId' AND object_id = OBJECT_ID(N'dbo.Issues'))
    CREATE INDEX IX_Issues_ProjectId ON dbo.Issues (ProjectId, Status);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Issues_Severity' AND object_id = OBJECT_ID(N'dbo.Issues'))
    CREATE INDEX IX_Issues_Severity ON dbo.Issues (Severity, Status);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Issues_AssignedToId' AND object_id = OBJECT_ID(N'dbo.Issues'))
    CREATE INDEX IX_Issues_AssignedToId ON dbo.Issues (AssignedToId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Issues_ValidationRunId' AND object_id = OBJECT_ID(N'dbo.Issues'))
    CREATE INDEX IX_Issues_ValidationRunId ON dbo.Issues (ValidationRunId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Issues_ValidationResultId' AND object_id = OBJECT_ID(N'dbo.Issues'))
    CREATE INDEX IX_Issues_ValidationResultId ON dbo.Issues (ValidationResultId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Issues_RevitModelId' AND object_id = OBJECT_ID(N'dbo.Issues'))
    CREATE INDEX IX_Issues_RevitModelId ON dbo.Issues (RevitModelId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RevitModels_ProjectId' AND object_id = OBJECT_ID(N'dbo.RevitModels'))
    CREATE INDEX IX_RevitModels_ProjectId ON dbo.RevitModels (ProjectId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RevitModels_Status' AND object_id = OBJECT_ID(N'dbo.RevitModels'))
    CREATE INDEX IX_RevitModels_Status ON dbo.RevitModels (Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ValidationRuns_ProjectId' AND object_id = OBJECT_ID(N'dbo.ValidationRuns'))
    CREATE INDEX IX_ValidationRuns_ProjectId ON dbo.ValidationRuns (ProjectId, StartedAt);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ValidationRuns_RevitModelId' AND object_id = OBJECT_ID(N'dbo.ValidationRuns'))
    CREATE INDEX IX_ValidationRuns_RevitModelId ON dbo.ValidationRuns (RevitModelId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ValidationRuns_Status' AND object_id = OBJECT_ID(N'dbo.ValidationRuns'))
    CREATE INDEX IX_ValidationRuns_Status ON dbo.ValidationRuns (Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ValidationResults_ValidationRunId' AND object_id = OBJECT_ID(N'dbo.ValidationResults'))
    CREATE INDEX IX_ValidationResults_ValidationRunId ON dbo.ValidationResults (ValidationRunId, SortOrder);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ValidationResults_Status' AND object_id = OBJECT_ID(N'dbo.ValidationResults'))
    CREATE INDEX IX_ValidationResults_Status ON dbo.ValidationResults (Status, CheckCode);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AutomationJobs_Status' AND object_id = OBJECT_ID(N'dbo.AutomationJobs'))
    CREATE INDEX IX_AutomationJobs_Status ON dbo.AutomationJobs (Status, CreatedAt);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AutomationJobs_ProjectId' AND object_id = OBJECT_ID(N'dbo.AutomationJobs'))
    CREATE INDEX IX_AutomationJobs_ProjectId ON dbo.AutomationJobs (ProjectId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AutomationJobs_RevitModelId' AND object_id = OBJECT_ID(N'dbo.AutomationJobs'))
    CREATE INDEX IX_AutomationJobs_RevitModelId ON dbo.AutomationJobs (RevitModelId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DrawingRevisions_ProjectId' AND object_id = OBJECT_ID(N'dbo.DrawingRevisions'))
    CREATE INDEX IX_DrawingRevisions_ProjectId ON dbo.DrawingRevisions (ProjectId, IssuedAt);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DrawingRevisions_RevitModelId' AND object_id = OBJECT_ID(N'dbo.DrawingRevisions'))
    CREATE INDEX IX_DrawingRevisions_RevitModelId ON dbo.DrawingRevisions (RevitModelId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Approvals_ProjectId' AND object_id = OBJECT_ID(N'dbo.Approvals'))
    CREATE INDEX IX_Approvals_ProjectId ON dbo.Approvals (ProjectId, Status);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Approvals_Entity' AND object_id = OBJECT_ID(N'dbo.Approvals'))
    CREATE INDEX IX_Approvals_Entity ON dbo.Approvals (EntityType, EntityId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AiCommandLogs_UserId' AND object_id = OBJECT_ID(N'dbo.AiCommandLogs'))
    CREATE INDEX IX_AiCommandLogs_UserId ON dbo.AiCommandLogs (UserId, CreatedAt);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AiCommandLogs_ProjectId' AND object_id = OBJECT_ID(N'dbo.AiCommandLogs'))
    CREATE INDEX IX_AiCommandLogs_ProjectId ON dbo.AiCommandLogs (ProjectId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AiCommandLogs_Status' AND object_id = OBJECT_ID(N'dbo.AiCommandLogs'))
    CREATE INDEX IX_AiCommandLogs_Status ON dbo.AiCommandLogs (Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_Entity' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE INDEX IX_AuditLogs_Entity ON dbo.AuditLogs (EntityType, EntityId, CreatedAt);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_UserId' AND object_id = OBJECT_ID(N'dbo.AuditLogs'))
    CREATE INDEX IX_AuditLogs_UserId ON dbo.AuditLogs (UserId, CreatedAt);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notifications_User_Unread' AND object_id = OBJECT_ID(N'dbo.Notifications'))
    CREATE INDEX IX_Notifications_User_Unread ON dbo.Notifications (UserId, CreatedAt DESC) WHERE IsRead = 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notifications_ProjectId' AND object_id = OBJECT_ID(N'dbo.Notifications'))
    CREATE INDEX IX_Notifications_ProjectId ON dbo.Notifications (ProjectId);
GO
