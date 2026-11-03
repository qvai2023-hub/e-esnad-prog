/* =============================================================================
   Task history cannot be deleted: dbo.TaskLog and dbo.TaskTLog

   Any DELETE on either table is rolled back with an error, including the
   cascade from a hard DELETE of a dbo.Task row (FK_TaskLog_Task is ON DELETE
   CASCADE, so the task delete itself fails too). The application never hard
   deletes: tasks are soft-deleted (Task.IsDeleted = 1).

   UPDATE stays allowed: the app updates TaskTLog.TimeCount / TimUnitID when an
   employee logs time again on the same day.

   AFTER triggers are used because SQL Server does not allow an INSTEAD OF
   DELETE trigger on a table with an ON DELETE CASCADE foreign key.

   A DBA who must remove rows (e.g. test data) disables the trigger first:
       DISABLE TRIGGER dbo.TR_TaskLog_NoDelete  ON dbo.TaskLog;
       ... DELETE ...
       ENABLE  TRIGGER dbo.TR_TaskLog_NoDelete  ON dbo.TaskLog;

   ROLLBACK at the bottom (commented out).

   TARGET ENGINE: SQL Server 2008 R2. No THROW.
   ============================================================================= */

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.TR_TaskLog_NoDelete', 'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_TaskLog_NoDelete;
GO

CREATE TRIGGER dbo.TR_TaskLog_NoDelete ON dbo.TaskLog
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        RAISERROR(N'سجل المهمة (TaskLog) لا يمكن حذفه.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

IF OBJECT_ID('dbo.TR_TaskTLog_NoDelete', 'TR') IS NOT NULL
    DROP TRIGGER dbo.TR_TaskTLog_NoDelete;
GO

CREATE TRIGGER dbo.TR_TaskTLog_NoDelete ON dbo.TaskTLog
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        RAISERROR(N'سجل حالات المهمة (TaskTLog) لا يمكن حذفه.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

/* -----------------------------------------------------------------------------
   Check: both triggers exist and are enabled. A delete inside a transaction
   that is rolled back afterwards must fail. All must be PASS.
   -------------------------------------------------------------------------- */
SELECT 'TR_TaskLog_NoDelete enabled' AS Check_,
       CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TR_TaskLog_NoDelete' AND is_disabled = 0)
            THEN 'PASS' ELSE 'FAIL' END AS Result
UNION ALL SELECT 'TR_TaskTLog_NoDelete enabled',
       CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'TR_TaskTLog_NoDelete' AND is_disabled = 0)
            THEN 'PASS' ELSE 'FAIL' END;
GO

DECLARE @Blocked INT = 0;
BEGIN TRY
    BEGIN TRANSACTION;
    DELETE TOP (1) FROM dbo.TaskTLog;
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;   -- reached only if the trigger did not fire
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SET @Blocked = 1;
END CATCH
SELECT 'DELETE on TaskTLog is blocked' AS Check_, CASE WHEN @Blocked = 1 THEN 'PASS' ELSE 'FAIL' END AS Result;
GO

/* =============================================================================
   ROLLBACK
       DROP TRIGGER dbo.TR_TaskLog_NoDelete;
       DROP TRIGGER dbo.TR_TaskTLog_NoDelete;
   ============================================================================= */
