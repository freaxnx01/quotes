# GET /Api/count Endpoint Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a `GET /Api/count` endpoint to `QuotesApiController` that returns the total number of quotes as `{"count": N}`.

**Architecture:** Single method added to the existing `QuotesApiController`. Uses `_context.Quote.Count()` consistent with the synchronous style of all existing methods in that controller. No new files, no new dependencies.

**Tech Stack:** ASP.NET Core MVC, Entity Framework Core, `QuotesDbContext`

## Global Constraints

- Follow the existing controller style exactly — synchronous methods, `ObjectResult` return type, no new using directives unless required
- No auth attribute — existing public endpoints carry none and this endpoint is equally public
- Do not modify any other file

---

### Task 1: Add GetCount method to QuotesApiController

**Files:**
- Modify: `WebApplication/Controllers/QuotesApiController.cs`

**Interfaces:**
- Consumes: `_context.Quote` (existing `DbSet<Quote>` on `QuotesDbContext`)
- Produces: `GET /Api/count` → `200 OK` with anonymous object `{ count: int }`

- [ ] **Step 1: Read the existing controller**

Open `WebApplication/Controllers/QuotesApiController.cs` and familiarise yourself with the three existing methods (`GetAll`, `GetById`, `GetRandom`) and the `[Route("Api")]` class-level attribute.

- [ ] **Step 2: Add the GetCount method**

Add the following method after `GetRandom`, inside the `QuotesApiController` class:

```csharp
[HttpGet("count")]
public IActionResult GetCount()
{
    return new ObjectResult(new { count = _context.Quote.Count() });
}
```

No additional `using` directives are needed — `System.Linq` is already imported.

- [ ] **Step 3: Build to verify no compile errors**

```bash
dotnet build
```

Expected: build succeeds with 0 errors.

- [ ] **Step 4: Smoke-test manually**

Start the app and call the endpoint:

```bash
dotnet run --project WebApplication
curl http://localhost:<port>/Api/count
```

Expected response:
```json
{"count":42}
```

(The exact number depends on the database contents — any non-negative integer is correct.)

- [ ] **Step 5: Commit**

```bash
git add WebApplication/Controllers/QuotesApiController.cs
git commit -m "feat(api): add GET /Api/count endpoint

Returns the total number of quotes as {\"count\": N}.
No auth required — consistent with existing read endpoints.

Closes #12"
```
