# Event Registration API

A REST API for managing events, categories, participants, and event registrations, built with ASP.NET Core, MediatR, Dapper, and MySQL.

## Tech stack

- ASP.NET Core (.NET 10)
- MediatR (CQRS-style request/handler pipeline, with a FluentValidation pipeline behavior)
- Dapper + MySqlConnector (data access)
- FluentValidation (request validation)
- Serilog (structured logging, console sink)
- Swashbuckle / Swagger (API docs, Development environment only)
- DotNetEnv (loads `.env` into process environment variables on startup)

## Project layout

```
EventRegistration.Api/
  Controllers/          API controllers
  Features/              MediatR commands/queries, grouped by resource
  Database/
    migrations/          Numbered, ordered schema migrations (001-004)
    seed.sql              Idempotent sample/demo data (categories, participants, events, registrations)
  Exceptions/             Shared exception types mapped to HTTP status codes
  Middleware/             Global exception-handling middleware
  Common/                 Shared API response models
```

## Prerequisites

- .NET 10 SDK
- MySQL 8.x (or compatible) server

## Setup

1. Copy the environment template and fill in your local database credentials:

   ```bash
   cd EventRegistration.Api
   cp .env.example .env
   ```

   Edit `.env`:

   ```
   DB_CONNECTION_STRING=server=localhost;port=3306;database=EventRegistration;user=root;password=your_password;
   CORS_ALLOWED_ORIGIN=http://localhost:5173
   ASPNETCORE_ENVIRONMENT=Development
   ```

   `.env` is git-ignored and never committed — do not put real credentials in `appsettings.json` or `appsettings.Development.json`.

2. Create the database, apply migrations, and load sample seed data. Either run the PowerShell helper (reads connection details from `.env` automatically, so nothing is hardcoded):

   ```powershell
   cd EventRegistration.Api/Database
   ./apply-db.ps1 -CreateDatabase      # migrations + seed
   ./apply-db.ps1 -CreateDatabase -SkipSeed   # migrations only, no sample data
   ```

   ...or run each file by hand, in order (migrations first, since events reference categories and registrations reference both events and participants):

   ```bash
   mysql -u root -p -e "CREATE DATABASE EventRegistration;"
   mysql -u root -p EventRegistration < EventRegistration.Api/Database/migrations/001_create_categories.sql
   mysql -u root -p EventRegistration < EventRegistration.Api/Database/migrations/002_create_events.sql
   mysql -u root -p EventRegistration < EventRegistration.Api/Database/migrations/003_create_participants.sql
   mysql -u root -p EventRegistration < EventRegistration.Api/Database/migrations/004_create_registrations.sql

   mysql -u root -p EventRegistration < EventRegistration.Api/Database/seed.sql
   ```

   Every migration and the seed file are idempotent and safe to re-run.

3. Restore and run:

   ```bash
   cd EventRegistration.Api
   dotnet restore
   dotnet run
   ```

   The API listens on `http://localhost:5080` by default (see `Properties/launchSettings.json`). Swagger UI is available at `/swagger` in the Development environment.

## Configuration

Configuration is layered: `appsettings.json` → `appsettings.Development.json` → environment variables loaded from `.env` at startup. `DB_CONNECTION_STRING` overrides `ConnectionStrings:Default` if set.

## API overview

| Resource | Routes |
|---|---|
| Categories | `GET/POST /api/categories`, `GET/PUT/DELETE /api/categories/{id}` |
| Events | `GET/POST /api/events`, `GET/PUT/DELETE /api/events/{id}` |
| Participants | `GET/POST /api/participants`, `GET/PUT/DELETE /api/participants/{id}` |
| Registrations | `GET/POST /api/events/{eventId}/registrations`, `GET /api/registrations/{id}`, `PATCH /api/registrations/{id}/cancel` |
| Dashboard | `GET /api/dashboard/summary` |

Registrations are listed/created in the context of their event (`/api/events/{eventId}/registrations`), but individually addressed by their own ID (`/api/registrations/{id}`), since a registration ID is globally unique and doesn't require the event ID to resolve.

## Error responses

Unhandled and business-rule exceptions are converted to a consistent JSON error shape by `ExceptionHandlingMiddleware`:

```json
{
  "success": false,
  "timestamp": "2026-07-14T00:00:00Z",
  "message": "...",
  "errors": []
}
```

| Exception | HTTP status |
|---|---|
| `ValidationException` | 400 |
| `NotFoundException` | 404 |
| `DuplicateResourceException` | 409 |
| Other `BusinessException` subclasses (e.g. `CategoryInUseException`, `ParticipantHasRegistrationsException`) | 409 |
| Anything else | 500 |

## Business rules of note

- A category cannot be deleted while it's referenced by any event.
- A participant cannot be deleted while they have registration history (active or cancelled).
- An event's capacity cannot be reduced below its current number of active registrations.
- Registering for an event is blocked if the event is inactive, past its registration deadline, already started, full, or the participant is inactive. Re-registering after a cancellation reactivates the existing registration row instead of creating a duplicate.
- A registration cannot be cancelled after its event has started.
