/*
    One failed run (Sharma) and one passed run (Harbor).
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Arjun UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000003';
DECLARE @Lena  UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000004';

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationRuns WHERE ValidationRunId = 'C9000001-0000-4000-8000-000000000001')
    INSERT INTO dbo.ValidationRuns
    (
        ValidationRunId, RevitModelId, ProjectId, TriggeredById, Status,
        StartedAt, CompletedAt, Summary, ChecksPassed, ChecksFailed, CreatedBy
    )
    VALUES
    (
        'C9000001-0000-4000-8000-000000000001',
        'B8000001-0000-4000-8000-000000000001',
        'D4000001-0000-4000-8000-000000000001',
        @Arjun, N'Failed',
        DATEADD(HOUR, -6, SYSUTCDATETIME()),
        DATEADD(HOUR, -5, SYSUTCDATETIME()),
        N'2 checks failed: doors missing fire rating, rooms without names.',
        2, 2, @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.ValidationRuns WHERE ValidationRunId = 'C9000001-0000-4000-8000-000000000002')
    INSERT INTO dbo.ValidationRuns
    (
        ValidationRunId, RevitModelId, ProjectId, TriggeredById, Status,
        StartedAt, CompletedAt, Summary, ChecksPassed, ChecksFailed, CreatedBy
    )
    VALUES
    (
        'C9000001-0000-4000-8000-000000000002',
        'B8000001-0000-4000-8000-000000000002',
        'D4000001-0000-4000-8000-000000000002',
        @Lena, N'Passed',
        DATEADD(DAY, -1, SYSUTCDATETIME()),
        DATEADD(HOUR, -20, SYSUTCDATETIME()),
        N'All four interior checks passed.',
        4, 0, @Lena
    );
GO
