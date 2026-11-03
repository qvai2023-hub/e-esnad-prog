/* =============================================================================
   ROLLBACK for telework-fixed-objects.sql

   Removes the three objects that script created:

       dbo.GetEvaluationServiceV2   (procedure)
       dbo.GetActivityPercent       (function)
       dbo.GetWorkMinutes           (function)

   telework-fixed-objects.sql is additive: it never ALTERs an existing object.
   The client's original objects stay as they are and are NOT touched here:

       dbo.GetEvaluationService, dbo.GetCountMiniuts, dbo.GetActivityLevel

   After the rollback the ministry report (telework-reporter) can no longer call
   GetEvaluationServiceV2. Stop or switch it back to GetEvaluationService first.

   Safe to run more than once. The procedure goes first because it calls the
   two functions. If any OTHER object references these three, the script stops
   and drops nothing.

   TARGET ENGINE: SQL Server 2008 R2. No THROW / IIF / TRY_CAST / CONCAT.
   ============================================================================= */

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* -----------------------------------------------------------------------------
   1. Stop if anything other than these three objects depends on them
   -------------------------------------------------------------------------- */
IF EXISTS (
    SELECT 1
    FROM   sys.sql_expression_dependencies d
    WHERE  d.referenced_id IN (OBJECT_ID('dbo.GetEvaluationServiceV2'),
                               OBJECT_ID('dbo.GetActivityPercent'),
                               OBJECT_ID('dbo.GetWorkMinutes'))
      AND  d.referencing_id NOT IN (ISNULL(OBJECT_ID('dbo.GetEvaluationServiceV2'), 0),
                                    ISNULL(OBJECT_ID('dbo.GetActivityPercent'), 0),
                                    ISNULL(OBJECT_ID('dbo.GetWorkMinutes'), 0)))
BEGIN
    SELECT OBJECT_SCHEMA_NAME(d.referencing_id) + '.' + OBJECT_NAME(d.referencing_id) AS ReferencingObject,
           OBJECT_NAME(d.referenced_id) AS UsesObject
    FROM   sys.sql_expression_dependencies d
    WHERE  d.referenced_id IN (OBJECT_ID('dbo.GetEvaluationServiceV2'),
                               OBJECT_ID('dbo.GetActivityPercent'),
                               OBJECT_ID('dbo.GetWorkMinutes'))
      AND  d.referencing_id NOT IN (ISNULL(OBJECT_ID('dbo.GetEvaluationServiceV2'), 0),
                                    ISNULL(OBJECT_ID('dbo.GetActivityPercent'), 0),
                                    ISNULL(OBJECT_ID('dbo.GetWorkMinutes'), 0));

    RAISERROR('Rollback stopped: the objects listed above use the V2 objects. Nothing was dropped.', 16, 1);
    SET NOEXEC ON;   -- skip the rest of the script
END
GO

/* -----------------------------------------------------------------------------
   2. Drop, all or nothing
   -------------------------------------------------------------------------- */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.GetEvaluationServiceV2', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetEvaluationServiceV2;

IF OBJECT_ID('dbo.GetActivityPercent', 'FN') IS NOT NULL
    DROP FUNCTION dbo.GetActivityPercent;

IF OBJECT_ID('dbo.GetWorkMinutes', 'FN') IS NOT NULL
    DROP FUNCTION dbo.GetWorkMinutes;

COMMIT TRANSACTION;
GO

PRINT 'dbo.GetEvaluationServiceV2, dbo.GetActivityPercent, dbo.GetWorkMinutes removed.';
GO

SET NOEXEC OFF;
GO

/* -----------------------------------------------------------------------------
   3. Check: V2 objects gone, original objects still there. All must be PASS.
   -------------------------------------------------------------------------- */
SELECT 'GetEvaluationServiceV2 removed' AS Check_,
       CASE WHEN OBJECT_ID('dbo.GetEvaluationServiceV2') IS NULL THEN 'PASS' ELSE 'FAIL' END AS Result
UNION ALL SELECT 'GetActivityPercent removed',
       CASE WHEN OBJECT_ID('dbo.GetActivityPercent') IS NULL THEN 'PASS' ELSE 'FAIL' END
UNION ALL SELECT 'GetWorkMinutes removed',
       CASE WHEN OBJECT_ID('dbo.GetWorkMinutes') IS NULL THEN 'PASS' ELSE 'FAIL' END
UNION ALL SELECT 'original GetEvaluationService still there',
       CASE WHEN OBJECT_ID('dbo.GetEvaluationService', 'P') IS NOT NULL THEN 'PASS' ELSE 'FAIL' END
UNION ALL SELECT 'original GetCountMiniuts still there',
       CASE WHEN OBJECT_ID('dbo.GetCountMiniuts', 'FN') IS NOT NULL THEN 'PASS' ELSE 'FAIL' END
UNION ALL SELECT 'original GetActivityLevel still there',
       CASE WHEN OBJECT_ID('dbo.GetActivityLevel', 'FN') IS NOT NULL THEN 'PASS' ELSE 'FAIL' END;
GO
