/* =============================================================================
   Evidence for the Assigned-Tasks definition - run on the PRODUCTION database

   READ ONLY: SELECTs only, no temp tables, nothing is written or locked beyond
   shared reads. Safe to run during working hours; the heaviest query aggregates
   TaskTLog once.

   Parameters:
       @Since   "recent" cutoff, so old data does not drown the current behaviour
       @Now     the reference moment for staleness (default: now)

   What each section shows:
     1 / 1b  Does the planned EndDate match the actual finish day? (claim 1)
     2 / 2b  Does every logged minute sit on a status that the corrected
             Assigned rule counts (1,2,5,8,10)? (numerator = denominator claim)
     3       Open tasks by how long they have had no movement (the stale set)
     3b      The same split by provider of the current assignee
     3c      The 20 stalest open tasks, to look at with your own eyes
     4       Open tasks whose planned period is already over - the set the
             proposed hybrid rule would stop counting while they stay idle

   TARGET ENGINE: SQL Server 2008 R2.
   ============================================================================= */

DECLARE @Since DATETIME = '2026-08-01';   -- <-- adjust if needed
DECLARE @Now   DATETIME = GETDATE();

/* ---------------------------------------------------------------------------
   1. Planned EndDate vs the actual finish day, all finished tasks
   ------------------------------------------------------------------------- */
SELECT  '1 - planned EndDate vs actual finish (all)' AS Section,
        COUNT(*) AS FinishedTasks,
        SUM(CASE WHEN t.EndDate IS NULL THEN 1 ELSE 0 END) AS EndDateNull,
        SUM(CASE WHEN t.EndDate IS NOT NULL AND DATEDIFF(DAY, d.LastDone, t.EndDate) = 0 THEN 1 ELSE 0 END) AS SameDayAsFinish,
        SUM(CASE WHEN t.EndDate IS NOT NULL AND DATEDIFF(DAY, d.LastDone, t.EndDate) < 0 THEN 1 ELSE 0 END) AS FinishedAfterPlannedEnd,
        SUM(CASE WHEN t.EndDate IS NOT NULL AND DATEDIFF(DAY, d.LastDone, t.EndDate) > 0 THEN 1 ELSE 0 END) AS FinishedBeforePlannedEnd
FROM    dbo.Task t
JOIN   (SELECT TaskID, MAX(CreatedDate) AS LastDone
        FROM dbo.TaskTLog WHERE StatusID = 5 GROUP BY TaskID) d ON d.TaskID = t.TaskID
WHERE   t.IsDeleted = 0;

-- 1b. The same, tasks finished since @Since (current behaviour only)
SELECT  '1b - planned EndDate vs actual finish (recent)' AS Section,
        COUNT(*) AS FinishedTasks,
        SUM(CASE WHEN t.EndDate IS NOT NULL AND DATEDIFF(DAY, d.LastDone, t.EndDate) = 0 THEN 1 ELSE 0 END) AS SameDayAsFinish,
        SUM(CASE WHEN t.EndDate IS NULL OR DATEDIFF(DAY, d.LastDone, t.EndDate) <> 0 THEN 1 ELSE 0 END) AS DifferentDay
FROM    dbo.Task t
JOIN   (SELECT TaskID, MAX(CreatedDate) AS LastDone
        FROM dbo.TaskTLog WHERE StatusID = 5 GROUP BY TaskID) d ON d.TaskID = t.TaskID
WHERE   t.IsDeleted = 0 AND d.LastDone >= @Since;

/* ---------------------------------------------------------------------------
   2. Every row that carries logged time, by the status on that row.
      The corrected Assigned rule counts statuses 1,2,5,8,10 - a time row on
      any OTHER status would be a minute in the numerator whose task could be
      missing from the denominator.
   ------------------------------------------------------------------------- */
SELECT  '2 - time rows by their status (all)' AS Section,
        l.StatusID,
        COUNT(*) AS TimeRows,
        CAST(SUM(l.TimeCount) AS DECIMAL(18,2)) AS SumTime,
        CASE WHEN l.StatusID IN (1, 2, 5, 8, 10) THEN 'counted' ELSE '*** NOT counted ***' END AS InAssignedRule
FROM    dbo.TaskTLog l
WHERE   l.TimeCount > 0
GROUP BY l.StatusID
ORDER BY l.StatusID;

-- 2b. The same, rows written since @Since
SELECT  '2b - time rows by their status (recent)' AS Section,
        l.StatusID, COUNT(*) AS TimeRows,
        CASE WHEN l.StatusID IN (1, 2, 5, 8, 10) THEN 'counted' ELSE '*** NOT counted ***' END AS InAssignedRule
