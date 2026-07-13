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

CREATE INDEX `IX_Events_CategoryId` ON `Events` (`CategoryId`);
CREATE INDEX `IX_Events_IsActive` ON `Events` (`IsActive`);
CREATE INDEX `IX_Events_StartAt` ON `Events` (`StartAt`);
