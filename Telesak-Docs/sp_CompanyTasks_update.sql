-- =====================================================================
-- Sprint 1: Task Report Improvements
-- Update sp_CompanyTasks to support @CalendarType parameter
-- Run this in SSMS against the production database
-- =====================================================================

ALTER PROCEDURE [dbo].[sp_CompanyTasks]
@CompanyId int =0,
@ProjectIds nvarchar(MAX)='',
@EmpId nvarchar(MAX)='',
@notInEmpList int=0,
@notInProjectList int=0,
@StatusId nvarchar(32)='',
@PriorityId nvarchar(5)='',
@FromDate datetime=null,
@ToDate datetime=null,
@taskName nvarchar(20)='',
@FromEndDate datetime=null,
@ToEndDate datetime=null,
@CalendarType int=0
AS
BEGIN
select t.TaskID, t.Title task, p.Name project,emps.[employee Id] as Employee_ID, emps.[Employee Name]  as Employee_Name, pr.Name priority, s.Name [status],
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
 case when t.ActualTime > 0 then t.ActualTime else SUM(isnull(tl.TimeCount,0)) end ActualTime, tu.Name timeUnit

from Task t   left join Project p on t.ProjectID = p.ProjectID
            left join [Status] s on t.StatusID = s.StatusID
            left join Priority pr on t.PriorityID = pr.PriorityID
            left join TimeUnit tu on t.TimeUnitID=tu.TimeUnitID
            left join TaskTLog tl on tl.TaskID = t.TaskID
            left join EmployeesNames emps on t.EmpID = emps.[employee Id] and t.companyid=emps.[company id]


where t.IsDeleted=0 and ((@CompanyId=0) or (t.CompanyID = @CompanyId))  and
   ((@ProjectIds = '')
   or (@notInProjectList = 0 and @ProjectIds='-2' and t.ProjectID is null)
   or (@notInProjectList = 1 and t.ProjectID not in(select * from dbo.fnSplitStringAsTable(@ProjectIds,','))  OR t.ProjectID IS NULL)
   or (@notInProjectList = 0 and t.ProjectID in (select * from dbo.fnSplitStringAsTable(@ProjectIds,','))))and

      ((@EmpId = '')
      or (@notInEmpList = 0 and @EmpId = '-2' and t.EmpID is null)
      or (@notInEmpList = 1 and t.EmpID not in(select * from dbo.fnSplitStringAsTable(@EmpId,',')))
      or (@notInEmpList = 0 and t.EmpID in (select * from dbo.fnSplitStringAsTable(@EmpId,','))))and

      ((@StatusId = '')
      or (@StatusId='-1' and (t.EndDate <  case when t.DeliverDate is not null then t.DeliverDate else CONVERT(date, getdate()) end)and t.StatusID != 1 )
      or (t.StatusID in (select * from dbo.fnSplitStringAsTable(@StatusId,',')))

      )and

      ((@PriorityId = '') or (t.PriorityID in (select * from dbo.fnSplitStringAsTable(@PriorityId,','))))and

      ((@FromDate IS NULL) or (@FromDate<>'' and t.StartDate >= @FromDate)) and

      ((@ToDate IS NULL) or (@ToDate<>'' and t.StartDate <= @ToDate))
    and
   ((@FromEndDate IS NULL) or (@FromEndDate<>'' and  t.EndDate >= @FromEndDate)) and

      ((@ToEndDate IS NULL) or (@ToEndDate<>'' and t.EndDate <= @ToEndDate))
   and

      ((@taskName = '')or(t.Title like '%'+@taskName+'%'))
group by  t.TaskID,t.Title,t.CreatedDate, p.Name, pr.Name,emps.[employee Id], emps.[Employee Name], s.Name, t.StartDate,t.EndDate, t.ActualTime, tu.Name, t.DeliverDate
order by t.CreatedDate desc
END
