-- =====================================================================
-- Bug #85: تقرير مهام كل مشروع showed task start/end dates only in Hijri.
-- Add @CalendarType (1 = Gregorian yyyy/MM/dd, 0 = Hijri) — same pattern
-- as sp_CompanyTasks_update.sql. The default keeps the old Hijri output,
-- so run this BEFORE deploying the web build that passes @CalendarType.
-- Run in SSMS against the target database (SQL Server 2008 R2 compatible).
-- =====================================================================

ALTER PROCEDURE [dbo].[sp_ProjectTasks]
 @CompanyId int,
 @ProjectId int,
 @EmployeeId int,
 @CalendarType int = 0
AS
BEGIN
 select t.TaskID, t.Title task, p.Name project, emps.[Employee Name] [EmpName], pr.Name priority, s.Name [status],
 CASE
     WHEN @CalendarType = 1
     THEN CONVERT(NVARCHAR(10), t.StartDate, 111)
     ELSE SUBSTRING(CAST([dbo].[GetHijriDate](t.StartDate) AS NVARCHAR(50)), 1, 10)
 END AS StartDate,
 CASE
     WHEN @CalendarType = 1
     THEN CONVERT(NVARCHAR(10), t.EndDate, 111)
     ELSE SUBSTRING(CAST([dbo].[GetHijriDate](t.EndDate) AS NVARCHAR(50)), 1, 10)
 END AS EndDate

from Task t left join Project p on t.ProjectID = p.ProjectID
            left join [Status] s on t.StatusID = s.StatusID
            left join Priority pr on t.PriorityID = pr.PriorityID
            left join EmployeesNames emps on t.EmpID = emps.[employee Id]

where t.CompanyID = case when @CompanyId = 0 then t.CompanyID else @CompanyId end and
      p.ProjectID like case when @ProjectId = 0 then p.ProjectID else @ProjectId end
and t.EmpID = case when  (CONVERT(NVarchar(10),@EmployeeId)='0' ) then t.EmpID else  CONVERT(NVarchar(10),@EmployeeId) end
and t.IsDeleted=0 and t.StatusID != 9
order by t.CreatedDate desc
END
GO

-- Verify (replace ids):
-- EXEC sp_ProjectTasks @CompanyId = 1, @ProjectId = 0, @EmployeeId = 0, @CalendarType = 1  -- Gregorian
-- EXEC sp_ProjectTasks @CompanyId = 1, @ProjectId = 0, @EmployeeId = 0                     -- Hijri (old output)
