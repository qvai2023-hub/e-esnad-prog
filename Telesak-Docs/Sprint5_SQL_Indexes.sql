-- Sprint 5 (T-13): Suggested Performance Indexes
-- Execute in SSMS on the production database
-- These are safe CREATE INDEX statements (non-destructive)

-- Index for Dashboard Delayed tasks count query
-- Avoids full table scan when counting delayed tasks per company
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Task_CompanyID_IsDelayed' AND object_id = OBJECT_ID('Task'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Task_CompanyID_IsDelayed
    ON Task (CompanyID, isDelayed)
    INCLUDE (TaskID, StatusID, EmpID, StartDate, EndDate, delayTime, DelayPercentage);
END
GO

-- Index for Dashboard queries filtering by CompanyID + IsDeleted
-- Covers NeedAssign and general task list queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Task_CompanyID_IsDeleted' AND object_id = OBJECT_ID('Task'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Task_CompanyID_IsDeleted
    ON Task (CompanyID, IsDeleted)
    INCLUDE (TaskID, StatusID, EmpID, PriorityID, StartDate, EndDate, Title, DeliverDate);
END
GO
