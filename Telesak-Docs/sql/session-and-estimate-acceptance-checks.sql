/* =============================================================================
   Acceptance checks - sign-in session / Logout and the task estimated time

   READ ONLY. Run after the manual test steps on a test database, with the
   employee and the day you tested:

       @EmpID  the employee who signed in during the test
       @Day    the test day
       @Since  when the new code was deployed (estimate consistency check)

   Each check returns Result = PASS / FAIL with what it saw.
   Works on both branches (fix/all-except-ministry and fix/all-fixes). The
   corrected report checks (CorrectedLoginCount, GetWorkMinutes,
   GetActivityPercent) are in ministry-report/acceptance-checks-v2.sql, on
   fix/all-fixes only.

   TARGET ENGINE: SQL Server 2008 R2.
   ============================================================================= */

DECLARE @EmpID INT      = 0;               -- <-- the test employee
DECLARE @Day   DATETIME = '2026-11-03';    -- <-- the test day
DECLARE @Since DATETIME = '2026-11-03';    -- <-- deployment time of this change

DECLARE @DayEnd DATETIME = DATEADD(DAY, 1, @Day);

/* ---------------------------------------------------------------------------
   1. Three Logins in the same day -> three Login events (ActionID 5)
      (the corrected report counting them as ONE day is checked in
       ministry-report/acceptance-checks-v2.sql)
   ------------------------------------------------------------------------- */
SELECT  'Check 1 - logins on the day' AS [Check],
        COUNT(*) AS LoginEvents,
        COUNT(DISTINCT CONVERT(VARCHAR(10), ActionTime, 120)) AS LoginDays,
        CASE WHEN COUNT(*) >= 3 AND COUNT(DISTINCT CONVERT(VARCHAR(10), ActionTime, 120)) = 1
             THEN 'PASS' ELSE 'FAIL (expected >= 3 events on 1 day)' END AS Result
FROM    dbo.LogGeneral
WHERE   ActionID = 5 AND EmployeeID = @EmpID
  AND   ActionTime >= @Day AND ActionTime < @DayEnd;

/* ---------------------------------------------------------------------------
   2. One Logout (ActionID 9) per Login: a Logout never follows another Logout
      covers: Logout button, inactivity checkout, heartbeat timeout, and a stale
      tab clicking Logout after the sign-in was already ended
   ------------------------------------------------------------------------- */
;WITH ev AS (
    SELECT  LogID, ActionID, ActionTime,
            ROW_NUMBER() OVER (ORDER BY ActionTime, LogID) AS rn
    FROM    dbo.LogGeneral
    WHERE   ActionID IN (5, 9) AND EmployeeID = @EmpID AND ISNULL(IsCompany, 0) = 0
      AND   ActionTime >= @Day AND ActionTime < @DayEnd
)
SELECT  'Check 2 - no Logout written twice' AS [Check],
        (SELECT COUNT(*) FROM ev WHERE ActionID = 5) AS LoginEvents,
        (SELECT COUNT(*) FROM ev WHERE ActionID = 9) AS LogoutEvents,
        (SELECT COUNT(*) FROM ev cur JOIN ev prv ON prv.rn = cur.rn - 1
          WHERE cur.ActionID = 9 AND prv.ActionID = 9) AS LogoutAfterLogout,
        CASE WHEN (SELECT COUNT(*) FROM ev cur JOIN ev prv ON prv.rn = cur.rn - 1
                    WHERE cur.ActionID = 9 AND prv.ActionID = 9) = 0
              AND (SELECT COUNT(*) FROM ev WHERE ActionID = 9) <= (SELECT COUNT(*) FROM ev WHERE ActionID = 5)
             THEN 'PASS' ELSE 'FAIL (a Logout was logged twice)' END AS Result;

-- 2b. The day's events in order, to read alongside Check 2
SELECT  'Check 2b - Login/Logout sequence' AS [Check], LogID, ActionID,
        CASE ActionID WHEN 5 THEN 'Login' ELSE 'Logout' END AS Action, ActionTime, Value AS IP
FROM    dbo.LogGeneral
WHERE   ActionID IN (5, 9) AND EmployeeID = @EmpID AND ISNULL(IsCompany, 0) = 0
  AND   ActionTime >= @Day AND ActionTime < @DayEnd
ORDER BY ActionTime, LogID;

/* ---------------------------------------------------------------------------
   3. One attendance row per employee per day, however many times they sign in
   ------------------------------------------------------------------------- */
SELECT  'Check 3 - one attendance row on the day' AS [Check],
        COUNT(*) AS AttendanceRows,
        CASE WHEN COUNT(*) = 1 THEN 'PASS' ELSE 'FAIL (expected exactly 1)' END AS Result
FROM    dbo.Attendance
WHERE   EmpId = @EmpID AND CheckIn >= @Day AND CheckIn < @DayEnd;

-- 3b. Nobody got a second row on the same day since the deployment
SELECT  'Check 3b - duplicate days since deployment' AS [Check],
        COUNT(*) AS EmployeeDaysWithDuplicates,
        CASE WHEN COUNT(*) = 0 THEN 'PASS' ELSE 'FAIL (see the list below)' END AS Result
FROM   (SELECT EmpId, CONVERT(VARCHAR(10), CheckIn, 120) AS AttDay
        FROM   dbo.Attendance
        WHERE  CheckIn >= @Since
        GROUP BY EmpId, CONVERT(VARCHAR(10), CheckIn, 120)
        HAVING COUNT(*) > 1) d;

