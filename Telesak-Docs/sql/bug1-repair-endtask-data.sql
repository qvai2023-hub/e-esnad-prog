-- =====================================================================
-- Bug #1 data repair — tasks ended from the employee task-list page
-- (/employee/Tasks/EndTask) BEFORE the fix on branch fix/employee-list-endtask.
--
-- What the old code damaged (EmployeeTaskListVM.EndTask + StatusLog.LogStatuse):
--   1. Task.EndDate      overwritten with the finish time (planned end date lost)
--   2. TaskTLog.EmpID    saved as 0 on the "Done" (StatusID = 5) row
--   3. TaskTLog.TimeCount saved as 0 on that row (phantom 0-hour day)
--
-- Where the original values come from:
--   * EndDate  -> the TaskLog row the same EndTask call wrote; its Value holds
--                 {'columnName':'EndDate','oldVal':'<original>','newVal':'...'}
--   * EmpID    -> the latest earlier TaskTLog row for the task with a real EmpID,
--                 falling back to Task.EmpID
--   * TimeCount-> NULL (what every other status-change row stores)
--
-- HOW TO RUN
--   1. Take a full DB backup. Run on UAT first.
--   2. Run with @Apply = 0 (preview). Nothing is changed.
--   3. Look at result set #3 (date formats) and decide which @DateStyle parses
--      the old EndDate text correctly:  101 = m/d/yyyy (en-US server)
--                                       103 = d/m/yyyy (en-GB / ar-EG server)
--                                       131 = Hijri d/m/yyyy (ar-SA server)
--   4. Set @DateStyle, keep @Apply = 0, re-run and review result set #2.
--   5. Set @Apply = 1 and run once. Original values are copied to backup
--      tables first, so the change can be rolled back (see bottom of file).
--
-- Safe to re-run: repaired rows no longer match the filters.
-- Requires SQL Server 2012+ (TRY_CONVERT).
-- Scope: only the Done rows written by the old EndTask. Other EmpID = 0 rows
-- (from the old DeliverTask code) are listed in result set #4 but NOT changed.
-- =====================================================================

SET NOCOUNT ON;

DECLARE @Apply     BIT = 0;     -- 0 = preview only, 1 = apply the repair
DECLARE @DateStyle INT = NULL;  -- 101 / 103 / 131 — required when @Apply = 1

-- EndTask sets EndDate = DateTime.Now, then inserts the TaskLog row, then the
-- TaskTLog row. All three timestamps are within a few seconds of each other.
DECLARE @WindowSeconds INT = 120;
DECLARE @Marker NVARCHAR(50) = N'''columnName'':''EndDate'',''oldVal'':''';

IF @Apply = 1 AND @DateStyle NOT IN (101, 103, 131)
BEGIN
    RAISERROR(N'Set @DateStyle to 101, 103 or 131 before running with @Apply = 1.', 16, 1);
    RETURN;
END;

IF OBJECT_ID('tempdb..#Fix') IS NOT NULL DROP TABLE #Fix;

-- ---------------------------------------------------------------------
-- Collect the damaged Done rows
-- ---------------------------------------------------------------------
SELECT
    tl.TaskTLogID,
    tl.TaskID,
    tl.CreatedDate                              AS EndedAt,
    tl.TimeCount                                AS CurrentTimeCount,
    t.EmpID                                     AS TaskEmpID,
    COALESCE(prevEmp.EmpID, t.EmpID)            AS RestoredEmpID,
    t.StartDate,
    t.EndDate                                   AS CurrentEndDate,
    COUNT(*) OVER (PARTITION BY tl.TaskID)      AS BugRowsForTask,
    lg.TaskLogID,
    lg.OldValRaw,
    CAST(NULL AS DATETIME)                      AS RestoredEndDate,
    CAST(0 AS BIT)                              AS RestoreEndDate,
    CAST(N'' AS NVARCHAR(200))                  AS Note
