-- ============================================================
-- 03_events.sql
--
-- Seed data for the Events feature.
-- Depends on: 01_categories.sql (Events reference Categories by name)
-- Run after migrations/002_create_events.sql has been applied.
--
-- Idempotent: Events has no natural unique key, so duplicates are
-- prevented with a NOT EXISTS guard on Name instead of INSERT IGNORE.
-- ============================================================

INSERT INTO `Events` (`CategoryId`, `Name`, `Description`, `Location`, `StartAt`, `EndAt`, `RegistrationDeadline`, `Capacity`, `IsActive`, `CreatedAt`)
SELECT c.Id, e.Name, e.Description, e.Location, e.StartAt, e.EndAt, e.RegistrationDeadline, e.Capacity, e.IsActive, UTC_TIMESTAMP()
FROM (
    SELECT
        'Technology' AS CategoryName,
        'Cloud Native Summit' AS Name,
        'A two-day summit on cloud native architecture.' AS Description,
        'Downtown Convention Center' AS Location,
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 30 DAY) AS StartAt,
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 31 DAY) AS EndAt,
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 25 DAY) AS RegistrationDeadline,
        200 AS Capacity,
        1 AS IsActive
    UNION ALL
    SELECT
        'Business',
        'Startup Pitch Night',
        'Local founders pitch to a panel of investors.',
        'Innovation Hub',
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 14 DAY),
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 14 DAY) + INTERVAL 3 HOUR,
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 12 DAY),
        80,
        1
    UNION ALL
    SELECT
        'Education',
        'Intro to Data Science Workshop',
        'Hands-on workshop for beginners.',
        'City Library Hall',
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 20 DAY),
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 20 DAY) + INTERVAL 6 HOUR,
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 18 DAY),
        50,
        1
    UNION ALL
    SELECT
        'Health',
        'Community 5K Run',
        'A fun run supporting local health charities.',
        'Riverside Park',
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 45 DAY),
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 45 DAY) + INTERVAL 3 HOUR,
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 40 DAY),
        500,
        1
    UNION ALL
    SELECT
        'Entertainment',
        'Indie Film Night',
        'Screening of local independent films.',
        'Downtown Cinema',
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 10 DAY),
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 10 DAY) + INTERVAL 4 HOUR,
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 9 DAY),
        120,
        1
) e
JOIN `Categories` c ON c.Name = e.CategoryName
WHERE NOT EXISTS (SELECT 1 FROM `Events` ex WHERE ex.Name = e.Name);
