-- ============================================================
-- Script: Fix UserAccount.CompanyID mismatch with Employee.CompanyID
-- The correct data is in the Employee table
-- ============================================================

-- Step 1: Preview the records that will be fixed
SELECT
    ua.ID AS UserAccountID,
    ua.UserName,
    ua.CompanyID AS OldCompanyID,
    e.CompanyID AS NewCompanyID,
    ua.IsTelesak AS OldIsTelesak,
    comp_ua.IsTelesak AS NewIsTelesak,
    e.Name AS EmployeeName
FROM UserAccount ua
INNER JOIN Employee e ON ua.EmpID = e.EmpID
LEFT JOIN UserAccount comp_ua
    ON comp_ua.CompanyID = e.CompanyID AND comp_ua.IsCompany = 1
WHERE ua.IsCompany = 0
  AND ua.CompanyID != e.CompanyID;

-- Step 2: Fix CompanyID — sync UserAccount.CompanyID from Employee.CompanyID
UPDATE ua
SET ua.CompanyID = e.CompanyID
FROM UserAccount ua
INNER JOIN Employee e ON ua.EmpID = e.EmpID
WHERE ua.IsCompany = 0
  AND ua.CompanyID != e.CompanyID;

-- Step 3: Fix IsTelesak — sync from the new company's main account
UPDATE ua
SET ua.IsTelesak = comp_ua.IsTelesak
FROM UserAccount ua
INNER JOIN Employee e ON ua.EmpID = e.EmpID
INNER JOIN UserAccount comp_ua
    ON comp_ua.CompanyID = e.CompanyID AND comp_ua.IsCompany = 1
WHERE ua.IsCompany = 0
  AND ISNULL(ua.IsTelesak, 0) != ISNULL(comp_ua.IsTelesak, 0);

-- Step 4: Verify CompanyID fix — should return 0 rows
SELECT ua.ID, ua.UserName, ua.CompanyID AS UA_CompanyID, e.CompanyID AS Emp_CompanyID
FROM UserAccount ua
INNER JOIN Employee e ON ua.EmpID = e.EmpID
WHERE ua.CompanyID != e.CompanyID;

-- ============================================================
-- Step 5: Find duplicate UserAccount records per Employee
-- (caused by UpdateRehireDeletedEmployee inserting without checking)
-- ============================================================
SELECT
    ua.EmpID,
    e.Name AS EmployeeName,
    COUNT(*) AS AccountCount,
    STUFF((SELECT ', ' + CAST(ua2.ID AS VARCHAR(20))
           FROM UserAccount ua2
           WHERE ua2.EmpID = ua.EmpID AND ua2.IsCompany = 0
           FOR XML PATH('')), 1, 2, '') AS UserAccountIDs,
    STUFF((SELECT ', ' + ua2.UserName
           FROM UserAccount ua2
           WHERE ua2.EmpID = ua.EmpID AND ua2.IsCompany = 0
           FOR XML PATH('')), 1, 2, '') AS UserNames
FROM UserAccount ua
INNER JOIN Employee e ON ua.EmpID = e.EmpID
WHERE ua.IsCompany = 0
GROUP BY ua.EmpID, e.Name
HAVING COUNT(*) > 1;

-- Step 6: Delete duplicate UserAccounts — keep only the latest one per Employee
-- Preview first:
SELECT del.ID, del.UserName, del.EmpID, del.CompanyID, 'WILL BE DELETED' AS Action
FROM UserAccount del
WHERE del.IsCompany = 0
  AND del.ID NOT IN (
    SELECT MAX(ua2.ID)
    FROM UserAccount ua2
    WHERE ua2.IsCompany = 0 AND ua2.EmpID IS NOT NULL
    GROUP BY ua2.EmpID
  )
  AND del.EmpID IN (
    SELECT EmpID FROM UserAccount WHERE IsCompany = 0 GROUP BY EmpID HAVING COUNT(*) > 1
  );

-- Uncomment to delete duplicates (keeps latest UserAccount per Employee):
-- DELETE FROM UserAccount
-- WHERE IsCompany = 0
--   AND ID NOT IN (
--     SELECT MAX(ua2.ID)
--     FROM UserAccount ua2
--     WHERE ua2.IsCompany = 0 AND ua2.EmpID IS NOT NULL
--     GROUP BY ua2.EmpID
--   )
--   AND EmpID IN (
--     SELECT EmpID FROM UserAccount WHERE IsCompany = 0 GROUP BY EmpID HAVING COUNT(*) > 1
--   );
