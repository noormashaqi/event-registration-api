-- `Status` was declared TINYINT(1), which MySqlConnector (the driver this
-- API uses) auto-converts to a C# `bool` by default, since display width 1
-- is its heuristic for "this is a boolean column". But `Status` is not a
-- boolean -- it's an enum-like value (1 = Active, 2 = Cancelled), so any
-- code that reads it back via a dynamic Dapper result and treats it as an
-- int (every read site in Features/Registrations/*.cs) either throws
-- (`(int)` cast on a bool fails at runtime) or silently misreads Cancelled
-- (2) registrations as Active, because both 1 and 2 collapse to boolean
-- `true`.
--
-- Dropping the `(1)` display width to plain TINYINT stops MySqlConnector
-- from treating the column as boolean (it maps to `sbyte`, which converts
-- to `int` correctly), without touching any of the genuinely-boolean
-- TINYINT(1) `IsActive` columns elsewhere (Categories, Events, Participants
-- all rely on the TINYINT(1) -> bool mapping and are unaffected by this).
ALTER TABLE `Registrations`
    MODIFY COLUMN `Status` TINYINT NOT NULL DEFAULT 1;
