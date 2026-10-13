/*
    Brief for the Sharma residence, including one requirement still Proposed.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

DECLARE @Admin UNIQUEIDENTIFIER = 'B2000001-0000-4000-8000-000000000001';
DECLARE @Project UNIQUEIDENTIFIER = 'D4000001-0000-4000-8000-000000000001';
DECLARE @Analysis NVARCHAR(MAX) = N'{"brief":"modern 2BHK with modular kitchen and storage","rooms":["Living and dining","Master bedroom","Bedroom 2","Modular kitchen","Two bathrooms"],"style":"Warm modern","materials":["Oak","Limestone","Matte black metal"],"constraints":["Wet areas cannot move","Ceiling height 2.7m"],"budgetCurrency":"INR"}';

IF NOT EXISTS (SELECT 1 FROM dbo.ProjectRequirements WHERE RequirementId = 'E5000001-0000-4000-8000-000000000001')
    INSERT INTO dbo.ProjectRequirements
        (RequirementId, ProjectId, Title, Description, Category, Priority, Source, Status, SortOrder, CreatedBy)
    VALUES
    (
        'E5000001-0000-4000-8000-000000000001', @Project,
        N'Open living and dining',
        N'Keep living and dining as one room. Sofa faces the window wall.',
        N'Room', N'High', N'ClientBrief', N'Approved', 1, @Admin
    );

IF NOT EXISTS (SELECT 1 FROM dbo.ProjectRequirements WHERE RequirementId = 'E5000001-0000-4000-8000-000000000002')
    INSERT INTO dbo.ProjectRequirements
        (RequirementId, ProjectId, Title, Description, Category, Priority, Source, Status, AiAnalysisJson, SortOrder, CreatedBy)
    VALUES
    (
        'E5000001-0000-4000-8000-000000000002', @Project,
        N'Modular kitchen',
        N'Tall unit, breakfast niche, and concealed appliances. Shutters in matte oak.',
        N'Room', N'Critical', N'AiProposed', N'Approved', @Analysis, 2, @Admin
    );

IF NOT EXISTS (SELECT 1 FROM dbo.ProjectRequirements WHERE RequirementId = 'E5000001-0000-4000-8000-000000000003')
    INSERT INTO dbo.ProjectRequirements
        (RequirementId, ProjectId, Title, Description, Category, Priority, Source, Status, SortOrder, CreatedBy)
    VALUES
    (
        'E5000001-0000-4000-8000-000000000003', @Project,
        N'Full-height storage',
        N'Storage wall in the passage and wardrobes in both bedrooms.',
        N'Storage', N'High', N'AiProposed', N'Approved', 3, @Admin
    );

IF NOT EXISTS (SELECT 1 FROM dbo.ProjectRequirements WHERE RequirementId = 'E5000001-0000-4000-8000-000000000004')
    INSERT INTO dbo.ProjectRequirements
        (RequirementId, ProjectId, Title, Description, Category, Priority, Source, Status, SortOrder, CreatedBy)
    VALUES
    (
        'E5000001-0000-4000-8000-000000000004', @Project,
        N'Warm modern palette',
        N'Oak, limestone, and matte black metal. No high-gloss laminate.',
        N'Style', N'Medium', N'ClientBrief', N'Approved', 4, @Admin
    );

IF NOT EXISTS (SELECT 1 FROM dbo.ProjectRequirements WHERE RequirementId = 'E5000001-0000-4000-8000-000000000005')
    INSERT INTO dbo.ProjectRequirements
        (RequirementId, ProjectId, Title, Description, Category, Priority, Source, Status, SortOrder, CreatedBy)
    VALUES
    (
        'E5000001-0000-4000-8000-000000000005', @Project,
        N'Acoustic treatment in living',
        N'AI suggestion from the brief. Waiting for the client to confirm before it becomes scope.',
        N'Constraint', N'Medium', N'AiProposed', N'Proposed', 5, @Admin
    );
GO
