/*
    Three projects. NW-2BHK-001 is inserted as Active; the failed-validation
    procedure later moves it to NeedsAttention.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Admin UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000001';
DECLARE @Maya  UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000002';
DECLARE @Arjun UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000003';
DECLARE @Lena  UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000004';
DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

IF NOT EXISTS (SELECT 1 FROM dbo.Projects WHERE Code = N'NW-2BHK-001')
    INSERT INTO dbo.Projects
    (
        ProjectId, ClientId, Code, Name, Description, Status, StartDate, DueDate,
        Budget, Spent, Currency, SiteAddress, ProjectManagerId, LeadDesignerId, CreatedBy
    )
    VALUES
    (
        'D4000001-0000-4000-8000-000000000001',
        'C3000001-0000-4000-8000-000000000001',
        N'NW-2BHK-001',
        N'Sharma Residence — Modern 2BHK',
        N'Modern 2BHK with a modular kitchen and full-height storage. Warm oak, limestone, and matte black metal.',
        N'Active',
        DATEADD(DAY, -36, @Today),
        DATEADD(DAY, 48, @Today),
        4200000.00, 1680000.00, 'INR',
        N'1402, Palm Court, Bandra West, Mumbai',
        @Maya, @Arjun, @Admin
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Projects WHERE Code = N'HB-LOFT-014')
    INSERT INTO dbo.Projects
    (
        ProjectId, ClientId, Code, Name, Description, Status, StartDate, DueDate,
        Budget, Spent, Currency, SiteAddress, ProjectManagerId, LeadDesignerId, CreatedBy
    )
    VALUES
    (
        'D4000001-0000-4000-8000-000000000002',
        'C3000001-0000-4000-8000-000000000002',
        N'HB-LOFT-014',
        N'Harbor Loft Renovation',
        N'Open loft with a mezzanine lounge, cove lighting, and loose furniture package.',
        N'Active',
        DATEADD(DAY, -60, @Today),
        DATEADD(DAY, 20, @Today),
        8900000.00, 3100000.00, 'INR',
        N'Level 6, Indiranagar, Bengaluru',
        @Maya, @Lena, @Admin
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Projects WHERE Code = N'LM-LOBBY-007')
    INSERT INTO dbo.Projects
    (
        ProjectId, ClientId, Code, Name, Description, Status, StartDate, DueDate,
        Budget, Spent, Currency, SiteAddress, ProjectManagerId, LeadDesignerId, CreatedBy
    )
    VALUES
    (
        'D4000001-0000-4000-8000-000000000003',
        'C3000001-0000-4000-8000-000000000003',
        N'LM-LOBBY-007',
        N'Lumen Lobby Concept',
        N'Hotel lobby concept: reception, lounge, and feature ceiling. Design development not started.',
        N'InReview',
        DATEADD(DAY, -12, @Today),
        DATEADD(DAY, 70, @Today),
        15000000.00, 2200000.00, 'INR',
        N'1 Convention Road, Hyderabad',
        @Maya, @Arjun, @Admin
    );
GO
