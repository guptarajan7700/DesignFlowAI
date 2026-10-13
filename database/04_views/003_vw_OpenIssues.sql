/*
    Issues that still need a designer or a resolution.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_OpenIssues
AS
SELECT
    i.IssueId,
    i.ProjectId,
    p.Code AS ProjectCode,
    p.Name AS ProjectName,
    i.ValidationRunId,
    i.RevitModelId,
    i.Title,
    i.Severity,
    i.Status,
    i.ElementId,
    i.AssignedToId,
    LTRIM(RTRIM(CONCAT(u.FirstName, N' ', u.LastName))) AS AssignedToName,
    i.CreatedAt
FROM dbo.Issues AS i
INNER JOIN dbo.Projects AS p ON p.ProjectId = i.ProjectId
LEFT JOIN dbo.Users AS u ON u.UserId = i.AssignedToId
WHERE i.Status IN (N'Open', N'Assigned', N'InProgress');
GO
