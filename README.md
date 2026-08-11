# C# Accepted Assessment app

An application for C# (.net) knowledge assessment

## Description

This is a web application that interacts with a 3nd party service (<https://fakeapi.platzi.com>/<https://api.escuelajs.co>) and serves data

We have to do some code refactoring and implement some new features

## Code refactoring

Seems that the use of http client is not so much efficient

Let's make a different, more solid, approach/implementation

## New features

**#1**

Right now only the **getAll** method supported for **products**

We have to implement **getOne** and **create** methods also

**#2**

Add implementation for **categories**

**#3**

3nd party service supports JWT auth. We have to implement and support it. Use the credentials provided to appsettings.json file.

**#4**

We must measure and log the performance of the requests. Create a middleware to achieve this.

## Implementation

* Try to understand and keep the architectural approach.
* Add unit testing.
* Add docker support.
* Using CQRS pattern will be considered as a strong plus.
* The attached collections (postman/insomnia) will help you with the requests.

## Project structure

```
CSharpApp.sln
src/
  CSharpApp.Api               (endpoints, middleware, contracts, mappings, validation, DI wiring)
  CSharpApp.Application        (use-case services: single-item cache-aside via ICacheService)
  CSharpApp.Core               (domain: internal DTOs, interfaces, Result pattern, settings)
  CSharpApp.Infrastructure      (HttpClient-based API gateways, JWT auth, Polly, provider-shaped DTOs)
tests/
  CSharpApp.Tests              (xUnit + Moq unit tests)
```

Full folder layout, project by project:

```
src/CSharpApp.Api/
  Contracts/           CreateProductApiRequest.cs, CreateCategoryApiRequest.cs,
                        ProductResponse.cs, CategoryResponse.cs
  Endpoints/            ProductsEndpoints.cs, CategoriesEndpoints.cs
  Extensions/           ResultExtensions.cs
  Mappings/             ProductMappings.cs, CategoryMappings.cs
  Middleware/           RequestPerformanceMiddleware.cs, RequestPerformanceMiddlewareExtensions.cs
  Validation/           PaginationValidator.cs
  Program.cs
  appsettings.json / appsettings.Development.json

src/CSharpApp.Application/
  Products/             ProductsService.cs
  Categories/            CategoriesService.cs
  Common/               ApplicationServiceCollectionExtensions.cs, CacheService.cs

src/CSharpApp.Core/
  Dtos/                 ProductDto.cs, CategoryDto.cs, AuthTokenDto.cs
  Dtos/Requests/         CreateProductRequest.cs, CreateCategoryRequest.cs
  Interfaces/            IProductsService.cs, ICategoriesService.cs, IProductsApiClient.cs,
                        ICategoriesApiClient.cs, IAuthenticator.cs, IAuthTokenProvider.cs,
                        ICacheService.cs
  Common/               Result.cs, Error.cs, ErrorType.cs
  Common/Caching/        CacheKeys.cs
  Settings/             RestApiSettings.cs, HttpClientSettings.cs

src/CSharpApp.Infrastructure/
  Http/                 ProductsApiClient.cs, CategoriesApiClient.cs, Authenticator.cs,
                        AuthTokenProvider.cs, AuthenticationDelegatingHandler.cs
  Http/Dtos/             ProductFakePlatziDto.cs, CategoryFakePlatziDto.cs,
                        CreateProductFakePlatziRequest.cs, CreateCategoryFakePlatziRequest.cs,
                        AuthLoginRequestFakePlatziDto.cs, AuthLoginResponseFakePlatziDto.cs
  Mappings/             FakePlatziMappings.cs
  Extensions/            HttpResponseMessageExtensions.cs
  Configuration/          DefaultConfiguration.cs, HttpConfiguration.cs, HttpClientNames.cs

tests/CSharpApp.Tests/
  Common/               ResultTests.cs
  Application/           Products/, Common/ (service + cache tests)
  Infrastructure/         Http/ (API clients, auth), Support/ (FakeHttpMessageHandler)
```

## Architecture

Clean/Onion Architecture, four projects, dependencies pointing inward only
(`Api → Application → Core ← Infrastructure`). No CQRS/MediatR — plain services and
clients, by design, so every moving part is simple and easy to reason about end to end.

* **CSharpApp.Core** – Framework-agnostic domain layer, with **no dependency on the
  third-party provider's JSON shape at all** (no `System.Text.Json` attributes live here).
  Contains:
  * **Internal DTOs** (`ProductDto`, `CategoryDto`, `AuthTokenDto`, `CreateProductRequest`,
    `CreateCategoryRequest`) — this app's own, provider-agnostic models, used by every
    layer except the part of Infrastructure that talks HTTP.
  * **Service abstractions** (`IProductsService`, `ICategoriesService`, `IProductsApiClient`,
    `ICategoriesApiClient`, `IAuthenticator`, `IAuthTokenProvider`, `ICacheService`).
  * A lightweight **Result pattern** (`Result` / `Result<T>` / `Error` / `ErrorType`) used
    everywhere instead of throwing exceptions for expected failure modes (validation,
    not-found, conflict, upstream failures).
  * **Settings** POCOs (`RestApiSettings`, `HttpClientSettings`) bound from configuration.
* **CSharpApp.Application** – Use-case layer: `ProductsService`/`CategoriesService`
  implement the Core service interfaces by consuming the Infrastructure API clients and
  adding cross-cutting concerns — currently **cache-aside via `ICacheService`** (backed by
  `IMemoryCache`). Only single-item lookups (`GetByIdAsync`) are cached, for a few minutes;
  list results (`GetAllAsync`) are always fetched fresh from the API, since paginated list
  caching added invalidation complexity (a per-resource cache "generation" counter bumped
  on every `CreateAsync`) without enough benefit to justify it.
* **CSharpApp.Infrastructure** – Talks to the third-party API (`api.escuelajs.co`) using
  **typed `HttpClient`s created through `IHttpClientFactory`** (connections/handlers are
  pooled and reused, instead of `new HttpClient()` per call). Resiliency is provided by
  **Polly** (exponential-backoff retry + circuit breaker, `Microsoft.Extensions.Http.Polly`).
  This is the **only** layer allowed to know the third-party provider's exact wire shape:
  * `Http/Dtos/` holds the provider-shaped, `[JsonPropertyName]`-attributed DTOs
    (`ProductFakePlatziDto`, `CategoryFakePlatziDto`, `CreateProductFakePlatziRequest`,
    `CreateCategoryFakePlatziRequest`, `AuthLoginRequestFakePlatziDto`,
    `AuthLoginResponseFakePlatziDto`) used purely for HTTP (de)serialization.
  * `Mappings/FakePlatziMappings.cs` converts between those wire DTOs and Core's internal
    DTOs immediately at the HTTP boundary, so a provider response/request shape change
    never ripples past `CSharpApp.Infrastructure`.
  * `Extensions/HttpResponseMessageExtensions.cs` turns raw `HttpResponseMessage`s into
    `Result<T>`, translating upstream HTTP status codes/error shapes into the app's own
    `ErrorType`s.
  * JWT authentication is implemented by `AuthTokenProvider` (logs in with the credentials
    in `appsettings.json` via `Authenticator`, caches the token in memory until close to
    its `exp` claim, thread-safe refresh via `SemaphoreSlim` so concurrent callers share one
    in-flight login instead of each triggering their own) and
    `AuthenticationDelegatingHandler` (attaches the `Bearer` token to every request and
    retries once, after invalidating the token, on a `401`).
  * `ThirdPartyRequestPerformanceHandler` is the outermost `DelegatingHandler` on every
    named `HttpClient` (`Auth`, `Products`, `Categories`); it times and logs each outgoing
    call to the third-party service (including any retries/circuit-breaker short-circuits
    and the auth handshake), so upstream latency can be told apart from the API's own
    processing time reported by `RequestPerformanceMiddleware`.
  * `Configuration/` wires everything into DI (`DefaultConfiguration`, `HttpConfiguration`)
    and holds the named-`HttpClient` string constants (`HttpClientNames`).
* **CSharpApp.Api** – Minimal API endpoints (versioned with `Asp.Versioning`) that call the
  Application services directly and map `Result<T>` to HTTP responses.
  * `Contracts/` – request/response DTOs (`CreateProductApiRequest`, `ProductResponse`,
    `CategoryResponse`, …). Response DTOs are this API's **own public contract**,
    deliberately decoupled from both Core's internal DTOs and the third-party provider's
    shape, so neither a provider change nor an internal refactor silently changes what this
    API returns.
  * `Mappings/` – extension methods projecting Core's internal DTOs onto the public
    response DTOs (`ToResponse()`).
  * `Validation/` – request-level guard-clause validation (e.g. `PaginationValidator` for
    `offset`/`limit`).
  * `Extensions/` – `ResultExtensions`, adapting `Result<T>`/`Error` into minimal-API
    `IResult` responses (200/201/400/404/409/502 depending on `ErrorType`).
  * `Middleware/RequestPerformanceMiddleware` wraps the entire pipeline (registered as the
    outermost middleware) and measures/logs (via Serilog) the elapsed time and final HTTP
    status code of every request, warning-level if `>= 1000ms`. It is registered *outside*
    `app.UseExceptionHandler(...)`, so its `finally` block always sees the definitive status
    code — including `500` on an unhandled exception — instead of a stale pre-exception
    value.
  * `app.UseExceptionHandler(...)` converts any unhandled exception into a `ProblemDetails`
    `500` response (via `IProblemDetailsService`) instead of letting the request fail with
    an undefined status.
* **CSharpApp.Tests** – xUnit + Moq unit tests covering the Result pattern, the Application
  services (single-item cache-aside via `ICacheService`), the HTTP API clients (including
  pagination query-string building and provider-DTO-to-internal-DTO mapping) and the JWT
  auth provider/handler (using a fake `HttpMessageHandler`, no network calls).

## API versioning

Endpoints are versioned with `Asp.Versioning`/`Asp.Versioning.Mvc.ApiExplorer`. Routes use
the `api/v{version:apiVersion}/...` template, and `AddApiVersioning().AddApiExplorer(...)`
in `Program.cs` substitutes the concrete version (e.g. `v1`) into the route and into the
generated OpenAPI document/Swagger UI.

Every endpoint declares its supported version explicitly via `.HasApiVersion(1.0)`. To add
a `v2`:

1. Register a new handler on the same route with `.HasApiVersion(2.0)` (for endpoints that
   change behavior), or add a second `.HasApiVersion(2.0)` call to an existing registration
   (for endpoints that stay the same across versions).
2. Register an extra OpenAPI document (`builder.Services.AddOpenApi("v2")`) and a matching
   `options.SwaggerEndpoint("/openapi/v2.json", "CSharpApp API v2")` so Swagger UI can show
   it.

No other routing changes are needed — the `{version:apiVersion}` route constraint resolves
to whichever version the caller requests.

## Endpoints

| Method | Route                                        | Description                          |
|--------|----------------------------------------------|---------------------------------------|
| GET    | `/api/v1/products?offset={offset}&limit={limit}` | List products (pagination optional) |
| GET    | `/api/v1/products/{id}`                      | Get a product by id                  |
| POST   | `/api/v1/products`                           | Create a product                     |
| GET    | `/api/v1/categories?offset={offset}&limit={limit}` | List categories (pagination optional) |
| GET    | `/api/v1/categories/{id}`                    | Get a category by id                 |
| POST   | `/api/v1/categories`                         | Create a category                    |

`offset`/`limit` are both optional; when supplied, `offset` must be `>= 0` and `limit`
must be between `1` and `100`.

## Error handling

Every service/gateway method returns a `Result<T>` instead of throwing for expected failure
modes. `Api/Extensions/ResultExtensions.cs` maps `Error.Type` to an HTTP response:

| `ErrorType`  | HTTP status | Response shape                          |
|--------------|-------------|-------------------------------------------|
| `Validation` | 400         | `ValidationProblem` (field → message array) |
| `NotFound`   | 404         | `Problem` (RFC 7807)                        |
| `Conflict`   | 409         | `Problem` (RFC 7807)                        |
| anything else (`Failure`, `Unexpected`, upstream unreachable, …) | 502 | `Problem` (RFC 7807) |

Request-level validation (e.g. malformed `offset`/`limit`, an invalid `CreateProductApiRequest`)
is performed **before** ever calling the third-party service and reuses the same
`ValidationProblem` shape via `Error.ToValidationApiResult()`.

## Resiliency

Every third-party HTTP call goes through Polly-backed policies configured in
`HttpConfiguration`:

* **Retry** – exponential backoff (`HttpClientSettings.RetryCount` attempts, base delay
  `HttpClientSettings.SleepDuration` ms) on transient HTTP errors and `429 Too Many Requests`.
* **Circuit breaker** – opens after `HttpClientSettings.CircuitBreakerFailureThreshold`
  consecutive failures, staying open for `HttpClientSettings.CircuitBreakerDurationOfBreakSeconds`
  seconds before allowing a trial request through again.
* **Timeout** – each `HttpClient` has a fixed `HttpClientSettings.TimeoutSeconds` timeout.

All of the above are configurable via `HttpClientSettings` in `appsettings.json` (or
environment variables, see [Configuration](#configuration)) without any code changes.

Every outgoing call is additionally timed and logged by `ThirdPartyRequestPerformanceHandler`
(registered outermost, so it wraps retries/circuit-breaking as one logical attempt),
warning-level if `>= 1000ms` — see [Architecture](#architecture).

## Authentication

The third-party API requires a JWT bearer token:

1. `Authenticator` logs in against `RestApiSettings.Auth` using `RestApiSettings.Username`/
   `Password`, via a **dedicated, unauthenticated** named `HttpClient` (`HttpClientNames.Auth`)
   so obtaining a token never itself requires a token.
2. `AuthTokenProvider` caches the resulting access token in memory, refreshing it shortly
   before it expires (decoded from the JWT's `exp` claim, `30`-second safety buffer). A
   `SemaphoreSlim` ensures only one login request is ever in flight, even under concurrent
   callers.
3. `AuthenticationDelegatingHandler` is attached to the `Products`/`Categories` typed
   `HttpClient`s and adds the `Bearer` token to every outgoing request. On a `401`, it
   invalidates the cached token and retries the request once with a freshly issued token.

## Caching

`ICacheService` (backed by `IMemoryCache`) implements a simple cache-aside pattern used by
`ProductsService`/`CategoriesService` for single-item lookups (`GetByIdAsync`) only, keyed
via `CacheKeys` and expiring after a few minutes. List endpoints (`GetAllAsync`) always hit
the third-party API directly.

## Configuration

Settings are bound from `appsettings.json` (see `src/CSharpApp.Api/appsettings.json`) into
strongly-typed options:

```json
{
  "RestApiSettings": {
    "BaseUrl": "https://api.escuelajs.co/api/v1/",
    "Products": "products",
    "Categories": "categories",
    "Auth": "auth/login",
    "Username": "john@mail.com",
    "Password": "changeme"
  },
  "HttpClientSettings": {
    "RetryCount": 3,
    "SleepDuration": 200,
    "CircuitBreakerFailureThreshold": 5,
    "CircuitBreakerDurationOfBreakSeconds": 30,
    "TimeoutSeconds": 15
  }
}
```

Any value can be overridden via environment variables using the standard ASP.NET Core
double-underscore convention, e.g. `RestApiSettings__Username`, `RestApiSettings__Password`,
`HttpClientSettings__RetryCount` — handy for Docker/CI without touching the checked-in file.

## Running locally

```powershell
dotnet run --project src/CSharpApp.Api
```

Swagger UI is available at `http://localhost:{port}/swagger` in the `Development`
environment, backed by the generated OpenAPI document at `/openapi/v1.json`.

## Running the tests

```powershell
dotnet test tests/CSharpApp.Tests
```

## Running with Docker

```powershell
docker compose up --build
```

The API listens on `http://localhost:8080`. Third-party credentials can be overridden
without touching `appsettings.json` via environment variables, e.g.
`RestApiSettings__Username` / `RestApiSettings__Password` (see `docker-compose.yml`).

