/* =============================================================================
   Backfill TaskTLog.TimUnitID for rows orphaned by the 2026-04-26 upsert change

   WHY
   ---
   TaskManger.EmpUpdateTaskTime used to insert a TaskTLog row and always set
   TimUnitID = TimeUnit.Hour (4). Commit 32acdb6 (2026-04-26, "upsert today's task
   time log") replaced that with an upsert whose UPDATE branch assigns TimeCount
   but never touches TimUnitID. When the row it updates was created by a status
   transition - which leaves TimUnitID NULL - the unit stays NULL.

   dbo.GetCountMiniuts is called as GetCountMiniuts(TimeCount, ISNULL(TimUnitID,0)),
   and 0 matches no WHEN branch, so it returns NULL and SUM discards the row. Those
   rows therefore contribute ZERO to TotalWorkTime in every report.

   Measured on Esnad rows for 2026-08-01..18: 3,966 of 4,117 rows carrying a
   TimeCount had no unit - 96% of the logged work silently dropped.

   WHY 4 (HOURS) IS THE RIGHT VALUE - NOT A GUESS
   ----------------------------------------------
   SELECT DISTINCT TimUnitID FROM TaskTLog WHERE TimUnitID IS NOT NULL  -->  4

   4 is the only value this column has ever held, across all rows since 2015. No
   UI, API or mobile surface offers a unit selector for task time logs: the
   employee types a bare number that is validated against > 24, and the app has
   always hardcoded hours. So a NULL here means "hours, not recorded".

   ORDER OF OPERATIONS
   -------------------
   1. Deploy the code fix first (TaskManger.cs - set TimUnitID in the UPDATE
      branch). Backfilling before the code fix just leaves new NULLs accumulating
      behind you.
   2. Run this script.
   3. Only then consider shipping the TotalWorkTime column fix in QvApi
      (branch fix/totalworktime-column-mapping) - see CLAUDE.md, BUG 2.

   HOW TO RUN
   ----------
   Read STEP 1 and STEP 2 output first. STEP 3 is wrapped in a transaction that
   ROLLS BACK by default - it shows you what would change without changing it.
   Only after the numbers look right, set @Commit = 1 and run again.

   Run against a restored copy before production.
   ============================================================================= */

SET NOCOUNT ON;

DECLARE @Commit          bit      = 0;             -- 1 = actually apply
DECLARE @HourUnit        int      = 4;             -- TimeUnit.Hour
DECLARE @RegressionStart datetime = '2026-05-14';  -- first day the data shows the break

/* -----------------------------------------------------------------------------
   STEP 1 - confirm the assumption still holds on THIS database.
   If this returns anything other than a single row containing 4, STOP: some other
   unit is in use and the backfill value needs rethinking.
   -------------------------------------------------------------------------- */
PRINT '=== STEP 1: units ever recorded (expect exactly one row: 4) ===';
SELECT TimUnitID, COUNT(*) AS Rows
FROM dbo.TaskTLog
WHERE TimUnitID IS NOT NULL
GROUP BY TimUnitID
ORDER BY TimUnitID;

/* -----------------------------------------------------------------------------
   STEP 2 - scope of the damage, split at the regression date.

   Rows BEFORE @RegressionStart with a TimeCount but no unit are pre-existing and
   rare; they are NOT touched by default, because they predate this defect and may
   have a different cause. Widen the WHERE in STEP 3 only deliberately.
   -------------------------------------------------------------------------- */
PRINT '';
PRINT '=== STEP 2: rows carrying a TimeCount but no unit ===';
SELECT
    CASE WHEN CreatedDate >= @RegressionStart
         THEN 'On/after ' + CONVERT(varchar(10), @RegressionStart, 120) + '  (in scope)'
         ELSE 'Before '   + CONVERT(varchar(10), @RegressionStart, 120) + '  (NOT touched)'
    END                        AS Period,
    COUNT(*)                   AS RowsAffected,
    SUM(TimeCount)             AS TimeCountSum,
    SUM(TimeCount) * 60        AS MinutesRecovered,
    MIN(CreatedDate)           AS FirstRow,
    MAX(CreatedDate)           AS LastRow
