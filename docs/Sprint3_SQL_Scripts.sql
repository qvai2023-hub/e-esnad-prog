-- ============================================================================
-- Sprint 3 SQL Scripts
-- E-Esnad / Telesak Task Management System
-- ============================================================================

-- ============================================================================
-- T-09: Attachment Table - Verify/Ensure Structure
-- NOTE: No binary column added per requirement. Files stay on disk at
--       /Upload/Task/{FileName}. Only filename stored in database.
-- ============================================================================

-- Verify existing Attachment table structure (no changes needed)
-- Current schema:
-- CREATE TABLE [dbo].[Attachment](
--     [AttachmentID] [int] IDENTITY(1,1) NOT NULL,
--     [FileName] [nvarchar](50) NOT NULL,
--     [Description] [nvarchar](350) NULL,
--     [TaskID] [int] NULL,
--  CONSTRAINT [PK_Attachment] PRIMARY KEY CLUSTERED ([AttachmentID] ASC)
-- ) ON [PRIMARY]
-- FK: FK_Attachment_Task -> Task(TaskID) ON DELETE CASCADE

-- If FileName column is too short for some filenames, increase it (optional):
-- Uncomment the following if needed:
-- ALTER TABLE [dbo].[Attachment] ALTER COLUMN [FileName] NVARCHAR(255) NOT NULL;
-- GO

-- ============================================================================
-- T-10: Performance Index on Task Table
-- Improves query performance for task listing filtered by StatusID and CompanyID
-- ============================================================================

-- Check if index already exists before creating
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Task_StatusID_CompanyID'
    AND object_id = OBJECT_ID('dbo.Task')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Task_StatusID_CompanyID]
    ON [dbo].[Task] ([StatusID], [CompanyID])
    INCLUDE ([TaskID], [Title], [StartDate], [EndDate], [EmpID], [PriorityID], [IsDeleted], [IsArchived], [ProjectID], [DeliverDate])
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
    ON [PRIMARY];

    PRINT 'Index IX_Task_StatusID_CompanyID created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_Task_StatusID_CompanyID already exists.';
END
GO

-- Additional index for employee-based filtering (supports multi-employee search)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Task_EmpID_CompanyID'
    AND object_id = OBJECT_ID('dbo.Task')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Task_EmpID_CompanyID]
    ON [dbo].[Task] ([EmpID], [CompanyID])
    INCLUDE ([TaskID], [StatusID], [IsDeleted])
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
    ON [PRIMARY];

    PRINT 'Index IX_Task_EmpID_CompanyID created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_Task_EmpID_CompanyID already exists.';
END
GO
