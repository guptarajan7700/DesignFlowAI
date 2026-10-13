/*
    Check constraints for status values, money, and progress.
    Keep these lists aligned with seed data and the API enums in Phase 2.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Roles_Name')
    ALTER TABLE dbo.Roles ADD CONSTRAINT CK_Roles_Name
        CHECK (Name IN (N'Admin', N'ProjectManager', N'Designer', N'Client'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Clients_Status')
    ALTER TABLE dbo.Clients ADD CONSTRAINT CK_Clients_Status
        CHECK (Status IN (N'Prospect', N'Active', N'Inactive'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Projects_Status')
    ALTER TABLE dbo.Projects ADD CONSTRAINT CK_Projects_Status
        CHECK (Status IN (N'Draft', N'Active', N'OnHold', N'InReview', N'NeedsAttention', N'Completed', N'Archived'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Projects_Budget')
    ALTER TABLE dbo.Projects ADD CONSTRAINT CK_Projects_Budget
        CHECK (Budget >= 0 AND Spent >= 0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Projects_Currency')
    ALTER TABLE dbo.Projects ADD CONSTRAINT CK_Projects_Currency
        CHECK (LEN(Currency) = 3 AND Currency = UPPER(Currency));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ProjectRequirements_Category')
    ALTER TABLE dbo.ProjectRequirements ADD CONSTRAINT CK_ProjectRequirements_Category
        CHECK (Category IN (N'Room', N'Style', N'Budget', N'Constraint', N'Material', N'Storage', N'General'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ProjectRequirements_Priority')
    ALTER TABLE dbo.ProjectRequirements ADD CONSTRAINT CK_ProjectRequirements_Priority
        CHECK (Priority IN (N'Low', N'Medium', N'High', N'Critical'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ProjectRequirements_Source')
    ALTER TABLE dbo.ProjectRequirements ADD CONSTRAINT CK_ProjectRequirements_Source
        CHECK (Source IN (N'Manual', N'AiProposed', N'ClientBrief'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ProjectRequirements_Status')
    ALTER TABLE dbo.ProjectRequirements ADD CONSTRAINT CK_ProjectRequirements_Status
        CHECK (Status IN (N'Proposed', N'Approved', N'Rejected', N'Implemented'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Milestones_Status')
    ALTER TABLE dbo.Milestones ADD CONSTRAINT CK_Milestones_Status
        CHECK (Status IN (N'Pending', N'InProgress', N'Completed', N'Overdue'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Tasks_Status')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT CK_Tasks_Status
        CHECK (Status IN (N'Proposed', N'ToDo', N'InProgress', N'InReview', N'Done', N'Rejected', N'Cancelled'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Tasks_Priority')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT CK_Tasks_Priority
        CHECK (Priority IN (N'Low', N'Medium', N'High', N'Critical'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Tasks_Source')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT CK_Tasks_Source
        CHECK (Source IN (N'Manual', N'AiProposed'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Tasks_EstimatedHours')
    ALTER TABLE dbo.Tasks ADD CONSTRAINT CK_Tasks_EstimatedHours
        CHECK (EstimatedHours IS NULL OR EstimatedHours >= 0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Issues_Severity')
    ALTER TABLE dbo.Issues ADD CONSTRAINT CK_Issues_Severity
        CHECK (Severity IN (N'Critical', N'High', N'Medium', N'Low'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Issues_Status')
    ALTER TABLE dbo.Issues ADD CONSTRAINT CK_Issues_Status
        CHECK (Status IN (N'Open', N'Assigned', N'InProgress', N'Resolved', N'Closed', N'WontFix'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_RevitModels_Status')
    ALTER TABLE dbo.RevitModels ADD CONSTRAINT CK_RevitModels_Status
        CHECK (Status IN (N'Draft', N'Synced', N'Translating', N'Ready', N'Failed'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ValidationRuns_Status')
    ALTER TABLE dbo.ValidationRuns ADD CONSTRAINT CK_ValidationRuns_Status
        CHECK (Status IN (N'Queued', N'Running', N'Passed', N'Failed', N'Error'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ValidationRuns_Counts')
    ALTER TABLE dbo.ValidationRuns ADD CONSTRAINT CK_ValidationRuns_Counts
        CHECK (ChecksPassed >= 0 AND ChecksFailed >= 0);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_ValidationResults_Status')
    ALTER TABLE dbo.ValidationResults ADD CONSTRAINT CK_ValidationResults_Status
        CHECK (Status IN (N'Pass', N'Fail', N'Warning', N'Skipped'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_AutomationJobs_Status')
    ALTER TABLE dbo.AutomationJobs ADD CONSTRAINT CK_AutomationJobs_Status
        CHECK (Status IN (N'Queued', N'Running', N'Succeeded', N'Failed', N'Cancelled'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_AutomationJobs_Progress')
    ALTER TABLE dbo.AutomationJobs ADD CONSTRAINT CK_AutomationJobs_Progress
        CHECK (ProgressPercent BETWEEN 0 AND 100);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Approvals_Status')
    ALTER TABLE dbo.Approvals ADD CONSTRAINT CK_Approvals_Status
        CHECK (Status IN (N'Pending', N'Approved', N'Rejected'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Approvals_EntityType')
    ALTER TABLE dbo.Approvals ADD CONSTRAINT CK_Approvals_EntityType
        CHECK (EntityType IN (N'TaskPlan', N'DrawingRevision', N'Requirement', N'Report'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_AiCommandLogs_Status')
    ALTER TABLE dbo.AiCommandLogs ADD CONSTRAINT CK_AiCommandLogs_Status
        CHECK (Status IN (N'Previewed', N'Confirmed', N'Executed', N'Cancelled', N'Failed'));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Notifications_Type')
    ALTER TABLE dbo.Notifications ADD CONSTRAINT CK_Notifications_Type
        CHECK (Type IN (N'ValidationFailed', N'TaskAssigned', N'ApprovalRequested', N'IssueAssigned', N'Info'));
GO
