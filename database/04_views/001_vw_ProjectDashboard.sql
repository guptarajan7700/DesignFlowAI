/*
    Project list with client, leads, task progress, overdue work, and open issues.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_ProjectDashboard
AS
SELECT
    p.ProjectId,
    p.Code,
    p.Name,
    p.Status,
    p.Budget,
    p.Spent,
    p.Currency,
    p.StartDate,
    p.DueDate,
    c.ClientId,
    c.CompanyName AS ClientName,
    pm.UserId AS ProjectManagerId,
    LTRIM(RTRIM(CONCAT(pm.FirstName, N' ', pm.LastName))) AS ProjectManagerName,
    ld.UserId AS LeadDesignerId,
    LTRIM(RTRIM(CONCAT(ld.FirstName, N' ', ld.LastName))) AS LeadDesignerName,
    (
        SELECT COUNT(*)
        FROM dbo.Tasks AS t
        WHERE t.ProjectId = p.ProjectId
          AND t.Status NOT IN (N'Proposed', N'Rejected', N'Cancelled')
    ) AS ActiveTaskCount,
    (
        SELECT COUNT(*)
        FROM dbo.Tasks AS t
        WHERE t.ProjectId = p.ProjectId
          AND t.Status = N'Done'
    ) AS CompletedTaskCount,
    (
        SELECT COUNT(*)
        FROM dbo.Tasks AS t
        WHERE t.ProjectId = p.ProjectId
          AND t.Status IN (N'ToDo', N'InProgress', N'InReview')
          AND t.DueDate < CAST(SYSUTCDATETIME() AS DATE)
    ) AS OverdueTaskCount,
    (
        SELECT COUNT(*)
        FROM dbo.Issues AS i
        WHERE i.ProjectId = p.ProjectId
          AND i.Status IN (N'Open', N'Assigned', N'InProgress')
    ) AS OpenIssueCount
FROM dbo.Projects AS p
INNER JOIN dbo.Clients AS c ON c.ClientId = p.ClientId
LEFT JOIN dbo.Users AS pm ON pm.UserId = p.ProjectManagerId
LEFT JOIN dbo.Users AS ld ON ld.UserId = p.LeadDesignerId;
GO
