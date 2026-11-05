/* =============================================================================
   The corrected telework-reporting path, as NEW objects.

   DESIGN DECISION (2026-09-22) -- why new objects and not ALTERs
   -------------------------------------------------------------
   dbo.GetCountMiniuts, dbo.GetActivityLevel and dbo.GetEvaluationService are
   read by reports the client relies on and did not ask us to change:

       dbo.sp_EmployeePerformanceReport
       dbo.sp_TotalEmployeeTasks  (through dbo.GetAssignedTime / dbo.GetTotalTime)
       serv.telesak.com           (the other SOAP service, same SP)

   Fixing them in place moves all of those at the same instant and offers no way
   to roll one consumer back. So the three stay exactly as the client has always
   had them -- restore-shared-objects.sql puts them back -- and everything below
   is new and additive. Nothing existing calls these objects; only
   TeleworkReporter does.

   WHAT IS CREATED
   ---------------
     dbo.GetWorkMinutes        replaces GetCountMiniuts for this path   (defect 4)
     dbo.GetActivityPercent    replaces GetActivityLevel for this path  (defect 1)
     dbo.GetEvaluationServiceV2                                    (defects 3, 7)

   GetEvaluationServiceV2 returns every column the original returns, computed the
   ORIGINAL way, so the "current" baseline TeleworkReporter shows is still the
   real thing -- and adds the corrected columns beside them. One read, both
   answers, nothing to keep in sync.

   TARGET ENGINE: SQL Server 2008 R2 (compatibility level 100). No DATEFROMPARTS,
   TRY_CAST, CONCAT, IIF or THROW.
   ============================================================================= */

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* =============================================================================
   1. dbo.GetWorkMinutes -- the corrected minute conversion.       DEFECT 4

   Two defects in dbo.GetCountMiniuts, which stays untouched:

     a. @TimeCount is declared INT while every caller passes DECIMAL(5,2), so
        SQL Server truncates toward zero: 0.75 hours becomes 0 minutes, not 45.
        This database holds 656 fractional rows, 121 of them since 2026-08-01,
        so it is not hypothetical.

     b. The CASE has no ELSE, so an unrecognised unit returns NULL. Callers pass
        ISNULL(TimUnitID, 0) and 0 matches no branch, so the row returns NULL and
        SUM discards it silently.

   Unit 0 and NULL are treated as HOURS here, not as "unknown":

        SELECT DISTINCT TimUnitID FROM TaskTLog WHERE TimUnitID IS NOT NULL --> 4

   4 is the only value the column has ever held since 2015, and no UI, API or
   mobile surface offers a unit selector -- the employee types a bare number
   validated against > 24 and the app hardcodes hours. A NULL means "hours, not
   recorded".

   A genuinely unrecognised unit (6, 99, anything new) returns 0, not hours.
   Guessing on a unit nobody has defined would be worse than a visible zero. ADD
   AN EXPLICIT BRANCH if a new unit is ever introduced.

   RETURNS stays INT so the column type the callers see does not move. ROUND is
   used rather than the implicit truncation, so 19.8 minutes becomes 20, not 19.

   The unit ladder matches the C# copy in Helpers/EvaluationHelper.cs
   (1=month x9600, 2=week x2400, 3=day x480, 4=hour x60, 5=minute x1). Do not
   change one without the other.
   ============================================================================= */
IF OBJECT_ID('dbo.GetWorkMinutes', 'FN') IS NOT NULL
    DROP FUNCTION dbo.GetWorkMinutes;
GO

CREATE FUNCTION [dbo].[GetWorkMinutes]
(
    @TimeCount DECIMAL(9,2) = 1,   -- was INT in GetCountMiniuts: that truncated
    @TimeUnit  INT
)
RETURNS INT
AS
BEGIN
    DECLARE @minutes DECIMAL(18,4);

    SET @minutes =
        CASE
            WHEN @TimeCount IS NULL THEN NULL                          -- no time logged
            WHEN @TimeUnit  = 1     THEN @TimeCount * 60 * 8 * 5 * 4   -- month
            WHEN @TimeUnit  = 2     THEN @TimeCount * 60 * 8 * 5       -- week
            WHEN @TimeUnit  = 3     THEN @TimeCount * 60 * 8           -- day
            WHEN @TimeUnit  = 4     THEN @TimeCount * 60               -- hour
            WHEN @TimeUnit  = 5     THEN @TimeCount                    -- minute

            -- "unit not recorded" -> hours. This is the branch that recovers
            -- work the deployed function throws away.
            WHEN @TimeUnit  = 0 OR @TimeUnit IS NULL THEN @TimeCount * 60

            -- A unit nobody has defined. 0, not NULL: a NULL vanishes inside SUM.
            ELSE 0
        END;

    RETURN CAST(ROUND(@minutes, 0) AS INT);
END
GO

/* =============================================================================
   2. dbo.GetActivityPercent -- the corrected activity level.      DEFECT 1

   dbo.GetActivityLevel, untouched, computes

        ROUND(((@TotalWorkTime * 100) / @AssignedTaskesTime) % 100, 2)

   The "% 100" means a teleworker at exactly 100% reports 0 and one at 150%
   reports 50 -- best and worst are indistinguishable. The division is integer,
   so everything below 1% collapses to 0 before the modulo. ROUND(...,2) on an
   INT is dead code. And "@TotalWorkTime * 100" overflows INT above 21,474,836.

   Here: decimal division, cap instead of wrap, explicit 0 when there is no
   target. Meeting the target reports 100.
   ============================================================================= */
