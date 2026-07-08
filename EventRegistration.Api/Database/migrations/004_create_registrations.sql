DROP TABLE IF EXISTS `Registrations`;

CREATE TABLE IF NOT EXISTS `Registrations` (
    `Id`              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `EventId`         BIGINT UNSIGNED NOT NULL,
    `ParticipantId`   BIGINT NOT NULL,
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

CREATE INDEX `IX_Registrations_EventId` ON `Registrations` (`EventId`);
CREATE INDEX `IX_Registrations_ParticipantId` ON `Registrations` (`ParticipantId`);
CREATE INDEX `IX_Registrations_Status` ON `Registrations` (`Status`);