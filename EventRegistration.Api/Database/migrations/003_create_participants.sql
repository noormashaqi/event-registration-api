CREATE TABLE IF NOT EXISTS `Participants` (
    `Id`          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `FullName`    VARCHAR(150) NOT NULL,
    `Email`       VARCHAR(255) NOT NULL,
    `Phone`       VARCHAR(30) NOT NULL,
    `DateOfBirth` DATE NULL,
    `IsActive`    TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt`   DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    `UpdatedAt`   DATETIME NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UQ_Participants_Email` (`Email`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci;

-- CREATE INDEX has no IF NOT EXISTS clause on older MySQL 8.x, so guard
-- each one manually via information_schema to keep this migration
-- safe to re-run.
SET @idx_exists := (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = DATABASE() AND table_name = 'Participants' AND index_name = 'IX_Participants_FullName');
SET @sql := IF(@idx_exists = 0, 'CREATE INDEX `IX_Participants_FullName` ON `Participants` (`FullName`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @idx_exists := (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = DATABASE() AND table_name = 'Participants' AND index_name = 'IX_Participants_IsActive');
SET @sql := IF(@idx_exists = 0, 'CREATE INDEX `IX_Participants_IsActive` ON `Participants` (`IsActive`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
