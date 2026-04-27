-- ============================================================
-- MIGRATION: Fix events.Events schema mismatch
-- Run this script in SQL Server Management Studio (SSMS)
-- against the ABCDMallDB database.
-- ============================================================

USE [ABCDMallDB];
GO

-- ============================================================
-- 1. Add missing columns to events.Events
-- ============================================================

-- ApprovalStatus: 0=Pending, 1=Approved, 2=Rejected
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'events' AND TABLE_NAME = 'Events' AND COLUMN_NAME = 'ApprovalStatus'
)
BEGIN
    ALTER TABLE [events].[Events] ADD [ApprovalStatus] int NOT NULL DEFAULT 0;
    PRINT 'Added ApprovalStatus column';
END
ELSE
    PRINT 'ApprovalStatus already exists - skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'events' AND TABLE_NAME = 'Events' AND COLUMN_NAME = 'RejectionReason'
)
BEGIN
    ALTER TABLE [events].[Events] ADD [RejectionReason] nvarchar(max) NULL;
    PRINT 'Added RejectionReason column';
END
ELSE
    PRINT 'RejectionReason already exists - skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'events' AND TABLE_NAME = 'Events' AND COLUMN_NAME = 'HasGiftRegistration'
)
BEGIN
    ALTER TABLE [events].[Events] ADD [HasGiftRegistration] bit NOT NULL DEFAULT 0;
    PRINT 'Added HasGiftRegistration column';
END
ELSE
    PRINT 'HasGiftRegistration already exists - skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'events' AND TABLE_NAME = 'Events' AND COLUMN_NAME = 'GiftDescription'
)
BEGIN
    ALTER TABLE [events].[Events] ADD [GiftDescription] nvarchar(500) NULL;
    PRINT 'Added GiftDescription column';
END
ELSE
    PRINT 'GiftDescription already exists - skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'events' AND TABLE_NAME = 'Events' AND COLUMN_NAME = 'ApprovedAt'
)
BEGIN
    ALTER TABLE [events].[Events] ADD [ApprovedAt] datetime2 NULL;
    PRINT 'Added ApprovedAt column';
END
ELSE
    PRINT 'ApprovedAt already exists - skipped';
GO

-- ============================================================
-- 2. Add DEFAULT constraints for old NOT NULL columns that
--    EF Core no longer maps (IsHot, Location).
--    Without a DEFAULT, INSERT from EF will fail because
--    the entity no longer has these properties.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID('events.Events')
      AND COL_NAME(parent_object_id, parent_column_id) = 'IsHot'
)
BEGIN
    ALTER TABLE [events].[Events] ADD CONSTRAINT [DF_Events_IsHot] DEFAULT 0 FOR [IsHot];
    PRINT 'Added DEFAULT 0 constraint for IsHot';
END
ELSE
    PRINT 'IsHot DEFAULT already exists - skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID('events.Events')
      AND COL_NAME(parent_object_id, parent_column_id) = 'Location'
)
BEGIN
    ALTER TABLE [events].[Events] ADD CONSTRAINT [DF_Events_Location] DEFAULT N'' FOR [Location];
    PRINT 'Added DEFAULT empty string constraint for Location';
END
ELSE
    PRINT 'Location DEFAULT already exists - skipped';
GO

-- ============================================================
-- 3. Add missing columns to shops.Shops (if not already present)
--    These should already exist from MallDbContext migration,
--    but added here as a safety net.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'shops' AND TABLE_NAME = 'Shops' AND COLUMN_NAME = 'ShopStatus'
)
BEGIN
    ALTER TABLE [shops].[Shops] ADD [ShopStatus] nvarchar(20) NOT NULL DEFAULT 'Active';
    PRINT 'Added ShopStatus column to shops.Shops';
END
ELSE
    PRINT 'ShopStatus already exists in shops.Shops - skipped';
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'shops' AND TABLE_NAME = 'Shops' AND COLUMN_NAME = 'OpeningDate'
)
BEGIN
    ALTER TABLE [shops].[Shops] ADD [OpeningDate] datetime2 NULL;
    PRINT 'Added OpeningDate column to shops.Shops';
END
ELSE
    PRINT 'OpeningDate already exists in shops.Shops - skipped';
GO

PRINT '=== Migration complete ===';
