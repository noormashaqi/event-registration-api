CREATE TABLE IF NOT EXISTS `Registrations` (
    `Id`              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `EventId`         BIGINT UNSIGNED NOT NULL,
    `ParticipantId`   BIGINT UNSIGNED NOT NULL,
    `Status`          TINYINT(1) NOT NULL DEFAULT 1,
    `Notes`           VARCHAR(500) NULL,
    `RegisteredAt`    DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    `CancelledAt`     DATETIME NULL,
    PRIMARY KEY (`Id`),
    
    CONSTRAINT `FK_Registrations_Events` FOREIGN KEY (`EventId`) 
        REFERENCES `Events` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    
    CONSTRAINT `FK_Registrations_Participants` FOREIGN KEY (`ParticipantId`) 
        REFERENCES `Participants` (`Id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    
    UNIQUE KEY `UQ_Registrations_EventParticipant` (`EventId`, `ParticipantId`)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci;

-- CREATE INDEX has no IF NOT EXISTS clause on older MySQL 8.x, so guard
-- each one manually via information_schema to keep this migration
-- safe to re-run.
SET @idx_exists := (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = DATABASE() AND table_name = 'Registrations' AND index_name = 'IX_Registrations_EventId');
SET @sql := IF(@idx_exists = 0, 'CREATE INDEX `IX_Registrations_EventId` ON `Registrations` (`EventId`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @idx_exists := (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = DATABASE() AND table_name = 'Registrations' AND index_name = 'IX_Registrations_ParticipantId');
SET @sql := IF(@idx_exists = 0, 'CREATE INDEX `IX_Registrations_ParticipantId` ON `Registrations` (`ParticipantId`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @idx_exists := (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = DATABASE() AND table_name = 'Registrations' AND index_name = 'IX_Registrations_Status');
SET @sql := IF(@idx_exists = 0, 'CREATE INDEX `IX_Registrations_Status` ON `Registrations` (`Status`)', 'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;