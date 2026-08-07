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
  CSharpApp.Api             (endpoints, middleware, contracts, mappings, validation, DI wiring)
  CSharpApp.Application      (use-case services: caching, cache-generation invalidation)
  CSharpApp.Core             (domain: DTOs, interfaces, Result pattern, settings)
  CSharpApp.Infrastructure    (HttpClient-based API gateways, JWT auth, Polly)
tests/
  CSharpApp.Tests            (xUnit + Moq unit tests)
```

## Architecture

Clean/Onion Architecture, four projects, dependencies pointing inward only
(`Api → Application → Core ← Infrastructure`). No CQRS/MediatR — plain services and
clients, by design, so every moving part is simple and easy to reason about end to end.

* **CSharpApp.Core** – Framework-agnostic domain: DTOs (`Product`, `Category`, matching the
  third-party provider's JSON shape), settings, service abstractions (`IProductsService`,
  `ICategoriesService`, `IProductsApiClient`, `ICategoriesApiClient`, `IAuthTokenProvider`)
  and a lightweight **Result pattern** (`Result` / `Result<T>` / `Error`) used everywhere
  instead of throwing exceptions for expected failure modes (validation, not-found,
  upstream failures).
* **CSharpApp.Application** – Use-case layer: `ProductsService`/`CategoriesService`
  implement the Core service interfaces by consuming the Infrastructure API clients and
  adding cross-cutting concerns — currently **cache-aside via `IMemoryCache`**. List
  results (`GetAllAsync`, one cache entry per distinct `offset`/`limit` page) and
  single-item results are cached for a few minutes; a successful `CreateAsync` bumps a
  per-resource cache "generation" counter, instantly invalidating every previously cached
  list page (regardless of pagination) without needing to enumerate/remove individual keys.
* **CSharpApp.Infrastructure** – Talks to the third-party API (`api.escuelajs.co`) using
  **typed `HttpClient`s created through `IHttpClientFactory`** (connections/handlers are
  pooled and reused, instead of `new HttpClient()` per call). Resiliency is provided by
  **Polly** (exponential-backoff retry + circuit breaker, `Microsoft.Extensions.Http.Polly`).
  JWT authentication is implemented by `AuthTokenProvider` (logs in with the credentials in
  `appsettings.json`, caches the token in memory until close to its `exp` claim, thread-safe
  refresh via `SemaphoreSlim` so concurrent callers share one in-flight login instead of
  each triggering their own) and `AuthenticationDelegatingHandler` (attaches the `Bearer`
  token to every request and retries once, after invalidating the token, on a `401`).
* **CSharpApp.Api** – Minimal API endpoints (versioned with `Asp.Versioning`) that call the
  Application services directly and map `Result<T>` to HTTP responses.
  * `Contracts/` – request/response DTOs (`CreateProductApiRequest`, `ProductResponse`,
    `CategoryResponse`, …). Response DTOs are this API's **own public contract**,
    deliberately decoupled from the third-party provider's `Product`/`Category` shape in
    Core, so a provider response change never silently changes what this API returns.
  * `Mappings/` – extension methods projecting internal Core models onto the public
    response DTOs (`ToResponse()`).
  * `Validation/` – request-level guard-clause validation (e.g. `PaginationValidator` for
    `offset`/`limit`).
  * `Extensions/` – `ResultExtensions`, adapting `Result<T>`/`Error` into minimal-API
    `IResult` responses (200/201/400/404/409/502 depending on `ErrorType`).
  * `RequestPerformanceMiddleware` measures and logs (via Serilog) the elapsed time of
    every request.
* **CSharpApp.Tests** – xUnit + Moq unit tests covering the Result pattern, the Application
  services (caching/invalidation, including per-page pagination caching), the HTTP API
  clients (including pagination query-string building) and the JWT auth provider/handler
  (using a fake `HttpMessageHandler`, no network calls).

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

## Running locally

```powershell
dotnet run --project src/CSharpApp.Api
```

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

