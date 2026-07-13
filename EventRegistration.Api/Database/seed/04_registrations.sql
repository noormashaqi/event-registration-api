-- ============================================================
-- 04_registrations.sql
--
-- Seed data for the Registrations feature.
-- Depends on: 02_participants.sql, 03_events.sql
-- Run after migrations/004_create_registrations.sql has been applied.
--
-- Idempotent: relies on the UQ_Registrations_EventParticipant
-- unique key for idempotency via INSERT IGNORE.
-- ============================================================

INSERT IGNORE INTO `Registrations` (`EventId`, `ParticipantId`, `Status`, `Notes`, `RegisteredAt`)
SELECT e.Id, p.Id, 1, r.Notes, UTC_TIMESTAMP()
FROM (
    SELECT 'Cloud Native Summit' AS EventName, 'alice.johnson@example.com' AS Email, 'Looking forward to the containers track.' AS Notes
    UNION ALL
    SELECT 'Cloud Native Summit', 'bilal.ahmed@example.com', NULL
    UNION ALL
    SELECT 'Startup Pitch Night', 'carla.mendes@example.com', 'Bringing a +1.'
    UNION ALL
    SELECT 'Intro to Data Science Workshop', 'david.kim@example.com', NULL
    UNION ALL
    SELECT 'Community 5K Run', 'elena.petrova@example.com', 'First 5K!'
) r
JOIN `Events` e ON e.Name = r.EventName
JOIN `Participants` p ON p.Email = r.Email;
