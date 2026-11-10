# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

.NET 10 solution (`Shop.sln`). No test projects, linter config, or README exist yet.

```
dotnet build Shop.sln
dotnet run --project Shop.Api --launch-profile https   # https://localhost:7024, http://localhost:5013
```

EF Core migrations live in `Shop.Infrastructure/Persistence/PostgreSql/EfCore/Migrations`; the design-time startup project is the API:

```
dotnet ef migrations add <Name> --project Shop.Infrastructure --startup-project Shop.Api --output-dir Persistence/PostgreSql/EfCore/Migrations
dotnet ef database update --project Shop.Infrastructure --startup-project Shop.Api
```

In Development, API docs are served by Scalar and Swagger UI (see `WebApplicationExtensions.UseApiDocumentation`).

## External services

The API needs all of these at startup (config in `Shop.Api/appsettings.Development.json`, which holds dev credentials and the JWT key):

- **PostgreSQL** (`ConnectionStrings:Postgres`) — primary store via EF Core/Npgsql.
- **MongoDB** (`Mongo:*`) — exception logs only.
- **Redis** (`Redis:*`) — distributed cache behind `ICacheService`.
- **`Jwt:*`** — validated on start (`ValidateOnStart`); `Key` must be ≥ 32 chars.

`Program.cs` runs `IdentityDataSeeder` on every boot, which upserts the roles in `Roles.SystemRoles` and permissions in `Permissions.SystemPermissions`. To add a role or permission, add the constant **and** append it to the corresponding `System*` list or it will never be seeded.

## Architecture

Clean-architecture layering; dependencies point inward: `Api → Application, Infrastructure`; `Infrastructure → Application, Domain`; `Application → Domain`. Each layer exposes one `DependencyInjection.cs` (`AddApi`, `AddApplication`, `AddInfrastructure`) called from `Program.cs`.

- **Domain** — entities, value objects (`Money`, `AddressValueObject`), constants (`Roles`, `Permissions`), and domain exceptions.
- **Application** — services (`ProductService`, `CategoryService`, `IAuthService`/`AuthService`, `IBasketService`), DTOs, and the ports Infrastructure implements (`I*Repository`, `ICacheService`, `ITokenService`, `IPasswordHasher`, `IRefreshTokenService`, `IUserPermissionService`). `ProductCache` wraps `ICacheService` with get-or-set and explicit invalidation; `ProductService` reads through it and must invalidate on writes. JWT claim names/values are in `Application/Authentication/Claims`.
- **Infrastructure** — adapters for the ports above (`Services/` holds the JWT, password-hasher, refresh-token and permission services; `Authentication/JwtSettings.cs` is the bound options class).
- **Api** — versioned controllers, request/response contracts, FluentValidation validators, mappers, middleware.

### Domain rules worth knowing

- Entities are rich: private setters, invariants enforced in methods that throw domain exceptions. DB generates all `long` ids (never pass ids into constructors).
- The shop has a single currency (`ShopConstants.Currency`); `Product` rejects prices in any other.
- Stock lives on `Product.StockQuantity` (`Product.Version` is a Postgres `xmin` concurrency token). Stock is decreased by `Order.Place(...)` at checkout and restored by `Order.Cancel(...)`; `Basket` checks stock but never reserves it. `OrderItem` snapshots product name and price.
- `Basket` is its own aggregate (one per user, `IBasketRepository`), not reached through `User`.
- Order flow: `Pending → Paid → Shipped → Completed`; cancel only from Pending/Paid. `Payment` is a separate entity created via `Order.CreatePayment`, and `Order.MarkPaid(payment)` validates it. The API contract still calls stock `Count`.

### Persistence is polyglot

- **EF Core + Npgsql** (`ShopDbContext`, per-entity `Configurations/`, `Repositories/`) backs every `I*Repository` except exception logs.
- **MongoDB** backs `IExceptionLogRepository` only (`Persistence/Mongo`).
- **Dapper** (`DapperProductRepository`) is registered under its *concrete type*, not `IProductRepository`, so it does not replace the EF repository. It uses `SqlConnectionFactory` — SQL Server (`Microsoft.Data.SqlClient`) with the `ConnectionStrings:Default` string — i.e. a different database engine than the Postgres EF path.