FROM    dbo.TaskTLog l
WHERE   l.TimeCount > 0 AND l.CreatedDate >= @Since
GROUP BY l.StatusID
ORDER BY l.StatusID;

/* ---------------------------------------------------------------------------
   3. Open tasks (current status New/Inprogress/Accepted/Delay) by how long
      they have had no TaskTLog movement. These sit in the employee's
      denominator every single week under the current rule.
   ------------------------------------------------------------------------- */
SELECT  '3 - open tasks by idle time' AS Section,
        SUM(CASE WHEN DATEDIFF(DAY, x.LastMove, @Now) <  14 THEN 1 ELSE 0 END) AS MovedWithin14d,
        SUM(CASE WHEN DATEDIFF(DAY, x.LastMove, @Now) BETWEEN 14 AND 29 THEN 1 ELSE 0 END) AS Idle14to29d,
        SUM(CASE WHEN DATEDIFF(DAY, x.LastMove, @Now) BETWEEN 30 AND 59 THEN 1 ELSE 0 END) AS Idle30to59d,
        SUM(CASE WHEN DATEDIFF(DAY, x.LastMove, @Now) BETWEEN 60 AND 179 THEN 1 ELSE 0 END) AS Idle60to179d,
        SUM(CASE WHEN DATEDIFF(DAY, x.LastMove, @Now) >= 180 THEN 1 ELSE 0 END) AS Idle180dPlus,
        COUNT(*) AS OpenTasksTotal
FROM   (SELECT t.TaskID,
               (SELECT MAX(l.CreatedDate) FROM dbo.TaskTLog l WHERE l.TaskID = t.TaskID) AS LastMove
        FROM dbo.Task t
        WHERE t.IsDeleted = 0 AND t.StatusID IN (1, 2, 8, 10)) x;

-- 3b. The same, split by the current assignee's provider
SELECT  '3b - stale open tasks by provider' AS Section,
        ISNULL(CAST(ua.IsTelesak AS INT), -1) AS IsTelesak,   -- 1 Telesak, 0 Esnad, -1 unknown
        COUNT(*) AS OpenTasks,
        SUM(CASE WHEN DATEDIFF(DAY, x.LastMove, @Now) >= 60 THEN 1 ELSE 0 END) AS Idle60dPlus,
        COUNT(DISTINCT CASE WHEN DATEDIFF(DAY, x.LastMove, @Now) >= 60 THEN x.EmpID END) AS EmployeesHoldingStale
FROM   (SELECT t.TaskID, t.EmpID,
               (SELECT MAX(l.CreatedDate) FROM dbo.TaskTLog l WHERE l.TaskID = t.TaskID) AS LastMove
        FROM dbo.Task t
        WHERE t.IsDeleted = 0 AND t.StatusID IN (1, 2, 8, 10)) x
LEFT JOIN dbo.UserAccount ua ON ua.EmpID = x.EmpID
GROUP BY ISNULL(CAST(ua.IsTelesak AS INT), -1);

-- 3c. The 20 stalest open tasks, to judge with your own eyes
SELECT  TOP 20
        '3c - stalest open tasks' AS Section,
        x.TaskID, x.EmpID, x.StatusID, x.CreatedDate, x.LastMove,
        x.StartDate AS PlannedStart, x.EndDate AS PlannedEnd,
        DATEDIFF(DAY, x.LastMove, @Now) AS IdleDays
FROM   (SELECT t.TaskID, t.EmpID, t.StatusID, t.CreatedDate, t.StartDate, t.EndDate,
               (SELECT MAX(l.CreatedDate) FROM dbo.TaskTLog l WHERE l.TaskID = t.TaskID) AS LastMove
        FROM dbo.Task t
        WHERE t.IsDeleted = 0 AND t.StatusID IN (1, 2, 8, 10)) x
ORDER BY x.LastMove ASC;

/* ---------------------------------------------------------------------------
   4. Open tasks whose planned period is already over. Under the proposed
      hybrid rule these stop counting while idle (and come back the moment the
      employee moves on them); under the current rule they count every week.
   ------------------------------------------------------------------------- */
SELECT  '4 - open tasks past their planned end' AS Section,
        COUNT(*) AS OpenTasks,
        SUM(CASE WHEN t.EndDate < @Now THEN 1 ELSE 0 END) AS PlannedEndPassed,
        SUM(CASE WHEN t.EndDate < DATEADD(DAY, -60, @Now) THEN 1 ELSE 0 END) AS PlannedEndPassed60dPlus
FROM    dbo.Task t
WHERE   t.IsDeleted = 0 AND t.StatusID IN (1, 2, 8, 10);
