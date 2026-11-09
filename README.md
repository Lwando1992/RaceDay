# RaceDay - ASP.NET Core Web API + MVC Client

A race-day management system with an ASP.NET Core Web API (organisers + participants),
a shared DTO library, an xUnit test suite, and an MVC .NET client that consumes the API.
The API ships with Swagger UI and Azure Blob Storage integration for profile images.

## Solution layout

```
RaceDay.sln
|-- RaceDay.Contracts/        # Shared DTOs (User, Event, Category, Enrolment, Result)
|-- RaceDay.Api/              # ASP.NET Core Web API
|   |-- Controllers/          # Auth, Users, Events, Categories, Enrolments, Results
|   |-- Models/               # EF Core entities
|   |-- Data/                 # ApplicationDbContext
|   |-- Storage/              # Azure Blob Storage service + local fallback
|   |-- Extensions/           # Mapping, JWT helper extensions
|   |-- Migrations/           # SQL schema (schema is also created automatically)
|   |-- Program.cs            # Host builder wiring Startup.cs
|   `-- Startup.cs            # DI, EF Core, JWT auth, Swagger, CORS
|-- RaceDay.Api.Tests/        # xUnit controller tests (in-memory EF Core)
`-- RaceDay.Web/              # MVC client consuming the API via a typed HttpClient
```

## Running locally (no Docker)

```bash
cd RaceDay.Api && dotnet run     # API + Swagger on http://localhost:5000/swagger
cd RaceDay.Web && dotnet run     # MVC client on http://localhost:5001
```

Seeded demo organiser login: `organiser@raceday.local` / `Organiser#1`

## Running with Docker Compose

```bash
docker compose up --build
# API:      http://localhost:5000  (Swagger at /swagger)
# MVC site: http://localhost:5001
```

## Azure Blob Storage

Set `AzureBlob:ConnectionString` (e.g. via environment variable
`AzureBlob__ConnectionString`) to enable `AzureBlobStorageService`. When unset, a
local-disk fallback (`LocalFileStorageService`, files under `wwwroot/uploads`) is used,
so the app runs anywhere without cloud credentials.

Endpoint: `POST /api/users/profile/image` (multipart `file`) stores the picture in the
`raceday-uploads` container and returns the updated profile with the blob URL.

## API endpoints

| Method | Route | Auth |
|--------|-------|------|
| POST | /api/auth/register | anonymous |
| POST | /api/auth/login | anonymous |
| GET | /api/auth/logout | any user |
| GET/PUT | /api/users/profile | any user |
| POST | /api/users/profile/image | any user |
| GET | /api/events, /api/events/{id} | anonymous |
| POST/PUT/DELETE | /api/events... | Organiser |
| GET | /api/categories | anonymous |
| POST | /api/events/{eventId}/categories | Organiser |
| PUT/DELETE | /api/categories/{id} | Organiser |
| POST | /api/enrolments | Participant |
| GET | /api/enrolments | any user (own) |
| GET | /api/events/{eventId}/enrolments | Organiser (own events) |
| POST | /api/results | Organiser (own events) |
| GET | /api/results | any user (own) |

All authenticated routes use JWT bearer tokens (`Authorization: Bearer <token>`),
obtainable from `POST /api/auth/login`; Swagger UI has an "Authorize" button.

## Tests

```bash
dotnet test RaceDay.Api.Tests
```