### API conventions

- Controllers inherit `BaseController` (`[ApiController]`, `[Authorize]` by default, route `api/v{version}`). Versioning is URL-segment (`[ApiVersion(1.0)]`; V2 products live in `Controllers/V2` with `Contracts/V2` and `Mappers/V2`). Route templates are constants in `Shop.Api/Constants/*UriConstants.cs`, not inline strings.
- Actions return `ApiResult` / `ApiResult<T>` (envelope: `Data`, `Success`, `Status`, `Error`; implicit conversion from `T`). `ApiResultFilter` wraps any plain `ObjectResult`/`NotFound`/`NoContent` the same way and skips OData requests. `ApiResultMiddleware` exists but is **not** registered in `Program.cs`.
- Errors: throw domain exceptions (e.g. `EntityNotFoundException`, `DuplicateEmailException`); `ExceptionHandlingMiddleware` maps them to status codes (anything unmapped → 500 with a masked message) and persists each to Mongo. New exception types need an entry in `ResolveStatusCode`.
- Sentry: `ExceptionHandlingMiddleware` reports only 500s via `SentrySdk.CaptureException` (it swallows exceptions, so Sentry's own pipeline never sees them). DSN is `Sentry:Dsn` in `appsettings.json` (empty = disabled). See `docs/sentry.md`.
- Logging: Serilog (`UseSerilog` in `Program.cs`, config in the `Serilog` section of `appsettings.json`; console + daily files under `logs/`). See `docs/serilog.md`.
- Telemetry: OpenTelemetry traces and metrics are registered by `AddOpenTelemetryExport`, and logs are exported by the Serilog OTLP sink in `WriteToOpenTelemetry` (both in `Shop.Api/Telemetry`, config in the `OpenTelemetry` section; a blank `Otlp:Endpoint` turns export off). `UseSerilog` replaces the logging providers, so OpenTelemetry's own log provider would receive nothing. Custom spans and metrics live in `Shop.Application/Telemetry`, which uses only .NET types. A new `ActivitySource` or `Meter` name must be added to both `AddSource` and `AddMeter` or its data is silently dropped. Metric tags must stay low-cardinality: ids go on spans, not metrics. See `docs/opentelemetry.md`.
- Health probes: anonymous `GET /health/live` (no dependency checks) and `GET /health/ready` (PostgreSQL, Redis, MongoDB, OTLP collector), registered in `Shop.Api/HealthChecks` (`AddShopHealthChecks`, `MapHealthProbes`); routes are in `HealthUriConstants`. A new readiness check must carry the `ready` tag or `/health/ready` ignores it. The OTLP collector check is registered as `Degraded` on failure (still HTTP 200) because telemetry export is background work.
- Validation: FluentValidators in `Validators/Validations/<Area>` (auto-registered from the Api assembly) run via `ValidationFilter`; message strings are in `Validators/Messages`.
- DTO ↔ contract conversion uses extension-method mappers in `Mappers/` (`ToDto()`, `ToResponse()`).
- OData: `ODataProductsController` is commented out ("temporarily disabled"), though `Microsoft.AspNetCore.OData` and `OData/ODataEdmModelBuilder.cs` remain.

### Authentication & authorization

JWT bearer with `MapInboundClaims = false` (claims keep their raw names). Two mechanisms coexist in `Shop.Api/Authentication`:

- Role/claim policies (`CustomerAccess` → `Roles.User`, `ActiveUser` → account-status claim).
- Permission policies (`CanReadProducts`, `CanManageProducts`, `CanManageCategories`) using `PermissionRequirement` + `PermissionAuthorizationHandler`, which loads the user's permissions from `IUserPermissionService` on each check (permissions are *not* in the token). Roles grant permissions via `RolePermission`; `UserPermission` adds per-user grants.

Policy names live in `PolicyNames`; add new policies in `AuthorizationPolicies.AddPolicies`.
