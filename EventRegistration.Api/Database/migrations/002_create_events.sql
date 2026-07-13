CREATE TABLE IF NOT EXISTS `Events` (
    `Id`                   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `CategoryId`           BIGINT UNSIGNED NOT NULL,
    `Name`                 VARCHAR(150) NOT NULL,
    `Description`          VARCHAR(1000) NULL,
    `Location`             VARCHAR(200) NOT NULL,
    `StartAt`              DATETIME NOT NULL,
    `EndAt`                DATETIME NOT NULL,
    `RegistrationDeadline` DATETIME NOT NULL,
    `Capacity`             INT NOT NULL,
    `IsActive`             TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt`            DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    `UpdatedAt`            DATETIME NULL,
    PRIMARY KEY (`Id`),
    
    CONSTRAINT `FK_Events_Categories` FOREIGN KEY (`CategoryId`) 
        REFERENCES `Categories` (`Id`) ON DELETE RESTRICT
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci;

-- CREATE INDEX has no IF NOT EXISTS clause on older MySQL 8.x, so guard
-- each one manually via information_schema to keep this migration
-- safe to re-run.
SET @idx_exists := (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = DATABASE() AND table_name = 'Events' AND index_name = 'IX_Events_CategoryId');
SET @sql := IF(@idx_exists = 0, 'CREATE INDEX `IX_Events_CategoryId` ON `Events` (`CategoryId`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @idx_exists := (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = DATABASE() AND table_name = 'Events' AND index_name = 'IX_Events_IsActive');
SET @sql := IF(@idx_exists = 0, 'CREATE INDEX `IX_Events_IsActive` ON `Events` (`IsActive`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @idx_exists := (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = DATABASE() AND table_name = 'Events' AND index_name = 'IX_Events_StartAt');
SET @sql := IF(@idx_exists = 0, 'CREATE INDEX `IX_Events_StartAt` ON `Events` (`StartAt`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
