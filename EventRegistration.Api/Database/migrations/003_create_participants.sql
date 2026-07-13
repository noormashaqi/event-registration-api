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
