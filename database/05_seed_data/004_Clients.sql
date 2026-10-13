/*
    Sample clients. PortalUserId points at the matching Client-role login.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Admin UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000001';

IF NOT EXISTS (SELECT 1 FROM dbo.Clients WHERE ClientId = 'C3000001-0000-4000-8000-000000000001')
    INSERT INTO dbo.Clients
    (
        ClientId, CompanyName, ContactName, Email, Phone, Website,
        AddressLine1, City, State, PostalCode, Country, Status, Notes, PortalUserId, CreatedBy
    )
    VALUES
    (
        'C3000001-0000-4000-8000-000000000001',
        N'Northwind Residences', N'Rahul Sharma', N'rahul.sharma@northwind.example', N'+91 98200 22001',
        N'https://northwind.example', N'1402, Palm Court, Bandra West', N'Mumbai', N'Maharashtra', N'400050', N'India',
        N'Active', N'Residential apartment. Decision maker is the owner.',
        'B2000001-0000-4000-8000-000000000005', @Admin
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Clients WHERE ClientId = 'C3000001-0000-4000-8000-000000000002')
    INSERT INTO dbo.Clients
    (
        ClientId, CompanyName, ContactName, Email, Phone, Website,
        AddressLine1, City, State, PostalCode, Country, Status, Notes, PortalUserId, CreatedBy
    )
    VALUES
    (
        'C3000001-0000-4000-8000-000000000002',
        N'Harbor & Co.', N'Sofia Alvarez', N'sofia.alvarez@harborandco.example', N'+91 98200 22002',
        N'https://harborandco.example', N'Level 6, Indiranagar', N'Bengaluru', N'Karnataka', N'560038', N'India',
        N'Active', N'Loft renovation for a hospitality brand studio.',
        'B2000001-0000-4000-8000-000000000006', @Admin
    );

IF NOT EXISTS (SELECT 1 FROM dbo.Clients WHERE ClientId = 'C3000001-0000-4000-8000-000000000003')
    INSERT INTO dbo.Clients
    (
        ClientId, CompanyName, ContactName, Email, Phone,
        AddressLine1, City, State, PostalCode, Country, Status, Notes, CreatedBy
    )
    VALUES
    (
        'C3000001-0000-4000-8000-000000000003',
        N'Lumen Hospitality', N'Priya Nair', N'projects@lumen.example', N'+91 98200 22003',
        N'1 Convention Road', N'Hyderabad', N'Telangana', N'500081', N'India',
        N'Prospect', N'Lobby concept only. Portal login not issued yet.', @Admin
    );
GO
