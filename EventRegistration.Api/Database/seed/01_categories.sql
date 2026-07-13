-- ============================================================
-- 01_categories.sql
--
-- Seed data for the Categories feature.
-- Run after migrations/001_create_categories.sql has been applied.
--
-- Idempotent: INSERT IGNORE skips rows that already exist
-- (Categories.Name is UNIQUE).
-- ============================================================

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
