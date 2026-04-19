#Aether Backend: Implementation Plan
##1. Architectural Overview (Onion Architecture)
The project is divided into four concentric layers to isolate business logic from external concerns (Database, Frameworks, UI).
* Aether.Domain: Core entities (User, Asset), Enums, and Repository Interfaces. No dependencies.
* Aether.Application: Use Cases (Services), DTOs (Data Transfer Objects), and FluentValidation.
* Aether.Infrastructure: EF Core implementation, Migrations, JWT Service, and external API clients (SELIC/Market Data).
* Aether.Api: Controllers, Middlewares (Global Exception Handling), and Dependency Injection wiring.

##2. Core Domain & Persistence
To support the frontend's 5 asset types, the database must strictly enforce these categories.
Database Schema (PostgreSQL)
Table	Key Fields	Purpose
Users	Id, Email, PasswordHash, CreatedAt	Identity and Auth.
Assets	Id, UserId, Name, Value, Type (Enum)	User portfolio tracking.
AssetHistory	Id, AssetId, Value, Timestamp	Data for the frontend's 24h change charts.
Note on Enums: We will use HasConversion<string>() in EF Core for the AssetType enum. This ensures the database stores "crypto" or "stock" as strings, matching the frontend's JSON expectation exactly.

##3. Authentication Flow
We will implement stateless JWT Authentication to handle the secure communication expected by the AuthRepository in Flutter.
* Algorithm: HMAC SHA256.
* Storage: The backend issues the token; the frontend stores it in flutter_secure_storage.
* Endpoints:
    * POST /auth/register (BCrypt/Argon2 password hashing).
    * POST /auth/login (Returns JWT + UserDto).
    * GET /auth/me (Validates token and returns current user context).

##4. Frontend-Backend Contract Alignment
To ensure the Flutter AppException hierarchy works, the API will use a standardized error response.
Global Error Middleware Output:
JSON
```
{
  "error": "Unauthorized",
  "message": "The provided token is expired or invalid."
}
```
##5. Development Roadmap

1:
Domain Entities & Enums
Foundation Layer
Define the Asset entity with the 5 types: crypto, stock, inventory, collectible, cash. Define IAssetRepository and IUserRepository interfaces.

2:
Identity & Infrastructure
Auth & Database
Configure EF Core with PostgreSQL. Implement the JWT Service and Identity controllers. Set up the Global Exception Middleware.

3:
Asset Management API
Feature Layer
Implement GetAssetsQuery and CreateAssetCommand using the Repository pattern. Ensure UserId is extracted from the JWT Claims to isolate data.

4:
Market & Discovery
Data Enrichment
Implement a stubbed GET /market/assets service. Future iteration: Integrate a worker service to fetch real-time SELIC and stock prices.


##6. Infrastructure & Hosting
Since you are running a Debian Homelab, the backend will be containerized.
* Docker: Dockerfile (Multi-stage build for .NET).
* Portainer: Deploy via a Stack (docker-compose) including the .NET API and a PostgreSQL container.
* Local AI Integration: The API can eventually call your local Ollama instance to provide "Wealth Insights" or portfolio summaries directly to the Flutter app.
