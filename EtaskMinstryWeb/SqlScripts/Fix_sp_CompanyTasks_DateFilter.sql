-- Fix T-01: Date Filter Bug in sp_CompanyTasks
-- Issue: ToDate and ToEndDate comparisons use <= with midnight timestamps,
--        excluding tasks from the selected end date that have a time component.
--
-- IMPORTANT: This script patches ONLY the 4 date comparison lines in the existing SP.
-- It preserves all other SP logic by reading the current definition and applying targeted replacements.
--
-- Compatible with SQL Server 2008 R2+

-- Step 1: Get the current SP definition
DECLARE @spText NVARCHAR(MAX) = ''

SELECT @spText = @spText + [text]
FROM sys.syscomments
WHERE id = OBJECT_ID('sp_CompanyTasks')
ORDER BY colid

IF @spText = ''
BEGIN
    PRINT 'ERROR: sp_CompanyTasks not found!'
    RETURN
END

-- Step 2: Replace the 4 date comparison lines to use CAST(... AS DATE)
-- This fixes the bug where tasks on the selected end date are excluded
-- because their time component (e.g., 14:30) is greater than midnight (00:00).

-- Fix FromDate: >= comparison (add CAST for consistency)
SET @spText = REPLACE(@spText,
    'or (t.StartDate >= @FromDate)',
    'or (CAST(t.StartDate AS DATE) >= CAST(@FromDate AS DATE))')

-- Fix ToDate: <= comparison (this is the main bug)
SET @spText = REPLACE(@spText,
    'or (t.StartDate <= @ToDate)',
    'or (CAST(t.StartDate AS DATE) <= CAST(@ToDate AS DATE))')

-- Fix FromEndDate: >= comparison
SET @spText = REPLACE(@spText,
    'or (t.EndDate >= @FromEndDate)',
    'or (CAST(t.EndDate AS DATE) >= CAST(@FromEndDate AS DATE))')

-- Fix ToEndDate: <= comparison (this is the main bug)
SET @spText = REPLACE(@spText,
    'or (t.EndDate <= @ToEndDate)',
    'or (CAST(t.EndDate AS DATE) <= CAST(@ToEndDate AS DATE))')

-- Step 3: Convert CREATE to ALTER if needed
IF @spText LIKE '%CREATE PROCEDURE%' OR @spText LIKE '%CREATE  PROCEDURE%'
BEGIN
    SET @spText = REPLACE(@spText, 'CREATE PROCEDURE', 'ALTER PROCEDURE')
    SET @spText = REPLACE(@spText, 'CREATE  PROCEDURE', 'ALTER PROCEDURE')
END

-- Step 4: Show the modified SP for review before executing
PRINT '-- Review the modified SP below. If it looks correct, uncomment the EXEC line at the bottom.'
PRINT @spText

-- Step 5: Uncomment the line below to apply the fix:
-- EXEC sp_executesql @spText