IF OBJECT_ID('dbo.GetActivityPercent', 'FN') IS NOT NULL
    DROP FUNCTION dbo.GetActivityPercent;
GO

CREATE FUNCTION [dbo].[GetActivityPercent]
(
    @TotalWorkTime      INT,
    @AssignedTaskesTime INT = 1
)
RETURNS INT
AS
BEGIN
    DECLARE @ret INT;

    IF (@AssignedTaskesTime > 0)
    BEGIN
        DECLARE @pct DECIMAL(18,4);

        -- CAST first: avoids the INT overflow the old "* 100" hit, and keeps the
        -- division decimal so sub-1% values survive.
        SET @pct = (CAST(ISNULL(@TotalWorkTime, 0) AS DECIMAL(18,4)) * 100.0)
                 /  CAST(@AssignedTaskesTime AS DECIMAL(18,4));

        IF (@pct > 100.0) SET @pct = 100.0;   -- cap, do not wrap
        IF (@pct < 0.0)   SET @pct = 0.0;

        SET @ret = CAST(ROUND(@pct, 0) AS INT);
    END
    ELSE
    BEGIN
        SET @ret = 0;   -- no target means no meaningful percentage
    END

    RETURN @ret;
END
GO

/* =============================================================================
   3. dbo.GetEvaluationServiceV2

   A copy of dbo.GetEvaluationService that returns BOTH answers side by side.

   The original columns are computed exactly as the original computes them --
   original functions, original uncorrelated ttls1 -- because TeleworkReporter
   shows "current" as the baseline the correction is judged against, and a
   baseline that has been quietly improved is not a baseline.

   Added beside them:

     CorrectedAssignedTasks       independent per-task period/status   DEFECT 3
     CorrectedAssignedTaskesTime  same, through GetWorkMinutes
     CorrectedTotalWorkTime       GetWorkMinutes                       DEFECT 4
     CorrectedActivityLevelSql    GetActivityPercent                   DEFECT 1
     CorrectedLoginCount          distinct login calendar days         ISSUE 1
     CorrectedLogOutCount         distinct logout calendar days        ISSUE 1
     CorrectedCompletedTasks      distinct tasks completed in period   ISSUE 3
     IsActive / IsDeleted         so the operator can see who is filed DEFECT 7

   The "Sql" suffix on the activity column is deliberate: TeleworkReporter
   computes the corrected percentage itself, to two decimal places, and keeps
   this one only so the two can be compared.

   ---------------------------------------------------------------------------
   DEFECT 3 -- how the correction is computed in one pass
   ---------------------------------------------------------------------------
   The original's ttls1 subquery asks "does this EMPLOYEE have any task at status
   1 or 2 before the period end", where ttls2-ttls8 all correlate on
   ttls.TaskID = Task.TaskID. So one active task makes every other task of that
   employee qualify.

   The corrected set is evaluated independently. Merely adding TaskID to the
   status-1/2 predicate incorrectly drops legitimate Accepted or Done-in-period
   tasks; leaving it uncorrelated lets one task affect another. V2 therefore
   evaluates period overlap and status history for each employee/TaskID pair.

   ---------------------------------------------------------------------------
   DEFECT 7 -- @ActiveOnly and @ExcludeDeleted
   ---------------------------------------------------------------------------
   The original filters on UserAccount.IsTelesak and nothing else, so inactive
   and deleted employees are filed to the ministry as teleworkers. On this
   database Esnad reports 228 people of whom 104 are active, 124 inactive and 3
   deleted.

   Whether the ministry wants every registered teleworker listed with zero
   activity, or only the active ones, is THEIR call and has not been asked. So
   this is a parameter, not a hardcoded WHERE: both default to 1 because that is
   the defensible position for a government filing, and either can be turned off
   for one run without redeploying anything. The counts of who was excluded come
   back as OUTPUT parameters and as a PRINT, so the choice is never invisible.

   The procedure returns exactly ONE result set, so INSERT ... EXEC into a temp
   table works. That is why the exclusion counts are OUTPUT parameters rather
   than a second SELECT.

   ---------------------------------------------------------------------------
   Leavers -- @IncludePeriodActivity (added 2026-10-06)
   ---------------------------------------------------------------------------
   Employee.IsActive / IsDeleted are bits read when the report RUNS, not over the
   period. Someone who works Sunday-Wednesday and is deactivated on Thursday is
   inactive by Monday's run, and @ActiveOnly would drop their last days of real
   work. Measured on the local restore, Telesak, 2026-08-01 -> 2026-09-14: 36
   inactive employees had activity, 21 logged time, 331 completed tasks; several
   worked until the very hour they were deactivated (EmpID 1611/1612, 583, 1580).
   Esnad: none.

   So with @IncludePeriodActivity = 1 an inactive or deleted employee is kept if
   they did anything inside [@PeriodStart, @PeriodEnd): any TaskTLog row, or a
   login. Activity, not the deactivation log, decides -- LogGeneral records only
   ~85% of deactivations (19 of 124 inactive Esnad people have no entry), and an
   activity rule can never drop real work.

   Returned beside each row so the operator sees why it is there:
     HadPeriodActivity   1 if the rule above matched
     DeactivatedAt       last IsActive -> false change in LogGeneral, if logged
     MaxDailyMinutes     largest single-day total of logged minutes in the period;
                         over 1440 is not possible as worked time and is flagged
                         by the app (EmpID 896: 6 x 9 h within 8 minutes). Flagged,
                         never removed -- a person decides.

   ---------------------------------------------------------------------------
   Company must be active (added 2026-10-12) -- always on, no parameter
   ---------------------------------------------------------------------------
   The employee's Company must exist, be active and not be deleted. The original
   never joins Company, so an active employee of a switched-off company is filed.
   Measured on the local restore, active undeleted employees: Esnad 19 in active
   companies, 72 in inactive, 4 in deleted; Telesak 1,011 / 27 / 19. Logged time
   2026-08..09 outside active companies: one Esnad person, 180 min.
   This applies over every other rule, leavers included. The count removed comes
   back as @ExcludedInactiveCompany.
   ============================================================================= */
