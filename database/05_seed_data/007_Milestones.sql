/*
    Milestone timeline. Harbor modeling is already overdue.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Admin UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000001';
DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @P1 UNIQUEIDENTIFIER = 'D4000001-0000-4000-8000-000000000001';
DECLARE @P2 UNIQUEIDENTIFIER = 'D4000001-0000-4000-8000-000000000002';
DECLARE @P3 UNIQUEIDENTIFIER = 'D4000001-0000-4000-8000-000000000003';

IF NOT EXISTS (SELECT 1 FROM dbo.Milestones WHERE MilestoneId = 'F6000001-0000-4000-8000-000000000001')
    INSERT INTO dbo.Milestones (MilestoneId, ProjectId, Name, Description, DueDate, CompletedAt, Status, SortOrder, CreatedBy)
    VALUES ('F6000001-0000-4000-8000-000000000001', @P1, N'Concept sign-off', N'Layout and material direction approved by the client.', DATEADD(DAY, -18, @Today), DATEADD(DAY, -18, SYSUTCDATETIME()), N'Completed', 1, @Admin);

IF NOT EXISTS (SELECT 1 FROM dbo.Milestones WHERE MilestoneId = 'F6000001-0000-4000-8000-000000000002')
    INSERT INTO dbo.Milestones (MilestoneId, ProjectId, Name, Description, DueDate, Status, SortOrder, CreatedBy)
    VALUES ('F6000001-0000-4000-8000-000000000002', @P1, N'Model and coordinate', N'Revit model for kitchen, storage, and reflected ceiling.', DATEADD(DAY, 14, @Today), N'InProgress', 2, @Admin);

IF NOT EXISTS (SELECT 1 FROM dbo.Milestones WHERE MilestoneId = 'F6000001-0000-4000-8000-000000000003')
    INSERT INTO dbo.Milestones (MilestoneId, ProjectId, Name, Description, DueDate, Status, SortOrder, CreatedBy)
    VALUES ('F6000001-0000-4000-8000-000000000003', @P1, N'Issue for construction', N'Sheets, revisions, and client approval.', DATEADD(DAY, 48, @Today), N'Pending', 3, @Admin);

IF NOT EXISTS (SELECT 1 FROM dbo.Milestones WHERE MilestoneId = 'F6000001-0000-4000-8000-000000000004')
    INSERT INTO dbo.Milestones (MilestoneId, ProjectId, Name, Description, DueDate, CompletedAt, Status, SortOrder, CreatedBy)
    VALUES ('F6000001-0000-4000-8000-000000000004', @P2, N'Concept sign-off', N'Loft plan and mezzanine approved.', DATEADD(DAY, -30, @Today), DATEADD(DAY, -28, SYSUTCDATETIME()), N'Completed', 1, @Admin);

IF NOT EXISTS (SELECT 1 FROM dbo.Milestones WHERE MilestoneId = 'F6000001-0000-4000-8000-000000000005')
    INSERT INTO dbo.Milestones (MilestoneId, ProjectId, Name, Description, DueDate, Status, SortOrder, CreatedBy)
    VALUES ('F6000001-0000-4000-8000-000000000005', @P2, N'Lighting and FF&E model', N'Cove lighting and furniture plan are past the internal due date.', DATEADD(DAY, -2, @Today), N'Overdue', 2, @Admin);

IF NOT EXISTS (SELECT 1 FROM dbo.Milestones WHERE MilestoneId = 'F6000001-0000-4000-8000-000000000006')
    INSERT INTO dbo.Milestones (MilestoneId, ProjectId, Name, Description, DueDate, Status, SortOrder, CreatedBy)
    VALUES ('F6000001-0000-4000-8000-000000000006', @P3, N'Lobby concept', N'Mood, reception desk, and feature ceiling options.', DATEADD(DAY, 10, @Today), N'InProgress', 1, @Admin);
GO
