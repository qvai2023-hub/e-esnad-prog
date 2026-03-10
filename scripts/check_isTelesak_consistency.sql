-- ============================================================
-- Script: Check IsTelesak consistency across UserAccount & Employee
-- Purpose: Find mismatches where UserAccount.CompanyID
--          does not match Employee.CompanyID
-- ============================================================

-- 1. Find UserAccounts where the linked Employee has a DIFFERENT CompanyID
--    (This means the cascade update would apply the wrong company's IsTelesak)
SELECT
    ua.ID AS UserAccountID,
    ua.UserName,
    ua.CompanyID AS UA_CompanyID,
    ua.EmpID,
    ua.IsCompany,
    ua.IsTelesak AS UA_IsTelesak,
    e.CompanyID AS Employee_CompanyID,
    e.Name AS EmployeeName
FROM UserAccount ua
INNER JOIN Employee e ON ua.EmpID = e.EmpID
WHERE ua.CompanyID != e.CompanyID
   OR (ua.CompanyID IS NULL AND e.CompanyID IS NOT NULL)
   OR (ua.CompanyID IS NOT NULL AND e.CompanyID IS NULL);


-- 2. Find employees whose IsTelesak differs from their company's main account
--    (The company account = UserAccount where IsCompany = 1)
SELECT
    emp_ua.ID AS EmployeeUserAccountID,
    emp_ua.UserName AS EmployeeUserName,
    emp_ua.CompanyID,
    emp_ua.IsTelesak AS Employee_IsTelesak,
    comp_ua.ID AS CompanyUserAccountID,
    comp_ua.UserName AS CompanyUserName,
    comp_ua.IsTelesak AS Company_IsTelesak
FROM UserAccount emp_ua
INNER JOIN UserAccount comp_ua
    ON emp_ua.CompanyID = comp_ua.CompanyID
    AND comp_ua.IsCompany = 1
WHERE emp_ua.IsCompany = 0
  AND ISNULL(emp_ua.IsTelesak, 0) != ISNULL(comp_ua.IsTelesak, 0);


-- 3. Find employees who have a UserAccount but with NULL CompanyID
SELECT
    ua.ID AS UserAccountID,
    ua.UserName,
    ua.EmpID,
    ua.CompanyID AS UA_CompanyID,
    ua.IsTelesak,
    e.Name AS EmployeeName,
    e.CompanyID AS Employee_CompanyID
FROM UserAccount ua
INNER JOIN Employee e ON ua.EmpID = e.EmpID
WHERE ua.CompanyID IS NULL
  AND e.CompanyID IS NOT NULL;


-- 4. Summary: Count of IsTelesak values per company
--    (all rows for a company should have the same IsTelesak value)
SELECT
    ua.CompanyID,
    c.Name AS CompanyName,
    COUNT(*) AS TotalAccounts,
    SUM(CASE WHEN ISNULL(ua.IsTelesak, 0) = 1 THEN 1 ELSE 0 END) AS TelesakCount,
    SUM(CASE WHEN ISNULL(ua.IsTelesak, 0) = 0 THEN 1 ELSE 0 END) AS NonTelesakCount
FROM UserAccount ua
INNER JOIN Company c ON ua.CompanyID = c.CompanyID
GROUP BY ua.CompanyID, c.Name
HAVING COUNT(DISTINCT ISNULL(CAST(ua.IsTelesak AS INT), 0)) > 1
ORDER BY ua.CompanyID;
