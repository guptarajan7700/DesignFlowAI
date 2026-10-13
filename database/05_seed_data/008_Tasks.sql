/*
    Active tasks plus three AI-proposed tasks that must stay Proposed
    until usp_ApproveProposedTasks runs.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Maya  UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000002';
DECLARE @Arjun UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000003';
DECLARE @Lena  UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000004';
DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);
DECLARE @P1 UNIQUEIDENTIFIER = 'D4000001-0000-4000-8000-000000000001';
DECLARE @P2 UNIQUEIDENTIFIER = 'D4000001-0000-4000-8000-000000000002';
DECLARE @P3 UNIQUEIDENTIFIER = 'D4000001-0000-4000-8000-000000000003';

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE TaskId = 'A7000001-0000-4000-8000-000000000001')
    INSERT INTO dbo.Tasks
        (TaskId, ProjectId, MilestoneId, RequirementId, Title, Description, Status, Priority, AssigneeId, DueDate, EstimatedHours, Labels, Source, CreatedBy)
    VALUES
    (
        'A7000001-0000-4000-8000-000000000001', @P1,
        'F6000001-0000-4000-8000-000000000001', 'E5000001-0000-4000-8000-000000000001',
        N'Site measurement and as-built notes',
        N'Record structural columns, window mullions, and wet-area walls.',
        N'Done', N'Medium', @Arjun, DATEADD(DAY, -20, @Today), 6, N'Survey', N'Manual', @Maya
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE TaskId = 'A7000001-0000-4000-8000-000000000002')
    INSERT INTO dbo.Tasks
        (TaskId, ProjectId, MilestoneId, RequirementId, Title, Description, Status, Priority, AssigneeId, DueDate, EstimatedHours, Labels, Source, CreatedBy)
    VALUES
    (
        'A7000001-0000-4000-8000-000000000002', @P1,
        'F6000001-0000-4000-8000-000000000001', 'E5000001-0000-4000-8000-000000000004',
        N'Concept boards and material palette',
        N'Oak, limestone, and matte black metal. Client review in progress.',
        N'InReview', N'High', @Arjun, DATEADD(DAY, 2, @Today), 12, N'Concept', N'Manual', @Maya
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE TaskId = 'A7000001-0000-4000-8000-000000000003')
    INSERT INTO dbo.Tasks
        (TaskId, ProjectId, MilestoneId, RequirementId, Title, Description, Status, Priority, AssigneeId, DueDate, EstimatedHours, Labels, Source, CreatedBy)
    VALUES
    (
        'A7000001-0000-4000-8000-000000000003', @P1,
        'F6000001-0000-4000-8000-000000000002', 'E5000001-0000-4000-8000-000000000002',
        N'Model kitchen carcass and appliances',
        N'Place the approved kitchen run. Do not treat AI proposals as extra scope.',
        N'InProgress', N'High', @Arjun, DATEADD(DAY, 5, @Today), 16, N'BIM,Kitchen', N'Manual', @Maya
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE TaskId = 'A7000001-0000-4000-8000-000000000004')
    INSERT INTO dbo.Tasks
        (TaskId, ProjectId, MilestoneId, Title, Description, Status, Priority, AssigneeId, DueDate, EstimatedHours, Labels, Source, CreatedBy)
    VALUES
    (
        'A7000001-0000-4000-8000-000000000004', @P2,
        'F6000001-0000-4000-8000-000000000005',
        N'Harbor cove lighting layout',
        N'Internal due date has passed. Mezzanine cove is still schematic.',
        N'InProgress', N'High', @Lena, DATEADD(DAY, -4, @Today), 10, N'Lighting', N'Manual', @Maya
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE TaskId = 'A7000001-0000-4000-8000-000000000005')
    INSERT INTO dbo.Tasks
        (TaskId, ProjectId, MilestoneId, Title, Description, Status, Priority, AssigneeId, DueDate, EstimatedHours, Labels, Source, CreatedBy)
    VALUES
    (
        'A7000001-0000-4000-8000-000000000005', @P2,
        'F6000001-0000-4000-8000-000000000005',
        N'Loose furniture schedule',
        N'Lounge chairs, dining table, and rug sizes for the loft.',
        N'ToDo', N'Medium', @Lena, DATEADD(DAY, 12, @Today), 8, N'FF&E', N'Manual', @Maya
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE TaskId = 'A7000001-0000-4000-8000-000000000006')
    INSERT INTO dbo.Tasks
        (TaskId, ProjectId, MilestoneId, Title, Description, Status, Priority, AssigneeId, DueDate, EstimatedHours, Labels, Source, CreatedBy)
    VALUES
    (
        'A7000001-0000-4000-8000-000000000006', @P3,
        'F6000001-0000-4000-8000-000000000006',
        N'Lobby material direction',
        N'Two options for the reception desk and feature ceiling.',
        N'InProgress', N'Medium', @Arjun, DATEADD(DAY, 7, @Today), 14, N'Hospitality', N'Manual', @Maya
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE TaskId = 'A7000001-0000-4000-8000-000000000007')
    INSERT INTO dbo.Tasks
        (TaskId, ProjectId, MilestoneId, RequirementId, Title, Description, Status, Priority, DueDate, EstimatedHours, Labels, Source, CreatedBy)
    VALUES
    (
        'A7000001-0000-4000-8000-000000000007', @P1,
        'F6000001-0000-4000-8000-000000000002', 'E5000001-0000-4000-8000-000000000002',
        N'Model modular kitchen from the approved brief',
        N'AI plan. Stays Proposed until a project manager approves the task plan.',
        N'Proposed', N'Critical', DATEADD(DAY, 8, @Today), 18, N'AI,Kitchen', N'AiProposed', @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE TaskId = 'A7000001-0000-4000-8000-000000000008')
    INSERT INTO dbo.Tasks
        (TaskId, ProjectId, MilestoneId, RequirementId, Title, Description, Status, Priority, DueDate, EstimatedHours, Labels, Source, CreatedBy)
    VALUES
    (
        'A7000001-0000-4000-8000-000000000008', @P1,
        'F6000001-0000-4000-8000-000000000002', 'E5000001-0000-4000-8000-000000000003',
        N'Place bedroom and passage storage',
        N'AI plan. Not assigned and not on the active board.',
        N'Proposed', N'High', DATEADD(DAY, 10, @Today), 12, N'AI,Storage', N'AiProposed', @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE TaskId = 'A7000001-0000-4000-8000-000000000009')
    INSERT INTO dbo.Tasks
        (TaskId, ProjectId, MilestoneId, Title, Description, Status, Priority, DueDate, EstimatedHours, Labels, Source, CreatedBy)
    VALUES
    (
        'A7000001-0000-4000-8000-000000000009', @P1,
        'F6000001-0000-4000-8000-000000000002',
        N'Check door fire-rating parameters before issue',
        N'AI plan generated from the validation whitelist. Requires manager approval.',
        N'Proposed', N'High', DATEADD(DAY, 12, @Today), 4, N'AI,QA', N'AiProposed', @Arjun
    );
GO
