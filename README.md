# Population Service

A .NET 8 Web API that aggregates country population data from two sources — a local SQLite database and an 'external' Country Stats Service — and exposes the result through a single REST endpoint.

## How to Run

Prerequisites: .NET 8 SDK, Visual Studio 2022+

```bash
dotnet run --project PopulationService
```

The API will be available at `https://localhost:{port}/api/populations`.
Swagger UI is available at `https://localhost:{port}/swagger` in development.

```bash
dotnet test
```

---

## API

There is one endpoint:

`GET /api/populations` — returns all countries with their aggregated population, sorted alphabetically.

```json
[
  { "countryName": "Canada", "population": 37000000 },
  { "countryName": "Germany", "population": 1500000 }
]
```

---

## Architecture

Single Web API project with folder-based layering:

```
PopulationService/
├── Controllers/        - HTTP layer (PopulationsController)
├── Services/           - Business logic (PopulationAggregationService)
├── Data/               - EF Core DbContext, repository, entities
├── ExternalServices/   - CountryStatsService (hard-coded stub, real API in production)
├── Interfaces/         - Contracts for DI and testability
└── Models/             - CountryPopulation record (shared model)
```

Why a single project? One entity, one endpoint — splitting into Core/Infrastructure/Api would be over-engineering here. The folder structure maps 1:1 to a multi-project layout, so that migration is trivial if needed.

If the domain grows significantly (multiple aggregates, complex business rules, cross-cutting concerns), Clean Architecture would be a natural next step — separating the solution into `Domain`, `Application`, `Infrastructure`, and `Presentation` layers with strict inward-only dependency rules.

Why Web API over a console app? Easier to demo, Swagger gives free documentation, and it's the right shape for a data-serving solution.

Why EF Core over raw ADO.NET? LINQ, async support, proper entity mapping, and easy testability using in-memory SQLite. The original `SqliteDbManager` with raw SQL was replaced entirely.

---

## Key Design Decisions

- DB wins on duplicates — when both sources return data for the same country, the database value is used, per the requirement. Matching is case-insensitive so `"usa"` and `"USA"` are treated as the same country.
- Both data sources are fetched concurrently with `Task.WhenAll`. This matters when `CountryStatsService` becomes a real HTTP call.
- `City.Population` is nullable in the schema; null is coalesced to 0 during aggregation so a missing value doesn't break the sum.
- No DTOs — `CountryPopulation` is a flat immutable record that is already the correct API shape. A separate response DTO would be identical with no benefit.
- Error handling uses .NET 8's built-in `AddProblemDetails()` + `UseExceptionHandler()`, so unhandled exceptions return a consistent JSON error shape with no custom middleware needed.

---

## Testing

- Unit tests cover `PopulationAggregationService` and `PopulationsController` in isolation using Moq, with no infrastructure involved.
- Integration tests cover `PopulationRepository` against a real in-memory SQLite database, and `PopulationsController` end-to-end using `WebApplicationFactory` with a seeded in-memory SQLite database replacing the real one.

---

## If This Project Grew

- `ICountryStatsService` is already an interface. An `HttpCountryStatsService` using `IHttpClientFactory` with Polly retry policies is a zero-effort addition when the stub needs to become a real HTTP call.
- Swapping SQLite for PostgreSQL or SQL Server only requires changing the connection string and EF Core provider — the Repository pattern (`ICountryPopulationRepository`) keeps the rest of the application completely unaware of what database sits behind it.
- The folder structure maps directly to separate Core/Infrastructure/Api projects for compiler-enforced layer boundaries when the codebase grows.
- A `Dockerfile` and `docker-compose.yml` would let the API and a real database run together with a single `docker-compose up`.
- Authentication, rate limiting, and health checks would be the next production additions.
- The built-in `ILogger<T>` is already wired up across the service, repository, and controller. In production, swap the default provider for a structured logging library as Serilog is the most common choice.
- Integration tests could be moved to Testcontainers to run against the same database engine used in production instead of in-memory SQLite.

