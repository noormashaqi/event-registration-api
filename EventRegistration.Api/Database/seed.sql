-- ============================================================
-- seed.sql
--
-- Idempotent sample/demo data for the Event Registration API.
-- Run after all migrations (001-004) have been applied, in this
-- order (each section depends on the ones before it):
--   1. Categories
--   2. Participants
--   3. Events        (depends on Categories)
--   4. Registrations (depends on Events, Participants)
--
-- Safe to re-run: Categories/Participants use INSERT IGNORE
-- (unique on Name/Email respectively), Events uses a NOT EXISTS
-- guard on Name (no natural unique key), and Registrations uses
-- INSERT IGNORE (unique on EventId+ParticipantId).
-- ============================================================

-- ----------------------------------------------------------
-- 1. Categories
-- ----------------------------------------------------------
INSERT IGNORE INTO `Categories` (`Name`, `Description`, `IsActive`, `CreatedAt`)
VALUES
('Technology', 'Technology events and conferences', 1, UTC_TIMESTAMP()),
('Business', 'Business and entrepreneurship events', 1, UTC_TIMESTAMP()),
('Education', 'Educational workshops and seminars', 1, UTC_TIMESTAMP()),
('Health', 'Health and wellness events', 1, UTC_TIMESTAMP()),
('Sports', 'Sports competitions and activities', 1, UTC_TIMESTAMP()),
('Entertainment', 'Music, movies and entertainment events', 1, UTC_TIMESTAMP()),
('Science', 'Science fairs and research events', 1, UTC_TIMESTAMP()),
('Art', 'Art exhibitions and creative workshops', 1, UTC_TIMESTAMP()),
('Environment', 'Environmental awareness and sustainability events', 0, UTC_TIMESTAMP()),
('Community', 'Community and volunteer activities', 1, UTC_TIMESTAMP());

-- ----------------------------------------------------------
-- 2. Participants (12)
-- ----------------------------------------------------------
INSERT IGNORE INTO `Participants` (`FullName`, `Email`, `Phone`, `DateOfBirth`, `IsActive`, `CreatedAt`)
VALUES
('Alice Johnson', 'alice.johnson@example.com', '+1-555-0101', '1990-04-12', 1, UTC_TIMESTAMP()),
('Bilal Ahmed', 'bilal.ahmed@example.com', '+1-555-0102', '1988-11-02', 1, UTC_TIMESTAMP()),
('Carla Mendes', 'carla.mendes@example.com', '+1-555-0103', '1995-07-23', 1, UTC_TIMESTAMP()),
('David Kim', 'david.kim@example.com', '+1-555-0104', '1992-01-30', 1, UTC_TIMESTAMP()),
('Elena Petrova', 'elena.petrova@example.com', '+1-555-0105', '1985-09-14', 1, UTC_TIMESTAMP()),
('Farah Noor', 'farah.noor@example.com', '+1-555-0106', '1998-03-05', 0, UTC_TIMESTAMP()),
('Grace Chen', 'grace.chen@example.com', '+1-555-0107', '1993-06-19', 1, UTC_TIMESTAMP()),
('Henry Osei', 'henry.osei@example.com', '+1-555-0108', '1991-12-08', 1, UTC_TIMESTAMP()),
('Isabel Rossi', 'isabel.rossi@example.com', '+1-555-0109', '1997-02-27', 1, UTC_TIMESTAMP()),
('Jack Dubois', 'jack.dubois@example.com', '+1-555-0110', '1989-08-16', 1, UTC_TIMESTAMP()),
('Karim Haddad', 'karim.haddad@example.com', '+1-555-0111', '1994-10-03', 1, UTC_TIMESTAMP()),
('Liu Wei', 'liu.wei@example.com', '+1-555-0112', '1996-05-21', 1, UTC_TIMESTAMP());

-- ----------------------------------------------------------
-- 3. Events (8)
-- ----------------------------------------------------------
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
    UNION ALL
    SELECT
        'Science',
        'AI Research Symposium',
        'Academic and industry talks on recent AI research.',
        'University Auditorium',
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 35 DAY),
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 36 DAY),
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 30 DAY),
        150,
        1
    UNION ALL
    SELECT
        'Art',
        'Modern Art Exhibition Opening',
        'Opening night for the new modern art exhibition.',
        'City Art Gallery',
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 7 DAY),
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 7 DAY) + INTERVAL 3 HOUR,
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 6 DAY),
        90,
        1
    UNION ALL
    SELECT
        'Community',
        'Neighborhood Volunteer Day',
        'A day of community clean-up and volunteering.',
        'Central Community Center',
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 5 DAY),
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 5 DAY) + INTERVAL 5 HOUR,
        DATE_ADD(UTC_TIMESTAMP(), INTERVAL 4 DAY),
        300,
        1
) e
JOIN `Categories` c ON c.Name = e.CategoryName
WHERE NOT EXISTS (SELECT 1 FROM `Events` ex WHERE ex.Name = e.Name);

-- ----------------------------------------------------------
-- 4. Registrations (12 active + 3 cancelled)
-- ----------------------------------------------------------
INSERT IGNORE INTO `Registrations` (`EventId`, `ParticipantId`, `Status`, `Notes`, `RegisteredAt`, `CancelledAt`)
SELECT e.Id, p.Id, r.Status, r.Notes, UTC_TIMESTAMP(), r.CancelledAt
FROM (
    -- Active (1)
    SELECT 'Cloud Native Summit' AS EventName, 'alice.johnson@example.com' AS Email, 1 AS Status, 'Looking forward to the containers track.' AS Notes, NULL AS CancelledAt
    UNION ALL
    SELECT 'Cloud Native Summit', 'bilal.ahmed@example.com', 1, NULL, NULL
    UNION ALL
    SELECT 'Startup Pitch Night', 'carla.mendes@example.com', 1, 'Bringing a +1.', NULL
    UNION ALL
    SELECT 'Intro to Data Science Workshop', 'david.kim@example.com', 1, NULL, NULL
    UNION ALL
    SELECT 'Community 5K Run', 'elena.petrova@example.com', 1, 'First 5K!', NULL
    UNION ALL
    SELECT 'Indie Film Night', 'grace.chen@example.com', 1, NULL, NULL
    UNION ALL
    SELECT 'AI Research Symposium', 'henry.osei@example.com', 1, 'Excited for the keynote.', NULL
    UNION ALL
    SELECT 'Modern Art Exhibition Opening', 'isabel.rossi@example.com', 1, NULL, NULL
    UNION ALL
    SELECT 'Neighborhood Volunteer Day', 'jack.dubois@example.com', 1, 'Signing up with two friends.', NULL
    UNION ALL
    SELECT 'Cloud Native Summit', 'karim.haddad@example.com', 1, NULL, NULL
    UNION ALL
    SELECT 'Startup Pitch Night', 'liu.wei@example.com', 1, NULL, NULL
    UNION ALL
    SELECT 'Community 5K Run', 'alice.johnson@example.com', 1, 'Running with a friend this time.', NULL
    -- Cancelled (2)
    UNION ALL
    SELECT 'Indie Film Night', 'bilal.ahmed@example.com', 2, 'Schedule conflict.', UTC_TIMESTAMP()
    UNION ALL
    SELECT 'Intro to Data Science Workshop', 'carla.mendes@example.com', 2, NULL, UTC_TIMESTAMP()
    UNION ALL
    SELECT 'AI Research Symposium', 'david.kim@example.com', 2, 'No longer available that day.', UTC_TIMESTAMP()
) r
JOIN `Events` e ON e.Name = r.EventName
JOIN `Participants` p ON p.Email = r.Email;
