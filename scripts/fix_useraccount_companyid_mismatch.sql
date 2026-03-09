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

-- Step 4: Verify — should return 0 rows
SELECT ua.ID, ua.UserName, ua.CompanyID AS UA_CompanyID, e.CompanyID AS Emp_CompanyID
FROM UserAccount ua
INNER JOIN Employee e ON ua.EmpID = e.EmpID
WHERE ua.CompanyID != e.CompanyID;
