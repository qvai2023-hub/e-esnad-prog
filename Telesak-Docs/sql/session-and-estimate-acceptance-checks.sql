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
   2. Every closed session has exactly ONE Logout (ActionID 9) at its LogoutAt
      covers: explicit Logout, closing the last tab (Timeout), re-login
   ------------------------------------------------------------------------- */
SELECT  'Check 2 - one Logout per closed session' AS [Check],
        s.UserSessionID, s.LoginAt, s.LastHeartbeatAt, s.LogoutAt, s.CloseReason,
        (SELECT COUNT(*) FROM dbo.LogGeneral lg
          WHERE lg.ActionID = 9 AND lg.EmployeeID = s.EmployeeID AND lg.ActionTime = s.LogoutAt) AS LogoutRows,
        CASE WHEN (SELECT COUNT(*) FROM dbo.LogGeneral lg
                    WHERE lg.ActionID = 9 AND lg.EmployeeID = s.EmployeeID AND lg.ActionTime = s.LogoutAt) = 1
             THEN 'PASS' ELSE 'FAIL (expected exactly 1)' END AS Result
FROM    dbo.UserSession s
WHERE   s.EmployeeID = @EmpID AND s.IsCompany = 0
  AND   s.LoginAt >= @Day AND s.LoginAt < @DayEnd
  AND   s.LogoutAt IS NOT NULL
ORDER BY s.LoginAt;

-- 2b. No more Logout events than closed sessions that day (nothing logged twice)
SELECT  'Check 2b - no duplicate Logouts' AS [Check],
        (SELECT COUNT(*) FROM dbo.LogGeneral
          WHERE ActionID = 9 AND EmployeeID = @EmpID AND ActionTime >= @Day AND ActionTime < @DayEnd) AS LogoutEvents,
        (SELECT COUNT(*) FROM dbo.UserSession
          WHERE EmployeeID = @EmpID AND IsCompany = 0 AND LogoutAt >= @Day AND LogoutAt < @DayEnd) AS ClosedSessions,
        CASE WHEN (SELECT COUNT(*) FROM dbo.LogGeneral
                    WHERE ActionID = 9 AND EmployeeID = @EmpID AND ActionTime >= @Day AND ActionTime < @DayEnd)
                = (SELECT COUNT(*) FROM dbo.UserSession
                    WHERE EmployeeID = @EmpID AND IsCompany = 0 AND LogoutAt >= @Day AND LogoutAt < @DayEnd)
             THEN 'PASS' ELSE 'FAIL (a Logout without a session, or logged twice)' END AS Result;

/* ---------------------------------------------------------------------------
   3. Closing one tab while another is open -> the session stays open
      Run this while the second tab is still open.
   ------------------------------------------------------------------------- */
SELECT  'Check 3 - session still open (run with the other tab open)' AS [Check],
        s.UserSessionID, s.LoginAt, s.LastHeartbeatAt,
        DATEDIFF(SECOND, s.LastHeartbeatAt, GETDATE()) AS SecondsSinceHeartbeat,
        CASE WHEN s.LogoutAt IS NULL AND DATEDIFF(SECOND, s.LastHeartbeatAt, GETDATE()) <= 120
             THEN 'PASS' ELSE 'FAIL (closed, or no heartbeat for 2 minutes)' END AS Result
FROM    dbo.UserSession s
WHERE   s.EmployeeID = @EmpID AND s.IsCompany = 0 AND s.LogoutAt IS NULL;

/* ---------------------------------------------------------------------------
   4. When the employee's last session closed, the attendance was checked out
   ------------------------------------------------------------------------- */
SELECT  'Check 4 - attendance checked out after the last session closed' AS [Check],
        a.Id, a.CheckIn, a.CheckOut, a.AutoCheckout,
        CASE WHEN a.CheckOut IS NOT NULL
               OR EXISTS (SELECT 1 FROM dbo.UserSession s
                           WHERE s.EmployeeID = @EmpID AND s.IsCompany = 0 AND s.LogoutAt IS NULL)
             THEN 'PASS' ELSE 'FAIL (open attendance with no open session)' END AS Result
FROM    dbo.Attendance a
WHERE   a.EmpId = @EmpID AND a.CheckIn >= @Day AND a.CheckIn < @DayEnd
ORDER BY a.CheckIn;

/* ---------------------------------------------------------------------------
   5. 1 Day = 8 Hours = 480 Minutes -> the same expected time and activity level
      (dbo.GetCountMiniuts / dbo.GetActivityLevel, used by the web app's reports)
      Note: GetActivityLevel returns (x % 100), so 100% shows as 0 - the same for
      both units. The corrected percentage is GetActivityPercent (V2 checks).
   ------------------------------------------------------------------------- */
SELECT  'Check 5 - 1 day = 8 hours = 480 minutes' AS [Check],
        dbo.GetCountMiniuts(1, 3)   AS OneDayMinutes,
        dbo.GetCountMiniuts(8, 4)   AS EightHoursMinutes,
        dbo.GetCountMiniuts(480, 5) AS MinutesMinutes,
        dbo.GetActivityLevel(240, dbo.GetCountMiniuts(1, 3)) AS ActivityOneDay,
        dbo.GetActivityLevel(240, dbo.GetCountMiniuts(8, 4)) AS ActivityEightHours,
        CASE WHEN dbo.GetCountMiniuts(1, 3) = 480 AND dbo.GetCountMiniuts(8, 4) = 480 AND dbo.GetCountMiniuts(480, 5) = 480
               AND dbo.GetActivityLevel(240, dbo.GetCountMiniuts(1, 3)) = dbo.GetActivityLevel(240, dbo.GetCountMiniuts(8, 4))
             THEN 'PASS' ELSE 'FAIL' END AS Result;

-- 5b. The agreed unit ids
SELECT  'Check 5b - unit ids' AS [Check], TimeUnitID, Name,
        CASE WHEN TimeUnitID IN (3, 4, 5) THEN 'used for new estimates' ELSE 'no longer offered' END AS Note
FROM    dbo.TimeUnit
ORDER BY TimeUnitID;

/* ---------------------------------------------------------------------------
   6. Tasks created or edited since the deployment save the estimate as a pair:
      a time > 0 always has TimeUnitID 3 / 4 / 5
   ------------------------------------------------------------------------- */
SELECT  'Check 6 - estimate saved with its unit' AS [Check],
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