INTO #Fix
FROM TaskTLog tl
JOIN Task t ON t.TaskID = tl.TaskID
OUTER APPLY (
    SELECT TOP 1 p.EmpID
    FROM TaskTLog p
    WHERE p.TaskID = tl.TaskID
      AND p.TaskTLogID < tl.TaskTLogID
      AND p.EmpID IS NOT NULL AND p.EmpID <> 0
    ORDER BY p.TaskTLogID DESC
) prevEmp
OUTER APPLY (
    SELECT TOP 1
        l.TaskLogID,
        CASE WHEN CHARINDEX(@Marker, l.Value) = 0 THEN NULL
             ELSE SUBSTRING(l.Value,
                            CHARINDEX(@Marker, l.Value) + LEN(@Marker),
                            CHARINDEX(N'''', l.Value, CHARINDEX(@Marker, l.Value) + LEN(@Marker))
                              - (CHARINDEX(@Marker, l.Value) + LEN(@Marker)))
        END AS OldValRaw
    FROM TaskLog l
    WHERE l.TaskID = tl.TaskID
      AND l.CurrentStatus = 5
      AND l.Value LIKE N'%''columnName'':''EndDate''%'
      AND l.LogDate BETWEEN DATEADD(SECOND, -@WindowSeconds, tl.CreatedDate) AND tl.CreatedDate
    ORDER BY l.LogDate DESC
) lg
WHERE tl.StatusID = 5
  AND tl.EmpID = 0
  AND t.IsDeleted = 0;

-- ---------------------------------------------------------------------
-- Decide, per row, whether EndDate can be restored safely
-- ---------------------------------------------------------------------
UPDATE f SET
    RestoredEndDate = CASE @DateStyle
        WHEN 101 THEN TRY_CONVERT(DATETIME, f.OldValRaw, 101)
        WHEN 103 THEN TRY_CONVERT(DATETIME, f.OldValRaw, 103)
        WHEN 131 THEN TRY_CONVERT(DATETIME,
                          REPLACE(REPLACE(f.OldValRaw, N'ص', N'AM'), N'م', N'PM'), 131)
    END
FROM #Fix f
WHERE f.OldValRaw IS NOT NULL AND f.OldValRaw <> N'';

UPDATE #Fix SET Note = CASE
    WHEN RestoredEmpID IS NULL
        THEN N'MANUAL: no employee found for this task'
    WHEN BugRowsForTask > 1
        THEN N'MANUAL: ended from the list more than once — EndDate not restored'
    WHEN CurrentEndDate IS NULL
      OR ABS(DATEDIFF(SECOND, CurrentEndDate, EndedAt)) > @WindowSeconds
        THEN N'SKIP ENDDATE: EndDate was changed after the task was ended'
    WHEN TaskLogID IS NULL OR OldValRaw IS NULL
        THEN N'MANUAL: matching TaskLog row not found — EndDate not restored'
    WHEN OldValRaw = N''
        THEN N'OK: original EndDate was empty -> restore NULL'
    WHEN @DateStyle IS NULL
        THEN N'PENDING: choose @DateStyle'
    WHEN RestoredEndDate IS NULL
        THEN N'MANUAL: old EndDate text did not parse with @DateStyle'
    WHEN StartDate IS NOT NULL AND RestoredEndDate < CAST(StartDate AS DATE)
        THEN N'CHECK: restored EndDate is before StartDate — verify @DateStyle'
    ELSE N'OK'
END;

UPDATE #Fix SET RestoreEndDate = 1 WHERE Note LIKE N'OK%';

-- ---------------------------------------------------------------------
-- Result set #1 — summary
-- ---------------------------------------------------------------------
SELECT
    COUNT(*)                                            AS DamagedDoneRows,
    COUNT(DISTINCT TaskID)                              AS TasksAffected,
    SUM(CASE WHEN RestoredEmpID IS NOT NULL THEN 1 ELSE 0 END) AS EmpIdWillBeFixed,
    SUM(CASE WHEN RestoreEndDate = 1 THEN 1 ELSE 0 END) AS EndDateWillBeRestored,
    SUM(CASE WHEN Note LIKE N'MANUAL%' OR Note LIKE N'CHECK%' THEN 1 ELSE 0 END) AS NeedManualReview,
    SUM(CASE WHEN Note LIKE N'SKIP%' THEN 1 ELSE 0 END) AS EndDateLeftAsIs
FROM #Fix;

-- Result set #2 — row-by-row plan
SELECT TaskTLogID, TaskID, EndedAt, TaskEmpID, RestoredEmpID, StartDate,
       CurrentEndDate, OldValRaw, RestoredEndDate, RestoreEndDate, Note
FROM #Fix
ORDER BY Note, TaskID;

-- Result set #3 — how the old EndDate text looks, parsed each way.
-- Pick the @DateStyle whose column gives sensible dates for every row.
SELECT TOP 20
    OldValRaw,
    TRY_CONVERT(DATETIME, OldValRaw, 101) AS As101_US,
    TRY_CONVERT(DATETIME, OldValRaw, 103) AS As103_DMY,
    TRY_CONVERT(DATETIME, REPLACE(REPLACE(OldValRaw, N'ص', N'AM'), N'م', N'PM'), 131) AS As131_Hijri
FROM #Fix
WHERE OldValRaw IS NOT NULL AND OldValRaw <> N''
GROUP BY OldValRaw
ORDER BY OldValRaw;

-- Result set #4 — FOR INFORMATION ONLY (not repaired by this script):
-- other EmpID = 0 rows, written by the old DeliverTask code path.
SELECT StatusID, COUNT(*) AS RowsWithEmpIdZero
FROM TaskTLog
WHERE EmpID = 0 AND StatusID <> 5
GROUP BY StatusID;

IF @Apply = 0
BEGIN
    PRINT N'Preview only — nothing was changed. Set @Apply = 1 to repair.';
    RETURN;
END;

-- ---------------------------------------------------------------------
-- Apply
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.Bug1Repair_TaskTLog_Backup') IS NULL
    CREATE TABLE dbo.Bug1Repair_TaskTLog_Backup (
        TaskTLogID INT NOT NULL,
        EmpID      INT NULL,
        TimeCount  DECIMAL(5, 2) NULL,
        RepairedAt DATETIME NOT NULL
    );

IF OBJECT_ID('dbo.Bug1Repair_Task_Backup') IS NULL
    CREATE TABLE dbo.Bug1Repair_Task_Backup (
        TaskID     INT NOT NULL,
        EndDate    DATETIME NULL,
        RepairedAt DATETIME NOT NULL
    );

DECLARE @Now DATETIME = GETDATE();

BEGIN TRY
    BEGIN TRANSACTION;

    -- Back up the original values first
    INSERT INTO dbo.Bug1Repair_TaskTLog_Backup (TaskTLogID, EmpID, TimeCount, RepairedAt)
    SELECT tl.TaskTLogID, tl.EmpID, tl.TimeCount, @Now
    FROM TaskTLog tl
    JOIN #Fix f ON f.TaskTLogID = tl.TaskTLogID
    WHERE f.RestoredEmpID IS NOT NULL;

    INSERT INTO dbo.Bug1Repair_Task_Backup (TaskID, EndDate, RepairedAt)
    SELECT t.TaskID, t.EndDate, @Now
    FROM Task t
    JOIN #Fix f ON f.TaskID = t.TaskID
    WHERE f.RestoreEndDate = 1;

    -- 1. Done row: real employee, no phantom time
    UPDATE tl SET
        tl.EmpID     = f.RestoredEmpID,
        tl.TimeCount = NULL
    FROM TaskTLog tl
    JOIN #Fix f ON f.TaskTLogID = tl.TaskTLogID
    WHERE f.RestoredEmpID IS NOT NULL;

    -- 2. Planned end date back to its original value
    UPDATE t SET t.EndDate = f.RestoredEndDate
    FROM Task t
    JOIN #Fix f ON f.TaskID = t.TaskID
    WHERE f.RestoreEndDate = 1;

    COMMIT TRANSACTION;

    SELECT @Now AS RepairedAt,
           (SELECT COUNT(*) FROM dbo.Bug1Repair_TaskTLog_Backup WHERE RepairedAt = @Now) AS TaskTLogRowsRepaired,
           (SELECT COUNT(*) FROM dbo.Bug1Repair_Task_Backup    WHERE RepairedAt = @Now) AS TaskEndDatesRestored;
    PRINT N'Repair applied. Keep the RepairedAt value — the rollback needs it.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Repair failed and was rolled back: %s', 16, 1, @Err);
END CATCH;

-- =====================================================================
-- ROLLBACK (run separately, only if needed). Replace the date with the
-- RepairedAt value printed by the apply run.
-- =====================================================================
/*
DECLARE @RepairedAt DATETIME = '2026-01-01T00:00:00';

BEGIN TRANSACTION;

UPDATE tl SET tl.EmpID = b.EmpID, tl.TimeCount = b.TimeCount
FROM TaskTLog tl
JOIN dbo.Bug1Repair_TaskTLog_Backup b ON b.TaskTLogID = tl.TaskTLogID
WHERE b.RepairedAt = @RepairedAt;

UPDATE t SET t.EndDate = b.EndDate
FROM Task t
JOIN dbo.Bug1Repair_Task_Backup b ON b.TaskID = t.TaskID
WHERE b.RepairedAt = @RepairedAt;

COMMIT TRANSACTION;
*/
