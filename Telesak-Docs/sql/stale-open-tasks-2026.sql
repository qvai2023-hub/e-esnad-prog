/* =============================================================================
   Open tasks created in 2026 with no movement - the live stale set

   READ ONLY. Safe on production. One row per stale open task with everything
   needed to decide what to do with it (close, reassign, or leave).

   Open = current status New (1) / Inprogress (2) / Accepted (8) / Delay (10)
   Stale = no TaskTLog row for @IdleDays days or more

   Parameters below: the creation window and the idle threshold.
   TARGET ENGINE: SQL Server 2008 R2.
   ============================================================================= */

DECLARE @CreatedFrom DATETIME = '2026-01-01';   -- <-- era start
DECLARE @CreatedTo   DATETIME = '2027-01-01';
DECLARE @IdleDays    INT      = 60;             -- <-- stale threshold
DECLARE @Now         DATETIME = GETDATE();

/* 1. Scale first: how many, held by how many employees / companies */
SELECT  'Summary' AS Section,
        COUNT(*) AS StaleOpenTasks,
        COUNT(DISTINCT t.EmpID) AS Employees,
        COUNT(DISTINCT t.CompanyID) AS Companies,
        SUM(CASE WHEN t.EmpID IS NULL THEN 1 ELSE 0 END) AS Unassigned
FROM    dbo.Task t
JOIN   (SELECT l.TaskID, MAX(l.CreatedDate) AS LastMove
        FROM dbo.TaskTLog l GROUP BY l.TaskID) x ON x.TaskID = t.TaskID
WHERE   t.IsDeleted = 0
  AND   t.StatusID IN (1, 2, 8, 10)
  AND   t.CreatedDate >= @CreatedFrom AND t.CreatedDate < @CreatedTo
  AND   DATEDIFF(DAY, x.LastMove, @Now) >= @IdleDays;

/* 2. The list itself */
SELECT  t.TaskID,
        t.Title                                         AS TaskName,
        s.Name                                          AS StatusName,
        t.StatusID,
        t.EmpID,
        e.Name                                          AS EmployeeName,
        CAST(ISNULL(e.IsActive, 0) AS BIT)              AS EmployeeIsActive,
        CAST(ISNULL(e.IsDeleted, 0) AS BIT)             AS EmployeeIsDeleted,
        t.CompanyID,
        c.Name                                          AS CompanyName,
        CAST(ISNULL(c.IsActive, 0) AS BIT)              AS CompanyIsActive,
        CAST(ISNULL(c.IsDeleted, 0) AS BIT)             AS CompanyIsDeleted,
        ua.IsTelesak,                                   -- 1 Telesak / 0 Esnad / NULL no account
        t.CreatedDate,
        t.StartDate                                     AS PlannedStart,
        t.EndDate                                       AS PlannedEnd,
        x.LastMove                                      AS LastMovement,
        DATEDIFF(DAY, x.LastMove, @Now)                 AS IdleDays,
        t.ExpectedTime,
        tu.Name                                         AS TimeUnitName,
        t.ActualTime
FROM    dbo.Task t
JOIN   (SELECT l.TaskID, MAX(l.CreatedDate) AS LastMove
        FROM dbo.TaskTLog l GROUP BY l.TaskID) x ON x.TaskID = t.TaskID
LEFT JOIN dbo.Status   s  ON s.StatusID     = t.StatusID
LEFT JOIN dbo.Employee e  ON e.EmpID        = t.EmpID
LEFT JOIN dbo.Company  c  ON c.CompanyID    = t.CompanyID
LEFT JOIN dbo.TimeUnit tu ON tu.TimeUnitID  = t.TimeUnitID
LEFT JOIN (SELECT EmpID, MAX(CAST(IsTelesak AS INT)) AS IsTelesak
           FROM dbo.UserAccount GROUP BY EmpID) ua ON ua.EmpID = t.EmpID
WHERE   t.IsDeleted = 0
  AND   t.StatusID IN (1, 2, 8, 10)
  AND   t.CreatedDate >= @CreatedFrom AND t.CreatedDate < @CreatedTo
  AND   DATEDIFF(DAY, x.LastMove, @Now) >= @IdleDays
ORDER BY IdleDays DESC, t.CompanyID, t.EmpID;

/* 3. Grouped view: who holds how much (for the companies follow-up) */
SELECT  t.CompanyID, c.Name AS CompanyName, t.EmpID, e.Name AS EmployeeName,
        CAST(ISNULL(e.IsActive, 0) AS BIT) AS EmployeeIsActive,
        COUNT(*) AS StaleTasks,
        MIN(x.LastMove) AS OldestMovement
FROM    dbo.Task t
JOIN   (SELECT l.TaskID, MAX(l.CreatedDate) AS LastMove
        FROM dbo.TaskTLog l GROUP BY l.TaskID) x ON x.TaskID = t.TaskID
LEFT JOIN dbo.Employee e ON e.EmpID = t.EmpID
LEFT JOIN dbo.Company  c ON c.CompanyID = t.CompanyID
WHERE   t.IsDeleted = 0
  AND   t.StatusID IN (1, 2, 8, 10)
  AND   t.CreatedDate >= @CreatedFrom AND t.CreatedDate < @CreatedTo
  AND   DATEDIFF(DAY, x.LastMove, @Now) >= @IdleDays
GROUP BY t.CompanyID, c.Name, t.EmpID, e.Name, CAST(ISNULL(e.IsActive, 0) AS BIT)
ORDER BY COUNT(*) DESC;