SELECT  TOP 50 EmpId, CONVERT(VARCHAR(10), CheckIn, 120) AS AttDay, COUNT(*) AS RowsOnDay
FROM    dbo.Attendance
WHERE   CheckIn >= @Since
GROUP BY EmpId, CONVERT(VARCHAR(10), CheckIn, 120)
HAVING  COUNT(*) > 1
ORDER BY AttDay DESC;

/* ---------------------------------------------------------------------------
   4. Closing one tab while another is open -> still checked in, heartbeat fresh
      Run this while the second tab is still open.
   ------------------------------------------------------------------------- */
SELECT  'Check 4 - still checked in (run with the other tab open)' AS [Check],
        a.Id, a.CheckIn, a.LastHeartbeat,
        DATEDIFF(SECOND, a.LastHeartbeat, GETDATE()) AS SecondsSinceHeartbeat,
        CASE WHEN a.CheckOut IS NULL AND DATEDIFF(SECOND, a.LastHeartbeat, GETDATE()) <= 120
             THEN 'PASS' ELSE 'FAIL (checked out, or no heartbeat for 2 minutes)' END AS Result
FROM    dbo.Attendance a
WHERE   a.EmpId = @EmpID AND a.CheckIn >= @Day AND a.CheckIn < @DayEnd;

/* ---------------------------------------------------------------------------
   5. After the last Logout of the day the attendance is checked out, and the
      checkout is not before that Logout's sign-in
   ------------------------------------------------------------------------- */
SELECT  'Check 5 - checked out after the last Logout' AS [Check],
        a.Id, a.CheckIn, a.CheckOut, a.AutoCheckout,
        (SELECT TOP 1 ActionID FROM dbo.LogGeneral
          WHERE ActionID IN (5, 9) AND EmployeeID = @EmpID AND ISNULL(IsCompany, 0) = 0
          ORDER BY ActionTime DESC, LogID DESC) AS LastEvent,
        CASE WHEN (SELECT TOP 1 ActionID FROM dbo.LogGeneral
                    WHERE ActionID IN (5, 9) AND EmployeeID = @EmpID AND ISNULL(IsCompany, 0) = 0
                    ORDER BY ActionTime DESC, LogID DESC) = 5
               OR a.CheckOut IS NOT NULL
             THEN 'PASS' ELSE 'FAIL (signed out but the attendance is still open)' END AS Result
FROM    dbo.Attendance a
WHERE   a.EmpId = @EmpID AND a.CheckIn >= @Day AND a.CheckIn < @DayEnd;

/* ---------------------------------------------------------------------------
   6. 1 Day = 8 Hours = 480 Minutes -> the same expected time and activity level
      (dbo.GetCountMiniuts / dbo.GetActivityLevel, used by the web app's reports)
      Note: GetActivityLevel returns (x % 100), so 100% shows as 0 - the same for
      both units. The corrected percentage is GetActivityPercent (V2 checks).
   ------------------------------------------------------------------------- */
SELECT  'Check 6 - 1 day = 8 hours = 480 minutes' AS [Check],
        dbo.GetCountMiniuts(1, 3)   AS OneDayMinutes,
        dbo.GetCountMiniuts(8, 4)   AS EightHoursMinutes,
        dbo.GetCountMiniuts(480, 5) AS MinutesMinutes,
        dbo.GetActivityLevel(240, dbo.GetCountMiniuts(1, 3)) AS ActivityOneDay,
        dbo.GetActivityLevel(240, dbo.GetCountMiniuts(8, 4)) AS ActivityEightHours,
        CASE WHEN dbo.GetCountMiniuts(1, 3) = 480 AND dbo.GetCountMiniuts(8, 4) = 480 AND dbo.GetCountMiniuts(480, 5) = 480
               AND dbo.GetActivityLevel(240, dbo.GetCountMiniuts(1, 3)) = dbo.GetActivityLevel(240, dbo.GetCountMiniuts(8, 4))
             THEN 'PASS' ELSE 'FAIL' END AS Result;

-- 6b. The agreed unit ids
SELECT  'Check 6b - unit ids' AS [Check], TimeUnitID, Name,
        CASE WHEN TimeUnitID IN (3, 4, 5) THEN 'used for new estimates' ELSE 'no longer offered' END AS Note
FROM    dbo.TimeUnit
ORDER BY TimeUnitID;

/* ---------------------------------------------------------------------------
   7. Tasks created or edited since the deployment save the estimate as a pair:
      a time > 0 always has TimeUnitID 3 / 4 / 5
   ------------------------------------------------------------------------- */
SELECT  'Check 7 - estimate saved with its unit' AS [Check],
        COUNT(*) AS BadTasks,
        CASE WHEN COUNT(*) = 0 THEN 'PASS' ELSE 'FAIL (see the list below)' END AS Result
FROM    dbo.Task t
WHERE   t.ExpectedTime > 0
  AND   (t.TimeUnitID IS NULL OR t.TimeUnitID NOT IN (3, 4, 5))
  AND   (t.CreatedDate >= @Since
         OR EXISTS (SELECT 1 FROM dbo.TaskLog l WHERE l.TaskID = t.TaskID AND l.LogDate >= @Since));

SELECT  TOP 50 t.TaskID, t.Title, t.CreatedDate, t.ExpectedTime, t.TimeUnitID
FROM    dbo.Task t
WHERE   t.ExpectedTime > 0
  AND   (t.TimeUnitID IS NULL OR t.TimeUnitID NOT IN (3, 4, 5))
  AND   t.CreatedDate >= @Since
ORDER BY t.CreatedDate DESC;
