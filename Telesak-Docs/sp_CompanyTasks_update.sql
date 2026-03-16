-- =====================================================================
-- Sprint 1: Task Report Improvements
-- SQL Server 2008 R2 - Ready to run
-- =====================================================================

ALTER proc [dbo].[CompanyTasks]
@CompanyId int,
@CalendarType int = 0
AS
SELECT t.Title task, p.Name project, pr.Name priority, s.Name [status],
    e.Employee_ID, e.Employee_Name,
    CASE
        WHEN @CalendarType = 1
        THEN CONVERT(NVARCHAR(10), t.StartDate, 111)
        ELSE SUBSTRING(CAST([dbo].[GetHijriDate](t.StartDate) AS NVARCHAR(50)), 1, 10)
    END AS StartDate,
    CASE
        WHEN @CalendarType = 1
        THEN CONVERT(NVARCHAR(10), t.EndDate, 111)
        ELSE SUBSTRING(CAST([dbo].[GetHijriDate](t.EndDate) AS NVARCHAR(50)), 1, 10)
    END AS EndDate,
    t.ActualTime,
    t.TaskID
FROM Task t
    LEFT JOIN Project p ON t.ProjectID = p.ProjectID
    JOIN [Status] s ON t.StatusID = s.StatusID
    JOIN Priority pr ON t.PriorityID = pr.PriorityID
    LEFT JOIN Employee e ON t.EmployeeID = e.Employee_ID
WHERE t.CompanyID = @CompanyId
