/* =============================================================================
   Can the ACTUAL start / end of a task be read from dbo.TaskTLog?

   READ ONLY. Run before building the display, on a copy of production data.

   The rule being checked (dbo.Task.StartDate / EndDate are NOT touched; they
   stay the planned dates from the form):

       ActualStart = first TaskTLog row of the task with StatusID = 2 (Inprogress)
       ActualEnd   = last  TaskTLog row of the task with StatusID = 5 (Done),
                     shown only while the task is Done / Approved / NotApproved
                     (5 / 6 / 7). A task reopened after Done has no end yet.

   Status ids: 1 New, 2 Inprogress, 3 Pending, 5 Done, 6 Approved,
               7 NotApproved, 8 Accepted, 9 Rejected

   @Since limits the checks to recent tasks (older data may predate the
   Inprogress logging). Each check returns Result = PASS / FAIL / INFO.

   TARGET ENGINE: SQL Server 2008 R2.
   ============================================================================= */

DECLARE @Since DATETIME = '2026-08-01';   -- <-- tasks created from this date

IF OBJECT_ID('tempdb..#A') IS NOT NULL DROP TABLE #A;

SELECT  t.TaskID, t.StatusID, t.CreatedDate, t.StartDate, t.EndDate, t.DeliverDate,
        (SELECT MIN(l.CreatedDate) FROM dbo.TaskTLog l WHERE l.TaskID = t.TaskID AND l.StatusID = 2) AS ActualStart,
        (SELECT MAX(l.CreatedDate) FROM dbo.TaskTLog l WHERE l.TaskID = t.TaskID AND l.StatusID = 5) AS LastDone,
        (SELECT MAX(l.CreatedDate) FROM dbo.TaskTLog l WHERE l.TaskID = t.TaskID AND l.StatusID = 2) AS LastInprogress
INTO    #A
FROM    dbo.Task t
WHERE   t.IsDeleted = 0 AND t.CreatedDate >= @Since;

/* 1. Tasks that were worked on have an Inprogress row (ActualStart not empty) */
SELECT  'Check 1 - worked-on tasks have an actual start' AS [Check],
        COUNT(*) AS TasksWorkedOn,
        SUM(CASE WHEN ActualStart IS NULL THEN 1 ELSE 0 END) AS MissingStart,
        CASE WHEN SUM(CASE WHEN ActualStart IS NULL THEN 1 ELSE 0 END) = 0 THEN 'PASS' ELSE 'FAIL (list below)' END AS Result
FROM    #A WHERE StatusID IN (2, 3, 5, 6, 7);

SELECT  TOP 30 'Check 1 list' AS [Check], a.TaskID, a.StatusID, a.CreatedDate, a.DeliverDate
FROM    #A a WHERE a.StatusID IN (2, 3, 5, 6, 7) AND a.ActualStart IS NULL
ORDER BY a.CreatedDate DESC;

/* 2. Finished tasks have a Done row (ActualEnd not empty) */
SELECT  'Check 2 - finished tasks have an actual end' AS [Check],
        COUNT(*) AS FinishedTasks,
        SUM(CASE WHEN LastDone IS NULL THEN 1 ELSE 0 END) AS MissingEnd,
        CASE WHEN SUM(CASE WHEN LastDone IS NULL THEN 1 ELSE 0 END) = 0 THEN 'PASS' ELSE 'FAIL (list below)' END AS Result
FROM    #A WHERE StatusID IN (5, 6, 7);

SELECT  TOP 30 'Check 2 list' AS [Check], a.TaskID, a.StatusID, a.CreatedDate, a.DeliverDate
FROM    #A a WHERE a.StatusID IN (5, 6, 7) AND a.LastDone IS NULL
ORDER BY a.CreatedDate DESC;

/* 3. The last Done row is on the same DAY as DeliverDate (DeliverDate keeps the date only, no time) */
SELECT  'Check 3 - last Done = DeliverDate (same day)' AS [Check],
        COUNT(*) AS Compared,
        SUM(CASE WHEN DATEDIFF(DAY, LastDone, DeliverDate) <> 0 THEN 1 ELSE 0 END) AS Different,
        CASE WHEN SUM(CASE WHEN DATEDIFF(DAY, LastDone, DeliverDate) <> 0 THEN 1 ELSE 0 END) = 0
             THEN 'PASS' ELSE 'FAIL (list below)' END AS Result
FROM    #A WHERE StatusID IN (5, 6, 7) AND LastDone IS NOT NULL AND DeliverDate IS NOT NULL;

SELECT  TOP 30 'Check 3 list' AS [Check], a.TaskID, a.StatusID, a.LastDone, a.DeliverDate,
        DATEDIFF(DAY, a.LastDone, a.DeliverDate) AS DaysApart
FROM    #A a
WHERE   a.StatusID IN (5, 6, 7) AND a.LastDone IS NOT NULL AND a.DeliverDate IS NOT NULL
  AND   DATEDIFF(DAY, a.LastDone, a.DeliverDate) <> 0
ORDER BY a.LastDone DESC;

/* 4. The end is never before the start */
SELECT  'Check 4 - actual end not before actual start' AS [Check],
        SUM(CASE WHEN LastDone < ActualStart THEN 1 ELSE 0 END) AS EndBeforeStart,
        CASE WHEN SUM(CASE WHEN LastDone < ActualStart THEN 1 ELSE 0 END) = 0 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM    #A WHERE StatusID IN (5, 6, 7) AND LastDone IS NOT NULL AND ActualStart IS NOT NULL;

/* 5. Reopened tasks: Done, then Inprogress again. Their end comes from the LAST Done */
SELECT  'Check 5 - reopened after Done (INFO)' AS [Check],
        SUM(CASE WHEN LastInprogress > LastDone THEN 1 ELSE 0 END) AS ReopenedTasks,
        SUM(CASE WHEN LastInprogress > LastDone AND StatusID NOT IN (5, 6, 7) THEN 1 ELSE 0 END) AS StillOpen_NoEndShown,
        'INFO' AS Result
FROM    #A WHERE LastDone IS NOT NULL;

/* 6. Tasks moved to another employee while in progress: the new task starts its own
      history, so its actual start is the new employee's first Inprogress (INFO) */
SELECT  'Check 6 - more than one employee in the history (INFO)' AS [Check],
        COUNT(*) AS TasksWithSeveralEmployees, 'INFO' AS Result
FROM   (SELECT l.TaskID FROM dbo.TaskTLog l JOIN #A a ON a.TaskID = l.TaskID
        WHERE l.EmpID IS NOT NULL GROUP BY l.TaskID HAVING COUNT(DISTINCT l.EmpID) > 1) x;

/* 7. Side by side: planned dates (unchanged) next to the actual ones */
SELECT  TOP 30 'Check 7 - sample' AS [Check], a.TaskID, a.StatusID,
        a.StartDate AS PlannedStart, a.ActualStart,
        a.EndDate   AS PlannedEnd,
        CASE WHEN a.StatusID IN (5, 6, 7) THEN a.LastDone END AS ActualEnd,
        a.DeliverDate
FROM    #A a
ORDER BY a.CreatedDate DESC;

DROP TABLE #A;