IF OBJECT_ID('dbo.GetEvaluationServiceV2', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetEvaluationServiceV2;
GO

CREATE PROCEDURE [dbo].[GetEvaluationServiceV2]
    @PeriodStart     DATETIME,
    @PeriodEnd       DATETIME,
    @IsTelesak       BIT,
    @ActiveOnly      BIT = 1,   -- 0 reproduces the original population
    @ExcludeDeleted  BIT = 1,   -- 0 reproduces the original population
    @IncludePeriodActivity BIT = 0,  -- business rule: always exclude inactive/deleted employees

    -- Who the filters removed. OUTPUT parameters rather than a second result set:
    -- a second result set makes INSERT ... EXEC fail with "Column name or number of
    -- supplied values does not match table definition", because INSERT ... EXEC
    -- captures EVERY result set and they must all match the target table. Operators
    -- do capture this procedure into a temp table, so the exclusion counts have to
    -- travel some other way. They are also PRINTed for anyone running it in SSMS.
    @PopulationBeforeFilters INT = NULL OUTPUT,
    @ExcludedInactive        INT = NULL OUTPUT,
    @ExcludedDeleted         INT = NULL OUTPUT,

    -- Active, undeleted people whose UserAccount.IsTelesak is NULL. "IsTelesak = @IsTelesak"
    -- never matches NULL, so they are filed by NEITHER provider - they are in no report at
    -- all, and no filter in this procedure is responsible. Measured 2026-09-22: 7 employees
    -- across 13 accounts, none of whom logged time in 2026-08. Harmless today, but it would
    -- swallow a real teleworker whose account was created without the flag, so it is counted
    -- here rather than left to be discovered later.
    @UnassignedActive        INT = NULL OUTPUT,

    -- Inactive or deleted, but kept because they were active inside the period.
    @IncludedLeavers         INT = NULL OUTPUT,

    -- Would have been reported, but their company is missing, inactive or deleted.
    @ExcludedInactiveCompany INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET FMTONLY OFF;

    /* -------------------------------------------------------------------------
       Who did anything inside [@PeriodStart, @PeriodEnd). Half-open on purpose:
       this decides who is IN the report, so a row exactly at @PeriodEnd must not
       pull someone into the wrong week.
       ---------------------------------------------------------------------- */
    SELECT DISTINCT x.EmpID
    INTO #PeriodActivity
    FROM (
        SELECT tl.EmpID
        FROM dbo.TaskTLog AS tl
        WHERE tl.CreatedDate >= @PeriodStart AND tl.CreatedDate < @PeriodEnd
        UNION
        SELECT lg.EmployeeID
        FROM dbo.LogGeneral AS lg
        WHERE lg.ActionID = 5
          AND lg.ActionTime >= @PeriodStart AND lg.ActionTime < @PeriodEnd
    ) AS x
    WHERE x.EmpID IS NOT NULL;

    /* -------------------------------------------------------------------------
       When each employee was last deactivated, where LogGeneral recorded it.
       Generallog.LogUpdate writes
         {'ID':'<EmpID>','itemText':'<name>','updates':[{'columnName':'IsActive',
          'oldVal':'<yes>','newVal':'<no>'}]}
       with the values in Arabic: NCHAR(1604)+NCHAR(1575) is "no". Built from
       NCHAR so the script does not depend on its own file encoding. Evidence for
       the operator only -- it decides nothing.
       ---------------------------------------------------------------------- */
    -- The CASEs guard the extraction: SQL Server does not promise to apply the WHERE
    -- before the SELECT list, and SUBSTRING with a negative length raises an error.
    SELECT CAST(d.IdText AS INT) AS EmpID, MAX(d.ActionTime) AS DeactivatedAt
    INTO #Deactivations
    FROM (
        SELECT lg.ActionTime,
               CASE WHEN CHARINDEX('''', lg.Value, 8) BETWEEN 9 AND 17
                     AND SUBSTRING(lg.Value, 8, CHARINDEX('''', lg.Value, 8) - 8) NOT LIKE '%[^0-9]%'
                    THEN SUBSTRING(lg.Value, 8, CHARINDEX('''', lg.Value, 8) - 8) END AS IdText
        FROM dbo.LogGeneral AS lg
        WHERE lg.ActionID = 3
          AND lg.Value LIKE N'{''ID'':''[0-9]%'
          AND lg.Value LIKE N'%''columnName'':''IsActive'',''oldVal'':''%'',''newVal'':'''
                            + NCHAR(1604) + NCHAR(1575) + N'''%'
    ) AS d
    WHERE d.IdText IS NOT NULL
    GROUP BY CAST(d.IdText AS INT);

    /* -------------------------------------------------------------------------
       Largest single-day total of logged minutes, per employee. Over 1440 cannot
       be worked time; the app flags it.
       ---------------------------------------------------------------------- */
    SELECT d.EmpID, MAX(d.DayMinutes) AS MaxDailyMinutes
    INTO #MaxDaily
    FROM (
        SELECT tl.EmpID, SUM(dbo.GetWorkMinutes(tl.TimeCount, tl.TimUnitID)) AS DayMinutes
        FROM dbo.TaskTLog AS tl
        WHERE tl.CreatedDate >= @PeriodStart AND tl.CreatedDate < @PeriodEnd
          AND tl.TimeCount IS NOT NULL
        GROUP BY tl.EmpID, CAST(tl.CreatedDate AS DATE)
    ) AS d
    GROUP BY d.EmpID;

    /* -------------------------------------------------------------------------
       Who the filters will remove. Computed first so the numbers are available
       whatever happens below, and so the report itself stays the ONLY result set.
       Someone kept by @IncludePeriodActivity is counted as an included leaver,
       not as excluded.
       ---------------------------------------------------------------------- */
    SELECT
        @PopulationBeforeFilters = COUNT(*),
        @ExcludedInactive = SUM(CASE WHEN @ActiveOnly = 1 AND ISNULL(e.IsActive, 0) = 0
                                      AND NOT (@IncludePeriodActivity = 1 AND pa.EmpID IS NOT NULL)
                                     THEN 1 ELSE 0 END),
        @ExcludedDeleted  = SUM(CASE WHEN @ExcludeDeleted = 1 AND ISNULL(e.IsDeleted, 0) = 1
                                      AND NOT (@IncludePeriodActivity = 1 AND pa.EmpID IS NOT NULL)
                                     THEN 1 ELSE 0 END),
        @IncludedLeavers  = SUM(CASE WHEN @IncludePeriodActivity = 1 AND pa.EmpID IS NOT NULL
                                      AND ((@ActiveOnly = 1 AND ISNULL(e.IsActive, 0) = 0)
                                        OR (@ExcludeDeleted = 1 AND ISNULL(e.IsDeleted, 0) = 1))
                                     THEN 1 ELSE 0 END),
        -- People, not account rows: an employee with two UserAccounts appears twice in this join.
        @ExcludedInactiveCompany = COUNT(DISTINCT CASE WHEN NOT (ISNULL(c.IsActive, 0) = 1 AND ISNULL(c.IsDeleted, 0) = 0)
                                             AND (    (    (@ActiveOnly     = 0 OR ISNULL(e.IsActive,  0) = 1)
                                                       AND (@ExcludeDeleted = 0 OR ISNULL(e.IsDeleted, 0) = 0))
                                                  OR (@IncludePeriodActivity = 1 AND pa.EmpID IS NOT NULL))
                                            THEN e.EmpID END)
    FROM dbo.Employee AS e
    JOIN dbo.UserAccount AS ua ON ua.EmpID = e.EmpID
    LEFT JOIN dbo.Company AS c ON c.CompanyID = e.CompanyID
    LEFT JOIN #PeriodActivity AS pa ON pa.EmpID = e.EmpID
    WHERE ua.IsTelesak = @IsTelesak;

    SELECT @UnassignedActive = COUNT(DISTINCT e.EmpID)
    FROM dbo.Employee AS e
    JOIN dbo.UserAccount AS ua ON ua.EmpID = e.EmpID
    WHERE ua.IsTelesak IS NULL
      AND ISNULL(e.IsActive, 0) = 1
      AND ISNULL(e.IsDeleted, 0) = 0;

    PRINT 'GetEvaluationServiceV2: population ' + CAST(ISNULL(@PopulationBeforeFilters, 0) AS varchar(12))
        + ', excluded inactive ' + CAST(ISNULL(@ExcludedInactive, 0) AS varchar(12))
        + ', excluded deleted '  + CAST(ISNULL(@ExcludedDeleted, 0) AS varchar(12))
        + ', inactive/deleted but active in the period, kept ' + CAST(ISNULL(@IncludedLeavers, 0) AS varchar(12))
        + ', excluded for inactive/deleted company ' + CAST(ISNULL(@ExcludedInactiveCompany, 0) AS varchar(12))
        + '. Active employees assigned to NEITHER provider (IsTelesak NULL): '
        + CAST(ISNULL(@UnassignedActive, 0) AS varchar(12)) + '.';

    /* -------------------------------------------------------------------------
       Legacy assigned tasks: original query, unchanged, including uncorrelated
       ttls1. This exists only for the Current comparison. The corrected TaskTLog
       calculation is the separate block immediately after it.
       ---------------------------------------------------------------------- */
    SELECT
        a.TaskID,
        a.TempId,
        a.ExpectedTime,
        a.TimeUnitID
    INTO #AssignedTasksTempTable
    FROM
    ( SELECT Task.TaskID
          , TaskTLog.EmpID as TempId
          , ISNULL(Task.ExpectedTime,0) as ExpectedTime
          , Task.TimeUnitID as TimeUnitID

      FROM  TaskTLog
      JOIN Task      ON (TaskTLog.TaskID = Task.TaskID)
      JOIN Employee  ON (TaskTLog.EmpID=Employee.EmpID)
        JOIN [dbo].[UserAccount]  ON ([UserAccount].EmpID=Employee.EmpID)
      Where  (TaskTLog.EmpID=Employee.EmpID) and [IsTelesak]=@IsTelesak
      AND (Task.StartDate<=@PeriodEnd)

         AND (
                        EXISTS(
                         SELECT NULL AS [EMPTY]
                         FROM [TaskTLog] AS [ttls1]
                         WHERE (Employee.EmpID = [ttls1].EmpID)
                         AND ((ttls1.StatusID = 1 OR ttls1.StatusID = 2 ))
                         AND (ttls1.CreatedDate < @PeriodEnd)
                        )
                    )
         AND (NOT
                  (EXISTS(
                            SELECT NULL AS [EMPTY]
                     FROM [TaskTLog] AS [ttls2]
                      WHERE ([ttls2].[EmpID] = Employee.EmpID)
                      AND ([ttls2].[StatusID] = 5)
                      AND ([ttls2].[CreatedDate] < @PeriodStart)
                      AND([ttls2].TaskID=Task.TaskID)
                   ))
                        )

               AND (NOT (((
                    (CASE
                        WHEN NOT (EXISTS(
                            SELECT TOP (1) NULL AS [EMPTY]
                            FROM [TaskTLog] AS [ttls3]
                            WHERE ([ttls3].[EmpID] = Employee.EmpID)
                            AND ([ttls3].[CreatedDate] < @PeriodStart)
                            AND ([ttls3].TaskID=Task.TaskID)
                            ORDER BY [ttls3].[CreatedDate] DESC
                            )) THEN 0
                        ELSE
                            (CASE
                                WHEN ((
                                    SELECT [t7].[StatusID]
                                    FROM (
                                        SELECT TOP (1) [ttls4].[StatusID]
                                        FROM [TaskTLog] AS [ttls4]
                                        WHERE ([ttls4].[EmpID] = Employee.EmpID)
                                        AND ([ttls4].[CreatedDate] < @PeriodStart)
                                        AND ([ttls4].TaskID=Task.TaskID)
                                        ORDER BY [ttls4].[CreatedDate] DESC
                                        ) AS [t7]
                                    )) = 3 THEN 1
                                WHEN NOT (((
                                    SELECT [t7].[StatusID]
                                    FROM (
                                        SELECT TOP (1) [ttls5].[StatusID]
                                        FROM [TaskTLog] AS [ttls5]
                                        WHERE ([ttls5].[EmpID] =Employee.EmpID)
                                        AND ([ttls5].[CreatedDate] < @PeriodStart)
                                        AND ([ttls5].TaskID=Task.TaskID)
                                        ORDER BY [ttls5].[CreatedDate] DESC
                                        ) AS [t7]
                                    )) = 3) THEN 0
                                ELSE NULL
                             END)
                     END)) = 1)

                         AND (NOT (EXISTS(
                        SELECT NULL AS [EMPTY]
                        FROM [TaskTLog] AS [ttls6]
                        WHERE ([ttls6].[EmpID] = Employee.EmpID)
                         AND ([ttls6].[CreatedDate] >= @PeriodStart)
                         AND ([ttls6].[CreatedDate] < @PeriodEnd)
                         AND (ttls6.TaskID=Task.TaskID))
                  ))))
            AND (NOT ((
            (CASE
                WHEN NOT (EXISTS(
                    SELECT TOP (1) NULL AS [EMPTY]
                    FROM [TaskTLog] AS [ttls7]
                    WHERE ([ttls7].[CreatedDate] < @PeriodEnd)
                     AND ([ttls7].[EmpID] =  Employee.EmpID)
                      AND ([ttls7].TaskID=Task.TaskID)
                    ORDER BY [ttls7].[CreatedDate] DESC
                    )) THEN 0
                ELSE
                    (CASE
                        WHEN ((
                            SELECT [t13].[StatusID]
                            FROM (
                                SELECT TOP (1) [ttls8].[StatusID]
                                FROM [TaskTLog] AS [ttls8]
                                WHERE ([ttls8].[CreatedDate] < @PeriodEnd)
                                AND ([ttls8].[EmpID] = Employee.EmpID)
                                 AND ([ttls8].TaskID=Task.TaskID)
                                ORDER BY [ttls8].[CreatedDate] DESC
                                ) AS [t13]
                            )) = 9 THEN 1
                        WHEN NOT (((
                            SELECT [t13].[StatusID]
                            FROM (
                                SELECT TOP (1) [ttls8].[StatusID]
                                FROM [TaskTLog] AS [ttls8]
                                WHERE ([ttls8].[CreatedDate] < @PeriodEnd)
                                AND ([ttls8].[EmpID] = Employee.EmpID)
                                AND ([ttls8].TaskID=Task.TaskID)
                                ORDER BY [ttls8].[CreatedDate] DESC
                                ) AS [t13]
                            )) = 9) THEN 0
                        ELSE NULL
                     END)
             END)) = 1))

         Group By Task.TaskID,TaskTLog.EmpID ,SequenceNumber,ExpectedTime,TimeUnitID)
    AS a;

    /* -------------------------------------------------------------------------
       Corrected assigned tasks are calculated independently from the legacy
       set. This is important in both directions:

         * another task's New/Inprogress row must not make this task qualify;
         * a legitimate Accepted/Done-in-period task must not be dropped merely
           because this exact task has no status 1/2 row.

       The corrected period is derived only from TaskTLog status timestamps.
       Task.StartDate/EndDate are planned dates and are deliberately not used to
       decide whether a task was assigned during the report window. Every status
       test is correlated to this employee AND this TaskID. The legacy table
       above remains untouched so Current still reproduces the deployed procedure.
       ---------------------------------------------------------------------- */
    SELECT c.TempId,
           COUNT(*) AS CorrectedAssignedTasks,
           SUM(dbo.GetWorkMinutes(c.ExpectedTime, c.TimeUnitID)) AS CorrectedAssignedTaskesTime
    INTO #CorrectedAssignedSummary
    FROM (
        SELECT DISTINCT
               t.TaskID,
               tl.EmpID AS TempId,
               ISNULL(t.ExpectedTime, 0) AS ExpectedTime,
               t.TimeUnitID
        FROM dbo.TaskTLog AS tl
        JOIN dbo.Task AS t ON t.TaskID = tl.TaskID
        JOIN dbo.Employee AS e ON e.EmpID = tl.EmpID
        JOIN dbo.UserAccount AS ua ON ua.EmpID = e.EmpID
        WHERE ua.IsTelesak = @IsTelesak
          AND tl.CreatedDate < @PeriodEnd
          AND ISNULL(t.IsDeleted, 0) = 0

          -- Active at the opening of the window, based on the last status log
          -- before it, OR became active/completed inside the window. Done (5)
          -- inside the window is included because the task was active for at
          -- least part of that window. Approved (6) alone is not enough: the
          -- corresponding Done may have happened before the window.
          AND (
              ISNULL((SELECT TOP (1) opening.StatusID
                      FROM dbo.TaskTLog AS opening
                      WHERE opening.TaskID = t.TaskID
                        AND opening.EmpID = tl.EmpID
                        AND opening.CreatedDate < @PeriodStart
                      ORDER BY opening.CreatedDate DESC, opening.TaskTLogID DESC), 0)
                  IN (1, 2, 8, 10)
              OR EXISTS (
                  SELECT NULL
                  FROM dbo.TaskTLog AS insidePeriod
                  WHERE insidePeriod.TaskID = t.TaskID
                    AND insidePeriod.EmpID = tl.EmpID
                    AND insidePeriod.StatusID IN (1, 2, 5, 8, 10)
                    AND insidePeriod.CreatedDate >= @PeriodStart
                    AND insidePeriod.CreatedDate < @PeriodEnd
              )
          )

          -- A task pending before the window with no movement inside it was not active.
          AND NOT (
              ISNULL((SELECT TOP (1) pending.StatusID
                      FROM dbo.TaskTLog AS pending
                      WHERE pending.TaskID = t.TaskID
                        AND pending.EmpID = tl.EmpID
                        AND pending.CreatedDate < @PeriodStart
                      ORDER BY pending.CreatedDate DESC, pending.TaskTLogID DESC), 0) = 3
              AND NOT EXISTS (
                  SELECT NULL
                  FROM dbo.TaskTLog AS moved
                  WHERE moved.TaskID = t.TaskID
                    AND moved.EmpID = tl.EmpID
                    AND moved.CreatedDate >= @PeriodStart
                    AND moved.CreatedDate < @PeriodEnd
              )
          )

          -- Rejected before the end of the window was not an assigned task.
          AND ISNULL((SELECT TOP (1) lastStatus.StatusID
                      FROM dbo.TaskTLog AS lastStatus
                      WHERE lastStatus.TaskID = t.TaskID
                        AND lastStatus.EmpID = tl.EmpID
                        AND lastStatus.CreatedDate < @PeriodEnd
                      ORDER BY lastStatus.CreatedDate DESC, lastStatus.TaskTLogID DESC), 0) <> 9
    ) AS c
    GROUP BY c.TempId;


    --Login Count INTO #LoginTempTable
    SELECT *
    INTO #LoginTempTable
    FROM (
            SELECT lg.EmployeeID
                  ,Count(*) AS 'LoginCount'
                  -- The legacy value above counts sessions. The ministry report needs days:
                  -- three sign-ins on one calendar date are one login day. @PeriodEnd is the
                  -- next midnight, so the corrected count uses the half-open interval.
                  ,Count(DISTINCT CASE WHEN lg.ActionTime < @PeriodEnd
                                       THEN CONVERT(char(8), lg.ActionTime, 112) END)
                       AS 'CorrectedLoginCount'
            FROM   LogGeneral lg
             Join Employee Emp ON (Emp.EmpID = lg.EmployeeID)
             JOIN [dbo].[UserAccount]  ON ([UserAccount].EmpID=Emp.EmpID)
            WHERE  (lg.ActionID = 5 AND (lg.ActionTime >=@PeriodStart AND lg.ActionTime <= @PeriodEnd))and UserAccount.IsTelesak=@IsTelesak
            Group By lg.EmployeeID
            )As TempLogin;


    --LogOut Count INTO #LogOutTempTable
    SELECT *
    INTO #LogOutTempTable
    FROM (
            SELECT lg.EmployeeID
                  ,Count(*) AS 'LogOutCount'
                  ,Count(DISTINCT CASE WHEN lg.ActionTime < @PeriodEnd
                                       THEN CONVERT(char(8), lg.ActionTime, 112) END)
                       AS 'CorrectedLogOutCount'
            FROM   LogGeneral lg
             Join Employee Emp ON (Emp.EmpID = lg.EmployeeID)
             Join [dbo].[UserAccount]  ON ([UserAccount].EmpID=Emp.EmpID)
            WHERE  (lg.ActionID = 9 AND (lg.ActionTime >=@PeriodStart AND lg.ActionTime <= @PeriodEnd))and UserAccount.IsTelesak=@IsTelesak
            Group By lg.EmployeeID
            )As TempLogOut;


    --CompeletedTasks Count INTO #CompeletedTasks
    SELECT *
    INTO #CompeletedTasks
    FROM(
            SELECT TaskTLog.EmpID
                  ,COUNT(TaskTLog.StatusID) As 'CompletedTasks'
                  ,COUNT(DISTINCT TaskTLog.TaskID) As 'CorrectedCompletedTasks'
            FROM  TaskTLog
            WHERE
            (
                TaskTLog.CreatedDate >= @PeriodStart
                AND TaskTLog.CreatedDate < @PeriodEnd
                AND TaskTLog.StatusID = 5
             )
            Group By TaskTLog.EmpID
    ) As CompeletedTasks;


    /* -------------------------------------------------------------------------
       Work time, both ways, from one scan.

       TotalWorkTime          exactly as the live service computes it, including
                              the INT truncation and the NULL-unit rows that SUM
                              discards.
       CorrectedTotalWorkTime the same rows through dbo.GetWorkMinutes.

       TimUnitID is passed raw to GetWorkMinutes, not through ISNULL(...,0) --
       the function handles NULL itself and both mean the same thing to it.
       ---------------------------------------------------------------------- */
    SELECT *
    INTO #TotalWorkTime
    From (
        SELECT TaskTLog.EmpID
                ,SUm(dbo.GetCountMiniuts(TaskTLog.TimeCount,ISNULL(TaskTLog.TimUnitID, 0))) AS 'TotalWorkTime'
                ,SUM(dbo.GetWorkMinutes(TaskTLog.TimeCount, TaskTLog.TimUnitID))            AS 'CorrectedTotalWorkTime'
                FROM  TaskTLog
                WHERE
                TaskTLog.CreatedDate >= @PeriodStart
                AND  TaskTLog.CreatedDate <= @PeriodEnd
                Group By TaskTLog.EmpID
    )AS TotalWorkTime;


    /* -------------------------------------------------------------------------
       THE ONLY RESULT SET -- the report. Original columns first, corrected
       beside them.

       Ordered so the teleworkers who actually logged time come first. The
       default EmpID order buries them: in a typical Esnad window 8 of 100 people
       logged anything, so the first screenful is all zeros and the report reads
       as if nothing was recorded. That misreading is exactly what produced the
       wrong conclusion in the first place. Do not "restore" the natural order.
       ---------------------------------------------------------------------- */
    SELECT  Employee.EmpID
           ,Employee.NationalID
           ,Employee.SequenceNumber As SequenceNumber
           ,Employee.LaborOfficeID As LaborOfficeID
           ,ISNULL(CompletedTasks, 0)     As CompletedTasks
           ,COUNT(#AssignedTasksTempTable.TaskID) As 'AssignedTasks'
           ,ISNULL(SUM(#AssignedTasksTempTable.ExpectedTime),0) As 'ExpectedTime'
           ,ISNULL(SUM(dbo.GetCountMiniuts(ISNULL(#AssignedTasksTempTable.ExpectedTime, 0),ISNULL(#AssignedTasksTempTable.TimeUnitID, 0))),0) as AssignedTaskesTime
           ,ISNULL(TotalWorkTime , 0)As TotalWorkTime
           ,ISNULL([dbo].[GetActivityLevel] (TotalWorkTime,ISNULL(SUM(dbo.GetCountMiniuts(ISNULL(#AssignedTasksTempTable.ExpectedTime, 0),ISNULL(#AssignedTasksTempTable.TimeUnitID, 0))),0)), 0)  As ActivityLevel

           ,ISNULL(LoginCount, 0)     As LoginCount
           ,ISNULL(LogOutCount, 0)     As LogOutCount

           -- ---- corrected, same row ----------------------------------------
           ,ISNULL(#CorrectedAssignedSummary.CorrectedAssignedTasks, 0)
                As CorrectedAssignedTasks
           ,ISNULL(#CorrectedAssignedSummary.CorrectedAssignedTaskesTime, 0)
                As CorrectedAssignedTaskesTime
           ,ISNULL(CorrectedTotalWorkTime, 0)
                As CorrectedTotalWorkTime
           ,ISNULL([dbo].[GetActivityPercent](
                       CorrectedTotalWorkTime,
                       ISNULL(#CorrectedAssignedSummary.CorrectedAssignedTaskesTime, 0)), 0)
                As CorrectedActivityLevelSql
           ,ISNULL(CorrectedLoginCount, 0)
                As CorrectedLoginCount
           ,ISNULL(CorrectedLogOutCount, 0)
                As CorrectedLogOutCount
           ,ISNULL(CorrectedCompletedTasks, 0)
                As CorrectedCompletedTasks

           -- ---- who this row actually is -----------------------------------
           ,CAST(ISNULL(Employee.IsActive, 0)  AS BIT) As IsActive
           ,CAST(ISNULL(Employee.IsDeleted, 0) AS BIT) As IsDeleted
           ,CAST(CASE WHEN #PeriodActivity.EmpID IS NULL THEN 0 ELSE 1 END AS BIT) As HadPeriodActivity
           ,#Deactivations.DeactivatedAt As DeactivatedAt
           ,ISNULL(#MaxDaily.MaxDailyMinutes, 0) As MaxDailyMinutes

    FROM Employee
         Join [dbo].[UserAccount]  ON ([UserAccount].EmpID=Employee.EmpID)
         -- the company must be active and not deleted; applies to leavers too
         Join [dbo].[Company]      ON ([Company].CompanyID=Employee.CompanyID
                                       AND ISNULL([Company].IsActive, 0) = 1
                                       AND ISNULL([Company].IsDeleted, 0) = 0)
    Left Join #AssignedTasksTempTable ON (Employee.EmpID=#AssignedTasksTempTable.TempId)
    Left Join #CorrectedAssignedSummary ON (Employee.EmpID=#CorrectedAssignedSummary.TempId)
    Left Join #LoginTempTable ON (Employee.EmpID=#LoginTempTable.EmployeeID)
    Left Join #LogOutTempTable ON (Employee.EmpID=#LogOutTempTable.EmployeeID)
    Left Join #CompeletedTasks ON (Employee.EmpID=#CompeletedTasks.EmpID)
    Left Join #TotalWorkTime ON (Employee.EmpID=#TotalWorkTime.EmpID)
    Left Join #PeriodActivity ON (Employee.EmpID=#PeriodActivity.EmpID)
    Left Join #Deactivations ON (Employee.EmpID=#Deactivations.EmpID)
    Left Join #MaxDaily ON (Employee.EmpID=#MaxDaily.EmpID)

    where  UserAccount.IsTelesak=@IsTelesak
      AND (    (    (@ActiveOnly     = 0 OR ISNULL(Employee.IsActive,  0) = 1)
                AND (@ExcludeDeleted = 0 OR ISNULL(Employee.IsDeleted, 0) = 0))
           -- a leaver who did anything inside the period is still reported for it
           OR (@IncludePeriodActivity = 1 AND #PeriodActivity.EmpID IS NOT NULL))
    group BY   Employee.EmpID,Employee.NationalID,Employee.LaborOfficeID,Employee.SequenceNumber,LoginCount,LogOutCount,CorrectedLoginCount,CorrectedLogOutCount,CompletedTasks,CorrectedCompletedTasks,TotalWorkTime,CorrectedTotalWorkTime,Employee.IsActive,Employee.IsDeleted
              ,#CorrectedAssignedSummary.CorrectedAssignedTasks,#CorrectedAssignedSummary.CorrectedAssignedTaskesTime
              ,#PeriodActivity.EmpID,#Deactivations.DeactivatedAt,#MaxDaily.MaxDailyMinutes
    order BY   ISNULL(CorrectedTotalWorkTime, 0) DESC, ISNULL(CompletedTasks, 0) DESC, Employee.EmpID;

END
GO

PRINT 'dbo.GetWorkMinutes, dbo.GetActivityPercent, dbo.GetEvaluationServiceV2 created.';
GO

/* -----------------------------------------------------------------------------
   Sanity checks on the two functions. All must pass.
   -------------------------------------------------------------------------- */
SELECT 'fractional hours no longer truncate' AS Check_, dbo.GetWorkMinutes(0.75, 4) AS Actual, 45 AS Expected
UNION ALL SELECT 'whole hours unchanged',            dbo.GetWorkMinutes(2, 4),          120
UNION ALL SELECT 'missing unit counts as hours',     dbo.GetWorkMinutes(3, 0),          180
UNION ALL SELECT 'NULL unit counts as hours',        dbo.GetWorkMinutes(3, NULL),       180
UNION ALL SELECT 'minutes unchanged',                dbo.GetWorkMinutes(90, 5),          90
UNION ALL SELECT 'day unchanged',                    dbo.GetWorkMinutes(1, 3),          480
UNION ALL SELECT 'fractional day',                   dbo.GetWorkMinutes(1.5, 3),        720
UNION ALL SELECT 'unknown unit returns 0, not NULL', ISNULL(dbo.GetWorkMinutes(5, 99), -1), 0
UNION ALL SELECT 'no time logged stays NULL',        ISNULL(dbo.GetWorkMinutes(NULL, 4), -1), -1
UNION ALL SELECT 'exactly on target reports 100',    dbo.GetActivityPercent(480, 480),  100
UNION ALL SELECT 'over target caps at 100',          dbo.GetActivityPercent(720, 480),  100
UNION ALL SELECT 'half the target reports 50',       dbo.GetActivityPercent(240, 480),   50
UNION ALL SELECT 'sub-1% no longer collapses to 0',  dbo.GetActivityPercent(3, 480),      1
UNION ALL SELECT 'no target reports 0',              dbo.GetActivityPercent(480, 0),      0
UNION ALL SELECT 'no INT overflow at scale',         dbo.GetActivityPercent(530000, 1060000), 50;
GO

/* =============================================================================
   ROLLBACK -- these objects are additive and nothing else references them:

       DROP PROCEDURE dbo.GetEvaluationServiceV2;
       DROP FUNCTION  dbo.GetActivityPercent;
       DROP FUNCTION  dbo.GetWorkMinutes;

   STILL NOT ADDRESSED HERE
     defect 5  sp_TotalEmployeeTasks calls GetActivityLevel with its arguments
               reversed. It is a different consumer and is left alone on purpose;
               fix it in the CALLER, not by changing the shared function.
     defect 2  QvApi reads AssignedTaskesTime into TotalWorkTime. TeleworkReporter
               does not go through QvApi, so this path is unaffected.
   ============================================================================= */
