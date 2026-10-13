/*
    Synced Revit models. Autodesk URNs are null until APS credentials exist.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Arjun UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000003';
DECLARE @Lena  UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000004';

IF NOT EXISTS (SELECT 1 FROM dbo.RevitModels WHERE RevitModelId = 'B8000001-0000-4000-8000-000000000001')
    INSERT INTO dbo.RevitModels
        (RevitModelId, ProjectId, Name, FileName, VersionLabel, LastSyncedAt, Status, CreatedBy)
    VALUES
    (
        'B8000001-0000-4000-8000-000000000001',
        'D4000001-0000-4000-8000-000000000001',
        N'Sharma Residence model', N'Sharma_Residence_2BHK.rvt', N'2026.1',
        DATEADD(HOUR, -8, SYSUTCDATETIME()), N'Ready', @Arjun
    );

IF NOT EXISTS (SELECT 1 FROM dbo.RevitModels WHERE RevitModelId = 'B8000001-0000-4000-8000-000000000002')
    INSERT INTO dbo.RevitModels
        (RevitModelId, ProjectId, Name, FileName, VersionLabel, LastSyncedAt, Status, CreatedBy)
    VALUES
    (
        'B8000001-0000-4000-8000-000000000002',
        'D4000001-0000-4000-8000-000000000002',
        N'Harbor Loft model', N'Harbor_Loft.rvt', N'2026.1',
        DATEADD(HOUR, -30, SYSUTCDATETIME()), N'Synced', @Lena
    );
GO
