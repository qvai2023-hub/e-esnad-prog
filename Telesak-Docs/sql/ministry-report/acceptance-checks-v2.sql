/* =============================================================================
   Acceptance checks on the CORRECTED ministry report (fix/all-fixes only)

   READ ONLY. Needs telework-fixed-objects.sql (GetEvaluationServiceV2,
   GetWorkMinutes, GetActivityPercent) and the test steps of
   ../session-and-estimate-acceptance-checks.sql already done.

       @EmpID      the employee who signed in 3 times on @Day
       @Day        the test day
       @IsTelesak  1 = Telesak, 0 = Esnad (the employee's provider)

   TARGET ENGINE: SQL Server 2008 R2.
   ============================================================================= */

DECLARE @EmpID     INT      = 0;              -- <-- the test employee
DECLARE @Day       DATETIME = '2026-11-03';   -- <-- the test day
DECLARE @IsTelesak BIT      = 1;              -- <-- provider

/* ---------------------------------------------------------------------------
   1. 1 Day and 8 Hours: the same expected time and the same activity level
   ------------------------------------------------------------------------- */
SELECT  'V2 check 1 - 1 day = 8 hours = 480 minutes' AS [Check],
        dbo.GetWorkMinutes(1, 3)   AS OneDay,
        dbo.GetWorkMinutes(8, 4)   AS EightHours,
        dbo.GetWorkMinutes(480, 5) AS Minutes480,
        dbo.GetWorkMinutes(1.5, 3) AS OneAndHalfDays,           -- 720: the fraction is kept
        dbo.GetActivityPercent(240, dbo.GetWorkMinutes(1, 3)) AS ActivityOneDay,
        dbo.GetActivityPercent(240, dbo.GetWorkMinutes(8, 4)) AS ActivityEightHours,
        CASE WHEN dbo.GetWorkMinutes(1, 3) = 480 AND dbo.GetWorkMinutes(8, 4) = 480 AND dbo.GetWorkMinutes(480, 5) = 480
               AND dbo.GetWorkMinutes(1.5, 3) = 720
               AND dbo.GetActivityPercent(240, dbo.GetWorkMinutes(1, 3)) = dbo.GetActivityPercent(240, dbo.GetWorkMinutes(8, 4))
               AND dbo.GetActivityPercent(480, dbo.GetWorkMinutes(1, 3)) = 100
             THEN 'PASS' ELSE 'FAIL' END AS Result;

/* ---------------------------------------------------------------------------
   2. Three Logins on one day -> the corrected report shows ONE login day,
      and one logout day for the single Logout of the day
   ------------------------------------------------------------------------- */
IF OBJECT_ID('tempdb..#V2') IS NOT NULL DROP TABLE #V2;
CREATE TABLE #V2
(
    EmpID INT, NationalID NVARCHAR(100), SequenceNumber NVARCHAR(100), LaborOfficeID NVARCHAR(100),
    CompletedTasks INT, AssignedTasks INT, ExpectedTime DECIMAL(18,2), AssignedTaskesTime INT,
    TotalWorkTime INT, ActivityLevel INT, LoginCount INT, LogOutCount INT,
    CorrectedAssignedTasks INT, CorrectedAssignedTaskesTime DECIMAL(18,2), CorrectedTotalWorkTime DECIMAL(18,2),
    CorrectedActivityLevelSql DECIMAL(18,2), CorrectedLoginCount INT, CorrectedLogOutCount INT,
    CorrectedCompletedTasks INT,
    IsActive BIT, IsDeleted BIT, HadPeriodActivity BIT, DeactivatedAt DATETIME, MaxDailyMinutes DECIMAL(18,2)
);

DECLARE @PeriodEnd DATETIME = DATEADD(SECOND, -1, DATEADD(DAY, 1, @Day));
INSERT INTO #V2 EXEC dbo.GetEvaluationServiceV2 @PeriodStart = @Day, @PeriodEnd = @PeriodEnd, @IsTelesak = @IsTelesak;

SELECT  'V2 check 2 - 3 logins = 1 corrected login day' AS [Check],
        EmpID, LoginCount, CorrectedLoginCount, LogOutCount, CorrectedLogOutCount,
        CASE WHEN LoginCount >= 3 AND CorrectedLoginCount = 1 THEN 'PASS'
             ELSE 'FAIL (expected LoginCount >= 3 and CorrectedLoginCount = 1)' END AS Result
FROM    #V2
WHERE   EmpID = @EmpID;

DROP TABLE #V2;
