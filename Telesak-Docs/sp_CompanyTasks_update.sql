-- =====================================================================
-- Sprint 1: Task Report Improvements (T-02 + T-04 + T-05)
-- SQL Server 2008 R2 Compatible
-- =====================================================================
-- Run this script on the EtaskMinistry database
-- =====================================================================
-- IMPORTANT NOTES FOR SQL 2008 R2:
--   1. IIF() does NOT exist in SQL 2008 R2 — use CASE WHEN instead
--   2. TRY_CONVERT() does NOT exist — use CONVERT() with CASE WHEN
--   3. STRING_AGG() does NOT exist — use FOR XML PATH if needed
--   4. OFFSET/FETCH does NOT exist — use TOP or ROW_NUMBER()
--   5. Make sure to backup the database before running this script
-- =====================================================================

-- Step 1: Check if the SP exists before modifying
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CompanyTasks]') AND type IN (N'P', N'PC'))
BEGIN
    PRINT 'Found sp_CompanyTasks - proceeding with modification...'
END
ELSE
BEGIN
    PRINT 'ERROR: sp_CompanyTasks not found! Check the database.'
    -- Stop execution
    RETURN
END
GO

-- Step 2: Get current SP definition (for your reference/backup)
-- Run this SELECT first to save a backup of the current SP:
--
--   SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.sp_CompanyTasks'))
--
-- Save the result to a .sql file as backup before proceeding.

-- =====================================================================
-- Step 3: ALTER the stored procedure
-- =====================================================================
-- INSTRUCTIONS:
--   1. First run the SELECT above to get current SP text
--   2. Copy the full CREATE PROCEDURE ... AS BEGIN ... END
--   3. Change CREATE to ALTER
--   4. Add the @CalendarType parameter (see below)
--   5. Replace the StartDate and EndDate SELECT columns (see below)
--   6. Run the modified ALTER PROCEDURE
-- =====================================================================

-- =====================================================================
-- WHAT TO CHANGE IN THE SP (3 changes total):
-- =====================================================================

-- =====================================================================
-- CHANGE 1: Add new parameter to the SP signature
-- =====================================================================
-- FIND the parameter list (looks something like):
--
--   CREATE PROCEDURE [dbo].[sp_CompanyTasks]
--       @CompanyId      BIGINT,
--       @ProjectIds     NVARCHAR(MAX),
--       @EmpId          NVARCHAR(MAX),
--       @notInEmpList   BIT,
--       @notInProjectList BIT,
--       @StatusId       NVARCHAR(MAX),
--       @PriorityId     NVARCHAR(MAX),
--       @FromDate       DATETIME = NULL,
--       @ToDate         DATETIME = NULL,
--       @taskName       NVARCHAR(MAX),
--       @FromEndDate    DATETIME = NULL,
--       @ToEndDate      DATETIME = NULL
--
-- ADD this line at the END of the parameter list:
--       ,@CalendarType   INT = 0
--
-- The = 0 means DEFAULT is Hijri (backward compatible with existing callers)

-- =====================================================================
-- CHANGE 2: Replace the StartDate column in the SELECT
-- =====================================================================
-- FIND something like:
--       SUBSTRING(CAST([dbo].[GetHijriDate](t.StartDate) AS NVARCHAR(50)), 1, 10) AS StartDate
--
-- REPLACE WITH:
--       CASE
--           WHEN @CalendarType = 1
--           THEN CONVERT(NVARCHAR(10), t.StartDate, 111)
--           ELSE SUBSTRING(CAST([dbo].[GetHijriDate](t.StartDate) AS NVARCHAR(50)), 1, 10)
--       END AS StartDate

-- =====================================================================
-- CHANGE 3: Replace the EndDate column in the SELECT
-- =====================================================================
-- FIND something like:
--       SUBSTRING(CAST([dbo].[GetHijriDate](t.EndDate) AS NVARCHAR(50)), 1, 10) AS EndDate
--
-- REPLACE WITH:
--       CASE
--           WHEN @CalendarType = 1
--           THEN CONVERT(NVARCHAR(10), t.EndDate, 111)
--           ELSE SUBSTRING(CAST([dbo].[GetHijriDate](t.EndDate) AS NVARCHAR(50)), 1, 10)
--       END AS EndDate

-- =====================================================================
-- VERIFICATION: Run these after applying changes
-- =====================================================================

-- Test 1: Hijri mode (default, backward compatible)
-- EXEC sp_CompanyTasks @CompanyId=0, @ProjectIds='', @EmpId='',
--     @notInEmpList=0, @notInProjectList=0, @StatusId='', @PriorityId='',
--     @FromDate=NULL, @ToDate=NULL, @taskName='',
--     @FromEndDate=NULL, @ToEndDate=NULL, @CalendarType=0

-- Test 2: Gregorian mode (new)
-- EXEC sp_CompanyTasks @CompanyId=0, @ProjectIds='', @EmpId='',
--     @notInEmpList=0, @notInProjectList=0, @StatusId='', @PriorityId='',
--     @FromDate=NULL, @ToDate=NULL, @taskName='',
--     @FromEndDate=NULL, @ToEndDate=NULL, @CalendarType=1

-- Expected: Both should return results.
-- In Test 1: StartDate/EndDate should be in Hijri format
-- In Test 2: StartDate/EndDate should be in yyyy/MM/dd Gregorian format
-- Example Gregorian: 2026/03/16
-- Example Hijri:     1447/09/16

-- =====================================================================
-- SQL 2008 R2 COMPATIBILITY REMINDER:
-- =====================================================================
-- CONVERT style 111 = yyyy/MM/dd format (works in SQL 2008 R2)
-- CASE WHEN is used (IIF is NOT available in SQL 2008 R2)
-- @CalendarType INT = 0 default ensures old callers still work
-- No changes to the result set structure (same columns returned)
-- =====================================================================