FROM dbo.TaskTLog
WHERE TimeCount IS NOT NULL
  AND TimUnitID IS NULL
GROUP BY CASE WHEN CreatedDate >= @RegressionStart THEN 1 ELSE 0 END,
         CASE WHEN CreatedDate >= @RegressionStart
              THEN 'On/after ' + CONVERT(varchar(10), @RegressionStart, 120) + '  (in scope)'
              ELSE 'Before '   + CONVERT(varchar(10), @RegressionStart, 120) + '  (NOT touched)'
         END;

/* Affects both providers - this table is shared by Esnad and Telesak. */
PRINT '';
PRINT '=== STEP 2b: split by provider (both are affected) ===';
SELECT
    CASE WHEN ua.IsTelesak = 1 THEN 'Telesak' ELSE 'Esnad' END AS Provider,
    COUNT(*)                AS RowsAffected,
    SUM(l.TimeCount) * 60   AS MinutesRecovered
FROM dbo.TaskTLog      AS l
JOIN dbo.Employee      AS e  ON e.EmpID = l.EmpID
JOIN dbo.UserAccount   AS ua ON ua.EmpID = e.EmpID
WHERE l.TimeCount IS NOT NULL
  AND l.TimUnitID IS NULL
  AND l.CreatedDate >= @RegressionStart
GROUP BY ua.IsTelesak;

/* -----------------------------------------------------------------------------
   STEP 3 - the backfill.

   Only rows that actually carry a TimeCount are touched. A row with no TimeCount
   has no time to attribute, so giving it a unit would invent data.
   -------------------------------------------------------------------------- */
PRINT '';
PRINT '=== STEP 3: backfill ===';

BEGIN TRANSACTION;

    UPDATE dbo.TaskTLog
    SET    TimUnitID = @HourUnit
    WHERE  TimeCount   IS NOT NULL
      AND  TimUnitID   IS NULL
      AND  CreatedDate >= @RegressionStart;

    DECLARE @Changed int = @@ROWCOUNT;
    PRINT 'Rows updated: ' + CAST(@Changed AS varchar(20));

    /* Verification, inside the transaction so it reflects the new state. */
    SELECT
        SUM(CASE WHEN TimeCount IS NOT NULL AND TimUnitID IS NULL THEN 1 ELSE 0 END) AS StillMissingUnit,
        SUM(CASE WHEN TimeCount IS NOT NULL AND TimUnitID = @HourUnit THEN 1 ELSE 0 END) AS NowHours
    FROM dbo.TaskTLog
    WHERE CreatedDate >= @RegressionStart;

    IF @Commit = 1
    BEGIN
        COMMIT TRANSACTION;
        PRINT 'COMMITTED.';
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT 'ROLLED BACK (dry run). Set @Commit = 1 at the top to apply.';
    END
GO

/* =============================================================================
   AFTER RUNNING

   Every report that covers a period on/after 2026-05-14 will change - TotalWorkTime
   rises sharply, because work that was being discarded now counts. That is the
   point, but it is visible to the client, so have the explanation ready before
   filing the next report.

   This table is shared, so serv.telesak.com output moves at the same moment.

   Rows BEFORE 2026-05-14 are deliberately left alone. They are a much smaller
   population with a different (pre-existing) cause; decide on them separately.

   Still outstanding after this - see CLAUDE.md:
     BUG 4  GetCountMiniuts still has no ELSE branch and still truncates its
            DECIMAL argument to INT, so sub-hour entries round to zero.
     BUG 2  TotalWorkTime is read from the wrong column in QvApi.
     BUG 3  AssignedTasks is inflated by a missing correlation in the SP.
     BUG 1  ActivityLevel wraps at 100% via "% 100".
   ============================================================================= */
