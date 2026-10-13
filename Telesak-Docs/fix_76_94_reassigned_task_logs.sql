-- =====================================================================
-- Bugs #76 / #94 — OPTIONAL one-time data repair (SQL Server 2008 R2 compatible)
--
-- Before the fix, changing the employee on the Company "تعديل المهمة" page
-- wrote no TaskTLog rows. The previous employee's last log row for the task
-- stayed "New" (StatusID 1), so the task vanished from that employee and
-- later reappeared as جديدة once the new employee moved it past New.
--
-- This inserts the missing "Pending" (StatusID 3 = متوقفة) row for every
-- previous employee whose LAST log on a task is "New" while the task is now
-- assigned to someone else. It does NOT touch the current assignee.
--
-- Step 1 is a preview. Run Step 2 only after reviewing Step 1.
-- Back up TaskTLog first.
-- =====================================================================

-- Step 1: preview the rows that would be inserted
;WITH LastLog AS (
    SELECT l.TaskID, l.EmpID, l.StatusID, l.CreatedDate,
           ROW_NUMBER() OVER (PARTITION BY l.TaskID, l.EmpID ORDER BY l.TaskTLogID DESC) AS rn
    FROM TaskTLog l
    WHERE l.EmpID IS NOT NULL
)
SELECT t.TaskID, t.Title, ll.EmpID AS PreviousEmpID, t.EmpID AS CurrentEmpID,
       t.StatusID AS CurrentTaskStatus, ll.CreatedDate AS PreviousEmpLastLogDate
FROM LastLog ll
JOIN Task t ON t.TaskID = ll.TaskID
WHERE ll.rn = 1
  AND ll.StatusID = 1              -- previous employee's last log is New
  AND t.EmpID IS NOT NULL
  AND t.EmpID <> ll.EmpID          -- task now belongs to someone else
  AND t.IsDeleted = 0
ORDER BY t.TaskID;

-- Step 2: insert the missing Pending rows (uncomment to run)
/*
BEGIN TRAN;

;WITH LastLog AS (
    SELECT l.TaskID, l.EmpID, l.StatusID,
           ROW_NUMBER() OVER (PARTITION BY l.TaskID, l.EmpID ORDER BY l.TaskTLogID DESC) AS rn
    FROM TaskTLog l
    WHERE l.EmpID IS NOT NULL
)
INSERT INTO TaskTLog (TaskID, EmpID, StatusID, CreatedDate)
SELECT ll.TaskID, ll.EmpID, 3, GETDATE()
FROM LastLog ll
JOIN Task t ON t.TaskID = ll.TaskID
WHERE ll.rn = 1
  AND ll.StatusID = 1
  AND t.EmpID IS NOT NULL
  AND t.EmpID <> ll.EmpID
  AND t.IsDeleted = 0;

SELECT @@ROWCOUNT AS InsertedRows;
-- COMMIT;   -- or ROLLBACK;
*/
