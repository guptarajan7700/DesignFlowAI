/*
    Jobs, an issued revision, a pending task-plan approval, an AI command preview,
    an audit row, and a notification asking Maya to approve the AI plan.
    The approve procedure is intentionally not called.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Maya  UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000002';
DECLARE @Arjun UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000003';
DECLARE @Lena  UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000004';
DECLARE @P1 UNIQUEIDENTIFIER = 'D4000001-0000-4000-8000-000000000001';
DECLARE @P2 UNIQUEIDENTIFIER = 'D4000001-0000-4000-8000-000000000002';
DECLARE @Approval UNIQUEIDENTIFIER = 'A9400001-0000-4000-8000-000000000001';

IF NOT EXISTS (SELECT 1 FROM dbo.AutomationJobs WHERE AutomationJobId = 'E9200001-0000-4000-8000-000000000001')
    INSERT INTO dbo.AutomationJobs
    (
        AutomationJobId, ProjectId, RevitModelId, JobType, Status, ProgressPercent,
        ExternalWorkItemId, RequestPayload, ResultPayload, StartedAt, CompletedAt, CreatedBy
    )
    VALUES
    (
        'E9200001-0000-4000-8000-000000000001', @P1, 'B8000001-0000-4000-8000-000000000001',
        N'TranslateModel', N'Succeeded', 100, N'local-seed-workitem-001',
        N'{"source":"Sharma_Residence_2BHK.rvt"}',
        N'{"viewable":false,"reason":"APS credentials are not configured in this seed."}',
        DATEADD(HOUR, -10, SYSUTCDATETIME()),
        DATEADD(HOUR, -9, SYSUTCDATETIME()),
        @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.AutomationJobs WHERE AutomationJobId = 'E9200001-0000-4000-8000-000000000002')
    INSERT INTO dbo.AutomationJobs
        (AutomationJobId, ProjectId, RevitModelId, JobType, Status, ProgressPercent, RequestPayload, CreatedBy)
    VALUES
    (
        'E9200001-0000-4000-8000-000000000002', @P2, 'B8000001-0000-4000-8000-000000000002',
        N'DesignAutomation', N'Queued', 0,
        N'{"activity":"ExportSheets","note":"Stub until Design Automation is implemented."}',
        @Lena
    );

IF NOT EXISTS (SELECT 1 FROM dbo.DrawingRevisions WHERE DrawingRevisionId = 'F9300001-0000-4000-8000-000000000001')
    INSERT INTO dbo.DrawingRevisions
    (
        DrawingRevisionId, ProjectId, RevitModelId, SheetNumber, SheetName,
        RevisionCode, Description, IssuedAt, IssuedById, CreatedBy
    )
    VALUES
    (
        'F9300001-0000-4000-8000-000000000001', @P2, 'B8000001-0000-4000-8000-000000000002',
        N'A-201', N'Furniture Plan', N'A',
        N'Issued for coordination. Cove lighting still open.',
        DATEADD(DAY, -7, SYSUTCDATETIME()), @Lena, @Lena
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Approvals WHERE ApprovalId = @Approval)
    INSERT INTO dbo.Approvals
    (
        ApprovalId, ProjectId, EntityType, EntityId, Status,
        RequestedById, Comments, RequestedAt, CreatedBy
    )
    VALUES
    (
        @Approval, @P1, N'TaskPlan', @P1, N'Pending', @Arjun,
        N'Three AI tasks from the 2BHK brief are waiting. They are not active work yet.',
        DATEADD(HOUR, -4, SYSUTCDATETIME()), @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.AiCommandLogs WHERE AiCommandLogId = 'B9500001-0000-4000-8000-000000000001')
    INSERT INTO dbo.AiCommandLogs
    (
        AiCommandLogId, UserId, ProjectId, Prompt, DetectedCommand,
        ParametersJson, PreviewJson, Status, Provider, CreatedBy
    )
    VALUES
    (
        'B9500001-0000-4000-8000-000000000001', @Arjun, @P1,
        N'Find doors missing a fire-rating parameter on the Sharma residence model.',
        N'FindDoorsMissingFireRating',
        N'{"modelId":"B8000001-0000-4000-8000-000000000001","level":"Level 1"}',
        N'{"command":"FindDoorsMissingFireRating","impact":"Read-only search. No model elements will be changed.","requiresConfirmation":true}',
        N'Previewed', N'Mock', @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.AuditLogs WHERE AuditLogId = 'C9600001-0000-4000-8000-000000000001')
    INSERT INTO dbo.AuditLogs
        (AuditLogId, UserId, EntityType, EntityId, Action, NewValues, IpAddress, CreatedBy)
    VALUES
    (
        'C9600001-0000-4000-8000-000000000001', @Maya, N'Project', @P1, N'Created',
        N'{"code":"NW-2BHK-001"}', N'127.0.0.1', @Maya
    );

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Notifications
    WHERE Type = N'ApprovalRequested'
      AND RelatedEntityId = @Approval
)
    INSERT INTO dbo.Notifications
    (
        NotificationId, UserId, ProjectId, Title, Message, Type,
        IsRead, RelatedEntityType, RelatedEntityId, CreatedBy
    )
    VALUES
    (
        'C9700001-0000-4000-8000-000000000001', @Maya, @P1,
        N'AI task plan needs approval',
        N'Arjun submitted three proposed tasks for the Sharma residence. Approve them before they join the board.',
        N'ApprovalRequested', 0, N'Approval', @Approval, @Arjun
    );
GO
