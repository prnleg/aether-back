# Aether — Portfolio Tracker API

ASP.NET Core 8 REST API for tracking multi-asset portfolios: crypto, Steam skins, and physical collectibles.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 / ASP.NET Core |
| ORM | Entity Framework Core 8 |
| Database | SQLite (dev) → PostgreSQL (prod) |
| Auth | ASP.NET Core Identity + JWT (HMAC SHA256) |
| Validation | FluentValidation 11 |

---

## Architecture

Onion Architecture with four projects:

```
Aether.Domain          → Entities, value objects, specs, domain events, interfaces
Aether.Application     → Feature slices: DTOs, validators, service interfaces
Aether.Infrastructure  → EF Core, repositories, services, background workers
Aether.API             → Controllers, middleware, DI wiring
```

### Architectural Patterns

| Pattern | Where | Purpose |
|---|---|---|
| **Result Pattern** | `Domain/Common/Result.cs` | No exception-throwing for logic errors. All service methods return `Result<T>`. |
| **Money Value Object** | `Domain/ValueObjects/Money.cs` | Amount + Currency together. Currency-safe `+`, `-`, `Multiply()` operators. |
| **Specification Pattern** | `Domain/Specifications/` | Reusable query predicates with EF Core includes. `SpecificationEvaluator<T>` translates to LINQ. |
| **Outbox Pattern** | `OutboxMessage` entity + `OutboxWorker` | Domain events saved atomically with data. Background worker dispatches them asynchronously. |
| **Vertical Slices** | `Application/Features/{Feature}/` | All code for one feature in one folder — DTOs, validators, service interface. |

---

## Domain Model

```
Portfolio (1) ──── (*) Asset (abstract, TPH discriminator)
                        ├── CryptoAsset    (symbol, quantity)
                        ├── SteamSkinAsset (marketHashName, appId)
                        └── PhysicalAsset  (category, brand, condition)
```

`Asset` owns two `Money` value objects: `AcquisitionPrice` and `CurrentFloorPrice`.

`Portfolio` raises domain events on state changes:
- `PortfolioCreatedEvent`
- `AssetAddedEvent`
- `AssetRemovedEvent`

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Setup

```bash
# Clone and restore
git clone <repo-url>
cd aether-back
dotnet restore

# Apply migrations
dotnet ef database update --project Aether.Infrastructure --startup-project Aether.API

# Run
dotnet run --project Aether.API
```

Swagger UI: `https://localhost:<port>/swagger`

### Configuration

`Aether.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=aether.db"
  },
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "Issuer": "AetherAPI",
    "Audience": "AetherApp",
    "ExpiryMinutes": "1440"
  }
}
```

> **Production**: change `Jwt:Key`, switch to PostgreSQL connection string, use environment variables or secrets manager.

---

## API Reference

### Auth

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | — | Register. Creates default portfolio. Returns JWT. |
| `POST` | `/api/auth/login` | — | Login. Returns JWT + portfolioId. |
| `GET` | `/api/auth/me` | Bearer | Current user info. |

**Register / Login body:**
```json
{
  "email": "user@example.com",
  "password": "secret"
}
```

**Response:**
```json
{
  "token": "eyJ...",
  "userId": "guid",
  "email": "user@example.com",
  "portfolioId": "guid"
}
```

---

### Users

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `PATCH` | `/api/users/me` | Bearer | Update name and email. |

---

### Portfolio

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/portfolio` | Bearer | List all portfolios for current user. |
| `GET` | `/api/portfolio/{id}` | Bearer | Get portfolio by ID. |
| `POST` | `/api/portfolio` | Bearer | Create portfolio. |
| `POST` | `/api/portfolio/{id}/assets/crypto` | Bearer | Add crypto asset. |
| `POST` | `/api/portfolio/{id}/assets/steam` | Bearer | Add Steam skin. |
| `POST` | `/api/portfolio/{id}/assets/physical` | Bearer | Add physical asset. |
| `DELETE` | `/api/portfolio/{id}/assets/{assetId}` | Bearer | Remove asset. |

**Add Crypto body:**
```json
{
  "name": "Bitcoin",
  "symbol": "BTC",
  "quantity": 0.5,
  "acquisitionPrice": 45000,
  "currency": "USD"
}
```

**Add Steam Skin body:**
```json
{
  "name": "AK-47 | Redline",
  "marketHashName": "AK-47 | Redline (Field-Tested)",
  "acquisitionPrice": 12.50,
  "currency": "USD"
}
```

**Add Physical Asset body:**
```json
{
  "name": "Jordan 1 Retro High OG",
  "category": "Sneakers",
  "brand": "Nike",
  "condition": "DS",
  "acquisitionPrice": 170,
  "currency": "USD"
}
```

---

## Error Contract

All errors follow this shape:

```json
{
  "error": "ErrorCode",
  "message": "Human-readable description."
}
```

| HTTP | Error Code | Cause |
|---|---|---|
| 400 | `BadRequest` | Invalid input |
| 401 | `Unauthorized` | Invalid credentials or missing token |
| 404 | `NotFound` | Resource not found |
| 409 | `Conflict` | Duplicate (e.g. email already registered) |
| 422 | `ValidationError` | FluentValidation failure |
| 500 | `InternalServerError` | Unhandled exception |

---

## Database

```bash
# New migration
dotnet ef migrations add <MigrationName> --project Aether.Infrastructure --startup-project Aether.API

# Apply
dotnet ef database update --project Aether.Infrastructure --startup-project Aether.API
```

Migrations live in `Aether.Infrastructure/Migrations/`.

---

## Project Structure

```
aether-back/
├── Aether.Domain/
│   ├── Common/              # Result<T>, Error, IDomainEvent, IHasDomainEvents
│   ├── Entities/            # Portfolio, Asset subtypes, OutboxMessage
│   ├── Events/              # PortfolioCreatedEvent, AssetAddedEvent, AssetRemovedEvent
│   ├── Interfaces/          # IPortfolioRepository
│   ├── Specifications/      # ISpecification<T>, BaseSpecification<T>, concrete specs
│   └── ValueObjects/        # Money
│
├── Aether.Application/
│   └── Features/
│       ├── Auth/            # AuthRequest, AuthResponse, UserDto, validators
│       ├── Portfolio/       # PortfolioDto, AssetDto, requests, IPortfolioService, validators
│       └── Users/           # UpdateUserRequest
│
├── Aether.Infrastructure/
│   ├── BackgroundServices/  # OutboxWorker
│   ├── Migrations/
│   ├── Persistence/         # AetherDbContext (intercepts domain events → OutboxMessages)
│   ├── Repositories/        # PortfolioRepository (spec-based)
│   ├── Services/            # PortfolioService, JwtService
│   └── Specifications/      # SpecificationEvaluator<T>
│
└── Aether.API/
    ├── Common/              # ResultExtensions (Result<T> → IActionResult)
    ├── Controllers/         # Auth, Portfolio, Users
    └── Middleware/          # GlobalExceptionMiddleware
```
