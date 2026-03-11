-- Migration Script: Add LastHeartbeat and AutoCheckout columns to Attendance table
-- Run this script before deploying the updated application code
-- Date: 2026-03-11
-- Purpose: Enable server-side auto-checkout job to track heartbeat and auto-close sessions

-- Add LastHeartbeat column to track last activity time from JavaScript tracker
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Attendance]') AND name = 'LastHeartbeat')
BEGIN
    ALTER TABLE [dbo].[Attendance] ADD [LastHeartbeat] DATETIME NULL;
    PRINT 'Added LastHeartbeat column to Attendance table';
END
GO

-- Add AutoCheckout column to flag automatic checkouts
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Attendance]') AND name = 'AutoCheckout')
BEGIN
    ALTER TABLE [dbo].[Attendance] ADD [AutoCheckout] BIT NOT NULL DEFAULT 0;
    PRINT 'Added AutoCheckout column to Attendance table';
END
GO

-- Create index on LastHeartbeat for efficient orphan detection queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Attendance_LastHeartbeat_CheckOut')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Attendance_LastHeartbeat_CheckOut]
    ON [dbo].[Attendance] ([CheckOut], [LastHeartbeat])
    WHERE [CheckOut] IS NULL;
    PRINT 'Created index IX_Attendance_LastHeartbeat_CheckOut';
END
GO

-- Update existing open attendance records with current time as LastHeartbeat
-- This prevents them from being immediately closed by the auto-checkout job
UPDATE [dbo].[Attendance]
SET [LastHeartbeat] = GETDATE()
WHERE [CheckOut] IS NULL AND [LastHeartbeat] IS NULL;

PRINT 'Migration completed successfully';
GO
