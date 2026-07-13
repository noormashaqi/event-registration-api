-- ============================================================
-- 02_participants.sql
--
-- Seed data for the Participants feature.
-- Run after migrations/003_create_participants.sql has been applied.
--
-- Idempotent: INSERT IGNORE skips rows that already exist
-- (Participants.Email is UNIQUE).
-- ============================================================

INSERT IGNORE INTO `Participants` (`FullName`, `Email`, `Phone`, `DateOfBirth`, `IsActive`, `CreatedAt`)
VALUES
('Alice Johnson', 'alice.johnson@example.com', '+1-555-0101', '1990-04-12', 1, UTC_TIMESTAMP()),
('Bilal Ahmed', 'bilal.ahmed@example.com', '+1-555-0102', '1988-11-02', 1, UTC_TIMESTAMP()),
('Carla Mendes', 'carla.mendes@example.com', '+1-555-0103', '1995-07-23', 1, UTC_TIMESTAMP()),
('David Kim', 'david.kim@example.com', '+1-555-0104', '1992-01-30', 1, UTC_TIMESTAMP()),
('Elena Petrova', 'elena.petrova@example.com', '+1-555-0105', '1985-09-14', 1, UTC_TIMESTAMP()),
('Farah Noor', 'farah.noor@example.com', '+1-555-0106', '1998-03-05', 0, UTC_TIMESTAMP());
