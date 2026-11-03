/* =============================================================================
   dbo.UserSession - one row per web sign-in (Company and Employee)

   Used by EtaskMinstry/Services/UserSessionTracker.cs (direct SQL, not in the EDMX).

   - Login       inserts the row (LoginAt = LastHeartbeatAt = now).
   - Every open tab sends a heartbeat each minute (Scripts/session-heartbeat.js)
     and updates LastHeartbeatAt.
   - The session is closed once, by whichever happens first:
       Logout   explicit Logout              LogoutAt = now
       Relogin  a new Login in the browser   LogoutAt = now
       Timeout  no heartbeat for 15 minutes  LogoutAt = LastHeartbeatAt
                (AutoCheckoutJob; appSettings SessionHeartbeatTimeoutMinutes)
     The close is UPDATE ... WHERE LogoutAt IS NULL, and the closer writes the
     single LogGeneral Logout row (ActionID = 9) and, for an employee with no
     other open session, checks out the open Attendance row.

   Additive only: no existing table changes. Until this script is run, the web
   app keeps its previous Login/Logout behaviour.

   TARGET ENGINE: SQL Server 2008 R2 (compatibility level 100).
   ============================================================================= */

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'dbo.UserSession', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserSession
    (
        UserSessionID   INT IDENTITY(1,1) NOT NULL,
        SessionKey      UNIQUEIDENTIFIER  NOT NULL,
        EmployeeID      INT               NULL,      -- set for Employee sign-ins
        CompanyID       INT               NULL,      -- the company (also set for employees)
        IsCompany       BIT               NOT NULL,
        IpAddress       NVARCHAR(64)      NULL,
        LoginAt         DATETIME          NOT NULL,
        LastHeartbeatAt DATETIME          NOT NULL,
        LogoutAt        DATETIME          NULL,      -- NULL while the session is open
        CloseReason     NVARCHAR(20)      NULL,      -- Logout / Relogin / Timeout
        CONSTRAINT PK_UserSession PRIMARY KEY CLUSTERED (UserSessionID),
        CONSTRAINT UQ_UserSession_SessionKey UNIQUE (SessionKey)
    );
END
GO

-- The timeout pass: open sessions by last heartbeat
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserSession_Open' AND object_id = OBJECT_ID(N'dbo.UserSession'))
    CREATE NONCLUSTERED INDEX IX_UserSession_Open
        ON dbo.UserSession (LogoutAt, LastHeartbeatAt);
GO

-- "Is this employee still signed in elsewhere?" before checking attendance out
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserSession_Employee' AND object_id = OBJECT_ID(N'dbo.UserSession'))
    CREATE NONCLUSTERED INDEX IX_UserSession_Employee
        ON dbo.UserSession (EmployeeID, LogoutAt);
GO
