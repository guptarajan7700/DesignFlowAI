/*
    Active tasks whose due date is before today.
    Proposed AI tasks are excluded until a manager approves them.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_OverdueTasks
AS
SELECT
    t.TaskId,
    t.ProjectId,
    p.Code AS ProjectCode,
    p.Name AS ProjectName,
    t.Title,
    t.Status,
    t.Priority,
    t.DueDate,
    t.AssigneeId,
    LTRIM(RTRIM(CONCAT(u.FirstName, N' ', u.LastName))) AS AssigneeName
FROM dbo.Tasks AS t
INNER JOIN dbo.Projects AS p ON p.ProjectId = t.ProjectId
LEFT JOIN dbo.Users AS u ON u.UserId = t.AssigneeId
WHERE t.Status IN (N'ToDo', N'InProgress', N'InReview')
  AND t.DueDate < CAST(SYSUTCDATETIME() AS DATE);
GO
