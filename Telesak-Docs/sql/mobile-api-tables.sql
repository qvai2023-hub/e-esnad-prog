/******************************************************************************
 * Mobile API Layer - Schema migration
 * Slice 1: Foundation
 *
 * PURELY ADDITIVE. Does not modify existing tables.
 * The web app does not read or write these tables.
 *
 * Run order:
 *   1. MobileDeviceToken   - stores FCM device tokens per user
 *   2. RefreshToken        - stores SHA-256 hashes of refresh tokens
 *
 * Run this script ONCE per environment BEFORE deploying Slice 1.
 ******************************************************************************/

SET NOCOUNT ON;
GO

/* -------------------------------------------------------------------------- */
/* 1. MobileDeviceToken                                                        */
/* -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MobileDeviceToken')
BEGIN
    CREATE TABLE MobileDeviceToken (
        ID              INT IDENTITY(1,1) PRIMARY KEY,
        UserID          INT NOT NULL,
        DeviceToken     NVARCHAR(500) NOT NULL,
        Platform        NVARCHAR(10) NOT NULL,
        DeviceModel     NVARCHAR(100) NULL,
        CreatedDate     DATETIME NOT NULL DEFAULT GETDATE(),
        LastSeenDate    DATETIME NOT NULL DEFAULT GETDATE(),
        IsActive        BIT NOT NULL DEFAULT 1
    );

    CREATE INDEX IX_MobileDeviceToken_UserID
        ON MobileDeviceToken (UserID);

    CREATE INDEX IX_MobileDeviceToken_Token
        ON MobileDeviceToken (DeviceToken);

    PRINT 'Created table: MobileDeviceToken';
END
ELSE
BEGIN
    PRINT 'Skipped: MobileDeviceToken already exists';
END
GO

/* -------------------------------------------------------------------------- */
/* 2. RefreshToken                                                             */
/* -------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RefreshToken')
BEGIN
    CREATE TABLE RefreshToken (
        ID                    INT IDENTITY(1,1) PRIMARY KEY,
        UserID                INT NOT NULL,
        UserTypeID            INT NOT NULL,
        TokenHash             NVARCHAR(64) NOT NULL,
        ExpiresAt             DATETIME NOT NULL,
        IsRevoked             BIT NOT NULL DEFAULT 0,
        CreatedAt             DATETIME NOT NULL DEFAULT GETDATE(),
        ReplacedByTokenId     INT NULL,
        RevokedAt             DATETIME NULL,
        RevokeReason          NVARCHAR(50) NULL
    );

    CREATE INDEX IX_RefreshToken_UserID
        ON RefreshToken (UserID);

    CREATE UNIQUE INDEX IX_RefreshToken_Hash
        ON RefreshToken (TokenHash);

    PRINT 'Created table: RefreshToken';
END
ELSE
BEGIN
    PRINT 'Skipped: RefreshToken already exists';
END
GO

PRINT 'Mobile API schema migration complete.';
GO
