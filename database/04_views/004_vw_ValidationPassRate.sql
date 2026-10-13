/*
    Pass rate across finished validation runs. Used by the dashboard KPI.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER VIEW dbo.vw_ValidationPassRate
AS
SELECT
    COALESCE(SUM(vr.ChecksPassed), 0) AS ChecksPassed,
    COALESCE(SUM(vr.ChecksFailed), 0) AS ChecksFailed,
    CASE
        WHEN COALESCE(SUM(vr.ChecksPassed + vr.ChecksFailed), 0) = 0 THEN CAST(0 AS DECIMAL(5, 2))
        ELSE CAST(
            100.0 * SUM(vr.ChecksPassed) / SUM(vr.ChecksPassed + vr.ChecksFailed)
            AS DECIMAL(5, 2)
        )
    END AS PassRatePercent
FROM dbo.ValidationRuns AS vr
WHERE vr.Status IN (N'Passed', N'Failed');
GO
