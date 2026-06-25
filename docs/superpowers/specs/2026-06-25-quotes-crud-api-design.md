# Quotes CRUD REST API — Design Spec

**Issue:** #4 REST API to CRUD Quotes
**Date:** 2026-06-25
**Status:** Approved

---

## Summary

Add a standards-compliant CRUD REST API for quotes at `/api/v1.0/quotes` using ASP.NET Core Minimal API. The existing legacy controller (`/Api/*`) is left untouched. Write operations are protected by an API key. All responses use clean DTOs (no computed properties).

---

## Routes & Auth

New endpoint group mounted at `/api/v1.0/quotes` via a `QuotesEndpoints` Minimal API extension registered in `Program.cs`.

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/v1.0/quotes` | none | List all quotes, newest first |
| GET | `/api/v1.0/quotes/{id}` | none | Get single quote |
| POST | `/api/v1.0/quotes` | API key | Create quote |
| PUT | `/api/v1.0/quotes/{id}` | API key | Replace quote |
| DELETE | `/api/v1.0/quotes/{id}` | API key | Delete quote |

The existing `/Api/random` endpoint on the legacy controller stays as-is.

### Authentication

A custom `ApiKeyFilter` (`IEndpointFilter`) reads the `X-API-Key` request header and compares it against the configured key using `CryptographicOperations.FixedTimeEquals`. Returns `401 ProblemDetails` on mismatch or missing header. Applied only to POST, PUT, and DELETE.

**Configuration:** `ApiKey__Value` environment variable (maps to `ApiKey:Value` in `IConfiguration`). Never in `appsettings.json`.

---

## DTO & Validation

### Response DTO (`QuoteDto`)

```json
{
  "id": 42,
  "date": "2024-03-15",
  "author": "Albert Einstein",
  "authorInfo": "Physicist",
  "quoteText": "Imagination is more important than knowledge."
}
```

- `authorInfo` is nullable
- `date` serialized as `yyyy-MM-dd` (ISO 8601, no time component)
- No computed fields (`dateFormatted`, `authorWithInfo` are excluded)

### Write Request (`QuoteRequest`)

Identical shape for POST and PUT (PUT is a full replace):

```json
{
  "date": "2024-03-15",
  "author": "Albert Einstein",
  "authorInfo": "Physicist",
  "quoteText": "Imagination is more important than knowledge."
}
```

### FluentValidation Rules

| Field | Rule |
|-------|------|
| `quoteText` | Required, non-empty, max 2000 chars |
| `author` | Required, non-empty, max 200 chars |
| `date` | Required, valid date, not in the future |
| `authorInfo` | Optional, max 200 chars |

### Error Responses

All errors are RFC 9457 `ProblemDetails`. Never raw strings or anonymous objects.

| Scenario | Status |
|----------|--------|
| Validation failure | 422 |
| Resource not found | 404 |
| Missing/wrong API key | 401 |

---

## Architecture & Files

Reuses the existing `QuotesDbContext` and `Quote` entity — no EF Core migration needed.

### New files

```
WebApplication/
  Api/
    QuotesEndpoints.cs       ← IEndpointRouteBuilder extension, registers all 5 routes
    QuoteDto.cs              ← response record
    QuoteRequest.cs          ← create/update request record + FluentValidation validator
    ApiKeyFilter.cs          ← IEndpointFilter, X-API-Key check, 401 on mismatch
```

### Changes to existing files

**`Program.cs`** (two additions):
1. `builder.Services.AddValidatorsFromAssemblyContaining<QuoteRequest>()` — registers FluentValidation
2. `app.MapQuotesEndpoints()` — mounts the new endpoint group

### OpenAPI

`Microsoft.AspNetCore.OpenApi` + Scalar UI at `/scalar`. Each endpoint annotated with `.WithName()`, `.Produces<T>()`, and `.ProducesValidationProblem()`. No new NuGet packages required beyond existing stack dependencies.

---

## Testing

Two integration test classes under `tests/WebApplication.IntegrationTests/` using `WebApplicationFactory`. SQLite in-memory is acceptable (matches production DB engine).

### `QuotesApiReadTests`

- `GetAll_ReturnsQuotes_WhenDbHasData`
- `GetById_ReturnsQuote_WhenExists`
- `GetById_Returns404_WhenNotFound`

### `QuotesApiWriteTests`

- `Create_Returns201_WhenRequestIsValid`
- `Create_Returns422_WhenRequiredFieldMissing` (`[Theory]` parametrized: missing `quoteText`, `author`, `date`)
- `Create_Returns401_WhenApiKeyMissing`
- `Create_Returns401_WhenApiKeyWrong`
- `Update_Returns200_WhenRequestIsValid`
- `Update_Returns404_WhenIdNotFound`
- `Delete_Returns204_WhenExists`
- `Delete_Returns404_WhenIdNotFound`

Each test class seeds its own data and cleans up. The API key is injected via `WebApplicationFactory` configuration override.

---

## Acceptance Criteria

- [ ] `GET /api/v1.0/quotes` returns all quotes newest-first as a JSON array of `QuoteDto`
- [ ] `GET /api/v1.0/quotes/{id}` returns a single `QuoteDto` or `404`
- [ ] `POST /api/v1.0/quotes` creates a quote and returns `201` with `Location` header; requires valid `X-API-Key`
- [ ] `PUT /api/v1.0/quotes/{id}` replaces a quote and returns `200`; requires valid `X-API-Key`
- [ ] `DELETE /api/v1.0/quotes/{id}` deletes a quote and returns `204`; requires valid `X-API-Key`
- [ ] Missing or wrong `X-API-Key` on write endpoints returns `401 ProblemDetails`
- [ ] Validation failures return `422 ProblemDetails` with field-level detail
- [ ] Existing `/Api/*` endpoints are unaffected
- [ ] Scalar UI at `/scalar` documents all new endpoints
- [ ] All integration tests pass
