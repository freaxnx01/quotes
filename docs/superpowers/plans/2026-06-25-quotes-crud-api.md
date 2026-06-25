# Quotes CRUD REST API Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `/api/v1.0/quotes` CRUD endpoints (Minimal API) protected by API key for writes, leaving the existing `/Api/*` controller untouched.

**Architecture:** New Minimal API endpoint group registered alongside the existing MVC controller in `Startup.cs`. Custom `IEndpointFilter` enforces `X-API-Key` on write operations. FluentValidation validates create/update requests. All responses use a clean `QuoteDto` record; the existing `Quote` entity and `QuotesDbContext` are reused without migration.

**Tech Stack:** .NET 10 · ASP.NET Core Minimal API · FluentValidation · Microsoft.AspNetCore.OpenApi + Scalar.AspNetCore · xUnit · FluentAssertions · Microsoft.AspNetCore.Mvc.Testing

## Global Constraints

- Target framework: `net10.0`
- Do NOT modify `DataModel/Quote.cs`, `DataModel/QuotesDbContext.cs`, or `WebApplication/Controllers/QuotesApiController.cs`
- Do NOT add EF Core migrations — `EnsureCreated()` already handles schema
- One auth scheme: `X-API-Key` header, configured via `ApiKey__Value` env var (maps to `ApiKey:Value` in IConfiguration)
- All error responses must be `ProblemDetails` (RFC 9457) — no raw strings
- New endpoints mount at `/api/v1.0/quotes` (not `/Api/`)
- Existing `/Api/*` and `/Api/random` routes must continue to work after all changes

---

## File Map

| Action | Path | Responsibility |
|--------|------|----------------|
| Create | `WebApplication/Api/QuoteDto.cs` | Response record |
| Create | `WebApplication/Api/QuoteRequest.cs` | Create/update request record + FluentValidation validator |
| Create | `WebApplication/Api/ApiKeyFilter.cs` | `IEndpointFilter` — checks `X-API-Key`, returns 401 ProblemDetails |
| Create | `WebApplication/Api/QuotesEndpoints.cs` | Registers all 5 routes on `IEndpointRouteBuilder` |
| Modify | `WebApplication/WebApplication.csproj` | Add FluentValidation, OpenAPI, Scalar packages |
| Modify | `WebApplication/Startup.cs` | Register FluentValidation + OpenAPI services; mount endpoints + Scalar UI |
| Create | `tests/WebApplication.IntegrationTests/WebApplication.IntegrationTests.csproj` | Integration test project |
| Create | `tests/WebApplication.IntegrationTests/QuotesApiFactory.cs` | `WebApplicationFactory<Program>` base with SQLite temp DB |
| Create | `tests/WebApplication.IntegrationTests/QuotesApiReadTests.cs` | GET endpoint integration tests |
| Create | `tests/WebApplication.IntegrationTests/QuotesApiWriteTests.cs` | POST/PUT/DELETE + auth integration tests |
| Modify | `Quotes.sln` | Add test project to solution |

---

## Task 1: NuGet packages, test project scaffold, OpenAPI registration

**Files:**
- Modify: `WebApplication/WebApplication.csproj`
- Modify: `WebApplication/Startup.cs`
- Create: `tests/WebApplication.IntegrationTests/WebApplication.IntegrationTests.csproj`
- Create: `tests/WebApplication.IntegrationTests/QuotesApiFactory.cs`
- Modify: `Quotes.sln`

**Interfaces:**
- Produces: `QuotesApiFactory : WebApplicationFactory<Program>` with `TestApiKey` constant and temp-file SQLite override — consumed by Tasks 3, 4, 5

- [ ] **Step 1: Add packages to WebApplication.csproj**

Replace the `<ItemGroup>` with package references in `WebApplication/WebApplication.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <RuntimeIdentifier>linux-musl-x64</RuntimeIdentifier>
        <PublishTrimmed>false</PublishTrimmed>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
        <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.9" />
        <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.9" />
        <PackageReference Include="Scalar.AspNetCore" Version="2.5.4" />
        <!-- Pin patched SQLite native (EF Core pulls vulnerable 2.1.11 transitively, GHSA-2m69-gcr7-jv3q) -->
        <PackageReference Include="SQLitePCLRaw.bundle_e_sqlite3" Version="3.0.3" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\DataModel\DataModel.csproj" />
    </ItemGroup>

</Project>
```

- [ ] **Step 2: Register OpenAPI and Scalar in Startup.cs**

In `ConfigureServices`, add after `services.AddScoped<IApplicationDbInitialization...>`:

```csharp
services.AddOpenApi();
```

In `Configure`, add after `app.UseStaticFiles()`:

```csharp
app.UseEndpoints(endpoints =>
{
    // existing route below — don't remove it
});
```

Find the existing `app.UseEndpoints(...)` block and ADD these two lines inside it (alongside the existing `MapControllerRoute`):

```csharp
endpoints.MapOpenApi();
endpoints.MapScalarApiReference();
```

So the full `UseEndpoints` call becomes:

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapOpenApi();
    endpoints.MapScalarApiReference();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
```

- [ ] **Step 3: Verify the app builds**

```bash
dotnet build WebApplication/WebApplication.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Create the integration test project file**

Create `tests/WebApplication.IntegrationTests/WebApplication.IntegrationTests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <IsPackable>false</IsPackable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="FluentAssertions" Version="7.2.0" />
        <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.9" />
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
        <PackageReference Include="xunit" Version="2.9.3" />
        <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
        <PackageReference Include="coverlet.collector" Version="6.0.4">
            <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
            <PrivateAssets>all</PrivateAssets>
        </PackageReference>
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\WebApplication\WebApplication.csproj" />
    </ItemGroup>

</Project>
```

- [ ] **Step 5: Create QuotesApiFactory**

Create `tests/WebApplication.IntegrationTests/QuotesApiFactory.cs`:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace WebApplication.IntegrationTests;

public sealed class QuotesApiFactory : WebApplicationFactory<Program>, IDisposable
{
    public const string TestApiKey = "integration-test-key"; // gitleaks:allow

    private readonly string _quotesDb = Path.Combine(Path.GetTempPath(), $"quotes_test_{Guid.NewGuid():N}.db");
    private readonly string _identityDb = Path.Combine(Path.GetTempPath(), $"identity_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:QuotesDbContext"] = $"Data Source={_quotesDb}",
                ["ConnectionStrings:IdentityDbContext"] = $"Data Source={_identityDb}",
                ["ApiKey:Value"] = TestApiKey,
                ["AdminUserSettings:Email"] = "test@test.com",
                ["AdminUserSettings:Pwd"] = "IntTest-Pwd-1!", // gitleaks:allow
                ["AdminUserSettings:RoleName"] = "Admin",
                ["Settings:PathBase"] = "",
                ["QuotesPathBase"] = "",
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            if (File.Exists(_quotesDb)) File.Delete(_quotesDb);
            if (File.Exists(_identityDb)) File.Delete(_identityDb);
        }
    }
}
```

- [ ] **Step 6: Add test project to solution**

```bash
dotnet sln Quotes.sln add tests/WebApplication.IntegrationTests/WebApplication.IntegrationTests.csproj
```

- [ ] **Step 7: Verify test project builds**

```bash
dotnet build tests/WebApplication.IntegrationTests/WebApplication.IntegrationTests.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 8: Commit**

```bash
git add WebApplication/WebApplication.csproj WebApplication/Startup.cs \
    tests/WebApplication.IntegrationTests/ Quotes.sln
git commit -m "chore(api): scaffold test project, add FluentValidation/OpenAPI/Scalar packages"
```

---

## Task 2: QuoteDto, QuoteRequest & validator

**Files:**
- Create: `WebApplication/Api/QuoteDto.cs`
- Create: `WebApplication/Api/QuoteRequest.cs`
- Modify: `WebApplication/Startup.cs` (register FluentValidation)
- Create: `tests/WebApplication.IntegrationTests/QuoteRequestValidatorTests.cs`

**Interfaces:**
- Consumes: nothing from prior tasks
- Produces:
  - `QuoteDto(int Id, DateOnly Date, string Author, string? AuthorInfo, string QuoteText)` — consumed by Tasks 4, 5
  - `QuoteRequest(DateOnly Date, string Author, string? AuthorInfo, string QuoteText)` — consumed by Task 5
  - `QuoteRequestValidator : AbstractValidator<QuoteRequest>` — consumed by Task 5 (injected as `IValidator<QuoteRequest>`)

- [ ] **Step 1: Create QuoteDto**

Create `WebApplication/Api/QuoteDto.cs`:

```csharp
namespace WebApplication.Api;

public sealed record QuoteDto(
    int Id,
    DateOnly Date,
    string Author,
    string? AuthorInfo,
    string QuoteText);
```

- [ ] **Step 2: Create QuoteRequest and validator**

Create `WebApplication/Api/QuoteRequest.cs`:

```csharp
using FluentValidation;

namespace WebApplication.Api;

public sealed record QuoteRequest(
    DateOnly Date,
    string Author,
    string? AuthorInfo,
    string QuoteText);

public sealed class QuoteRequestValidator : AbstractValidator<QuoteRequest>
{
    public QuoteRequestValidator()
    {
        RuleFor(x => x.QuoteText)
            .NotEmpty().WithMessage("QuoteText is required.")
            .MaximumLength(2000);

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(200);

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date must not be in the future.");

        When(x => x.AuthorInfo is not null, () =>
            RuleFor(x => x.AuthorInfo).MaximumLength(200));
    }
}
```

- [ ] **Step 3: Register FluentValidation in Startup.ConfigureServices**

Add after `services.AddControllersWithViews()` in `Startup.cs`:

```csharp
services.AddValidatorsFromAssemblyContaining<QuoteRequestValidator>();
```

This requires `using FluentValidation;` at the top of `Startup.cs`.

- [ ] **Step 4: Write failing validator unit tests**

Create `tests/WebApplication.IntegrationTests/QuoteRequestValidatorTests.cs`:

```csharp
using FluentAssertions;
using WebApplication.Api;
using Xunit;

namespace WebApplication.IntegrationTests;

public sealed class QuoteRequestValidatorTests
{
    private readonly QuoteRequestValidator _validator = new();

    [Fact]
    public void Validate_ReturnsValid_WhenAllFieldsAreCorrect()
    {
        var request = new QuoteRequest(
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            Author: "Einstein",
            AuthorInfo: "Physicist",
            QuoteText: "Imagination is everything.");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ReturnsInvalid_WhenQuoteTextIsEmpty()
    {
        var request = new QuoteRequest(DateOnly.FromDateTime(DateTime.UtcNow), "Einstein", null, "");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "QuoteText");
    }

    [Fact]
    public void Validate_ReturnsInvalid_WhenAuthorIsEmpty()
    {
        var request = new QuoteRequest(DateOnly.FromDateTime(DateTime.UtcNow), "", null, "Some quote.");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Author");
    }

    [Fact]
    public void Validate_ReturnsInvalid_WhenDateIsInTheFuture()
    {
        var request = new QuoteRequest(
            Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Author: "Einstein",
            AuthorInfo: null,
            QuoteText: "Some quote.");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Date");
    }

    [Fact]
    public void Validate_ReturnsInvalid_WhenAuthorInfoExceedsMaxLength()
    {
        var request = new QuoteRequest(
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            Author: "Einstein",
            AuthorInfo: new string('x', 201),
            QuoteText: "Some quote.");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AuthorInfo");
    }

    [Fact]
    public void Validate_ReturnsValid_WhenAuthorInfoIsNull()
    {
        var request = new QuoteRequest(DateOnly.FromDateTime(DateTime.UtcNow), "Einstein", null, "Some quote.");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
```

- [ ] **Step 5: Run tests — expect FAIL (types don't exist yet)**

```bash
dotnet test tests/WebApplication.IntegrationTests/ --filter "QuoteRequestValidatorTests"
```

Expected: FAIL — compilation error (types not yet defined).

- [ ] **Step 6: Run tests — expect PASS after Step 2 & 3**

```bash
dotnet test tests/WebApplication.IntegrationTests/ --filter "QuoteRequestValidatorTests"
```

Expected: 6 tests pass.

- [ ] **Step 7: Commit**

```bash
git add WebApplication/Api/QuoteDto.cs WebApplication/Api/QuoteRequest.cs \
    WebApplication/Startup.cs \
    tests/WebApplication.IntegrationTests/QuoteRequestValidatorTests.cs
git commit -m "feat(api): add QuoteDto, QuoteRequest, and FluentValidation validator"
```

---

## Task 3: ApiKeyFilter

**Files:**
- Create: `WebApplication/Api/ApiKeyFilter.cs`

**Interfaces:**
- Consumes: `IConfiguration["ApiKey:Value"]`
- Produces: `ApiKeyFilter : IEndpointFilter` — consumed by Task 4/5 via `.AddEndpointFilter<ApiKeyFilter>()`

- [ ] **Step 1: Write failing integration test for API key enforcement**

Add a new file `tests/WebApplication.IntegrationTests/ApiKeyFilterTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System.Net;
using WebApplication.Api;
using Xunit;

namespace WebApplication.IntegrationTests;

public sealed class ApiKeyFilterTests : IClassFixture<QuotesApiFactory>
{
    private readonly HttpClient _client;

    public ApiKeyFilterTests(QuotesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Returns401_WhenApiKeyHeaderIsMissing()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1.0/quotes",
            new { date = "2024-01-01", author = "A", quoteText = "Q" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_Returns401_WhenApiKeyHeaderIsWrong()
    {
        _client.DefaultRequestHeaders.Remove("X-API-Key");
        _client.DefaultRequestHeaders.Add("X-API-Key", "wrong-key");

        var response = await _client.PostAsJsonAsync(
            "/api/v1.0/quotes",
            new { date = "2024-01-01", author = "A", quoteText = "Q" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        _client.DefaultRequestHeaders.Remove("X-API-Key");
    }
}
```

- [ ] **Step 2: Run test — expect FAIL (endpoints not registered yet)**

```bash
dotnet test tests/WebApplication.IntegrationTests/ --filter "ApiKeyFilterTests"
```

Expected: FAIL — 404 returned, not 401.

- [ ] **Step 3: Create ApiKeyFilter**

Create `WebApplication/Api/ApiKeyFilter.cs`:

```csharp
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WebApplication.Api;

internal sealed class ApiKeyFilter(IConfiguration configuration) : IEndpointFilter
{
    private static readonly string HeaderName = "X-API-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var configuredKey = configuration["ApiKey:Value"] ?? string.Empty;

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey)
            || !FixedTimeEquals(providedKey.ToString(), configuredKey))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Invalid or missing API key.");
        }

        return await next(context);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        // Length check outside FixedTimeEquals leaks length info, acceptable for personal-use API key.
        if (aBytes.Length != bBytes.Length) return false;
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}
```

Note: the filter tests require the endpoint to exist (Task 4 registers it). The test will pass after Task 4's endpoint registration. Commit the filter now; the test validates after Task 4.

- [ ] **Step 4: Commit**

```bash
git add WebApplication/Api/ApiKeyFilter.cs \
    tests/WebApplication.IntegrationTests/ApiKeyFilterTests.cs
git commit -m "feat(api): add ApiKeyFilter IEndpointFilter for X-API-Key auth"
```

---

## Task 4: Read endpoints (GET /api/v1.0/quotes, GET /api/v1.0/quotes/{id})

**Files:**
- Create: `WebApplication/Api/QuotesEndpoints.cs`
- Modify: `WebApplication/Startup.cs` (mount endpoint group)
- Create: `tests/WebApplication.IntegrationTests/QuotesApiReadTests.cs`

**Interfaces:**
- Consumes: `QuoteDto` (Task 2), `QuotesDbContext` + `Quote` (existing), `ApiKeyFilter` (Task 3)
- Produces: `IEndpointRouteBuilder.MapQuotesEndpoints()` extension method — consumed by Startup.cs; also relied on by `ApiKeyFilterTests` (Task 3) and `QuotesApiWriteTests` (Task 5)

- [ ] **Step 1: Write failing read endpoint tests**

Create `tests/WebApplication.IntegrationTests/QuotesApiReadTests.cs`:

```csharp
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using WebApplication.Api;
using Xunit;

namespace WebApplication.IntegrationTests;

public sealed class QuotesApiReadTests : IClassFixture<QuotesApiFactory>
{
    private readonly HttpClient _client;

    public QuotesApiReadTests(QuotesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyArray_WhenNoQuotesExist()
    {
        var response = await _client.GetAsync("/api/v1.0/quotes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quotes = await response.Content.ReadFromJsonAsync<List<QuoteDto>>();
        quotes.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetById_Returns404_WhenQuoteDoesNotExist()
    {
        var response = await _client.GetAsync("/api/v1.0/quotes/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

- [ ] **Step 2: Run tests — expect FAIL (404, endpoint not registered)**

```bash
dotnet test tests/WebApplication.IntegrationTests/ --filter "QuotesApiReadTests"
```

Expected: FAIL.

- [ ] **Step 3: Create QuotesEndpoints.cs with GET endpoints**

Create `WebApplication/Api/QuotesEndpoints.cs`:

```csharp
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Quotes.DataModel;

namespace WebApplication.Api;

internal static class QuotesEndpoints
{
    internal static IEndpointRouteBuilder MapQuotesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1.0/quotes").WithOpenApi();

        group.MapGet("/", GetAll)
            .WithName("GetAllQuotes")
            .Produces<List<QuoteDto>>();

        group.MapGet("/{id:int}", GetById)
            .WithName("GetQuoteById")
            .Produces<QuoteDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", Create)
            .WithName("CreateQuote")
            .Produces<QuoteDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .AddEndpointFilter<ApiKeyFilter>();

        group.MapPut("/{id:int}", Update)
            .WithName("UpdateQuote")
            .Produces<QuoteDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AddEndpointFilter<ApiKeyFilter>();

        group.MapDelete("/{id:int}", Delete)
            .WithName("DeleteQuote")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AddEndpointFilter<ApiKeyFilter>();

        return app;
    }

    private static Ok<List<QuoteDto>> GetAll(QuotesDbContext db) =>
        TypedResults.Ok(
            db.Quote
                .OrderByDescending(q => q.ID)
                .Select(q => ToDto(q))
                .ToList());

    private static Results<Ok<QuoteDto>, NotFound> GetById(int id, QuotesDbContext db)
    {
        var quote = db.Quote.FirstOrDefault(q => q.ID == id);
        return quote is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ToDto(quote));
    }

    private static async Task<Results<Created<QuoteDto>, ValidationProblem>> Create(
        QuoteRequest request,
        IValidator<QuoteRequest> validator,
        QuotesDbContext db)
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        var quote = FromRequest(request);
        db.Quote.Add(quote);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/api/v1.0/quotes/{quote.ID}", ToDto(quote));
    }

    private static async Task<Results<Ok<QuoteDto>, NotFound, ValidationProblem>> Update(
        int id,
        QuoteRequest request,
        IValidator<QuoteRequest> validator,
        QuotesDbContext db)
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        var quote = db.Quote.FirstOrDefault(q => q.ID == id);
        if (quote is null)
            return TypedResults.NotFound();

        quote.Date = request.Date.ToDateTime(TimeOnly.MinValue);
        quote.Author = request.Author;
        quote.AuthorInfo = request.AuthorInfo;
        quote.QuoteText = request.QuoteText;
        await db.SaveChangesAsync();

        return TypedResults.Ok(ToDto(quote));
    }

    private static async Task<Results<NoContent, NotFound>> Delete(int id, QuotesDbContext db)
    {
        var quote = db.Quote.FirstOrDefault(q => q.ID == id);
        if (quote is null)
            return TypedResults.NotFound();

        db.Quote.Remove(quote);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static QuoteDto ToDto(Quote q) =>
        new(q.ID, DateOnly.FromDateTime(q.Date), q.Author, q.AuthorInfo, q.QuoteText);

    private static Quote FromRequest(QuoteRequest r) =>
        new()
        {
            Date = r.Date.ToDateTime(TimeOnly.MinValue),
            Author = r.Author,
            AuthorInfo = r.AuthorInfo,
            QuoteText = r.QuoteText,
        };
}
```

- [ ] **Step 4: Mount endpoints in Startup.Configure**

Inside the existing `app.UseEndpoints(endpoints => { ... })` block in `Startup.cs`, add one line before `MapControllerRoute`:

```csharp
endpoints.MapQuotesEndpoints();
```

Add `using WebApplication.Api;` at the top of `Startup.cs`.

The full `UseEndpoints` block should now read:

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapOpenApi();
    endpoints.MapScalarApiReference();
    endpoints.MapQuotesEndpoints();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
```

- [ ] **Step 5: Build**

```bash
dotnet build WebApplication/WebApplication.csproj
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 6: Run read tests — expect PASS**

```bash
dotnet test tests/WebApplication.IntegrationTests/ --filter "QuotesApiReadTests"
```

Expected: 2 tests pass.

- [ ] **Step 7: Run ApiKeyFilter tests — expect PASS (endpoint now exists)**

```bash
dotnet test tests/WebApplication.IntegrationTests/ --filter "ApiKeyFilterTests"
```

Expected: 2 tests pass.

- [ ] **Step 8: Commit**

```bash
git add WebApplication/Api/QuotesEndpoints.cs WebApplication/Startup.cs \
    tests/WebApplication.IntegrationTests/QuotesApiReadTests.cs
git commit -m "feat(api): add GET /api/v1.0/quotes and GET /api/v1.0/quotes/{id}"
```

---

## Task 5: Write endpoints (POST, PUT, DELETE) + full integration tests

**Files:**
- Modify: `tests/WebApplication.IntegrationTests/QuotesApiWriteTests.cs`

Note: All write endpoint code is already in `QuotesEndpoints.cs` from Task 4. This task verifies them with tests.

**Interfaces:**
- Consumes: `QuotesEndpoints.MapQuotesEndpoints()` (Task 4), `QuoteDto` (Task 2), `QuoteRequest` (Task 2), `ApiKeyFilter` (Task 3)
- Produces: nothing (terminal task)

- [ ] **Step 1: Write failing write tests**

Create `tests/WebApplication.IntegrationTests/QuotesApiWriteTests.cs`:

```csharp
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebApplication.Api;
using Xunit;

namespace WebApplication.IntegrationTests;

public sealed class QuotesApiWriteTests : IClassFixture<QuotesApiFactory>
{
    private readonly HttpClient _client;

    public QuotesApiWriteTests(QuotesApiFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-API-Key", QuotesApiFactory.TestApiKey);
    }

    // ── POST ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_Returns201_WhenRequestIsValid()
    {
        var request = new QuoteRequest(
            Date: new DateOnly(2024, 3, 15),
            Author: "Einstein",
            AuthorInfo: "Physicist",
            QuoteText: "Imagination is everything.");

        var response = await _client.PostAsJsonAsync("/api/v1.0/quotes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<QuoteDto>();
        created.Should().NotBeNull();
        created!.Author.Should().Be("Einstein");
        created.QuoteText.Should().Be("Imagination is everything.");
        created.Date.Should().Be(new DateOnly(2024, 3, 15));
    }

    [Theory]
    [InlineData("quoteText", "2024-01-01", "Einstein", "", "Missing quoteText")]
    [InlineData("author",    "2024-01-01", "",         "Some quote", "Missing author")]
    [InlineData("date",      "0001-01-01", "Einstein", "Some quote", "Missing date")]
    public async Task Create_Returns422_WhenRequiredFieldMissing(
        string missingField, string date, string author, string quoteText, string _)
    {
        var payload = new { date, author, quoteText };
        var response = await _client.PostAsJsonAsync("/api/v1.0/quotes", payload);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_Returns401_WhenApiKeyMissing()
    {
        using var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var response = await client.PostAsJsonAsync(
            "/api/v1.0/quotes",
            new QuoteRequest(new DateOnly(2024, 1, 1), "A", null, "Q"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Returns401_WhenApiKeyIsWrong()
    {
        using var client = new HttpClient { BaseAddress = _client.BaseAddress };
        client.DefaultRequestHeaders.Add("X-API-Key", "wrong-key");
        var response = await client.PostAsJsonAsync(
            "/api/v1.0/quotes",
            new QuoteRequest(new DateOnly(2024, 1, 1), "A", null, "Q"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_Returns200_WhenRequestIsValid()
    {
        // Seed a quote via POST
        var created = await CreateQuoteAsync("Original author", "Original text");

        var updateRequest = new QuoteRequest(
            Date: new DateOnly(2024, 6, 1),
            Author: "Updated author",
            AuthorInfo: null,
            QuoteText: "Updated text.");

        var response = await _client.PutAsJsonAsync($"/api/v1.0/quotes/{created.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<QuoteDto>();
        updated!.Author.Should().Be("Updated author");
        updated.QuoteText.Should().Be("Updated text.");
    }

    [Fact]
    public async Task Update_Returns404_WhenIdDoesNotExist()
    {
        var request = new QuoteRequest(new DateOnly(2024, 1, 1), "A", null, "Q");
        var response = await _client.PutAsJsonAsync("/api/v1.0/quotes/99999", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Returns204_WhenQuoteExists()
    {
        var created = await CreateQuoteAsync("Delete me", "Temporary quote.");

        var response = await _client.DeleteAsync($"/api/v1.0/quotes/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1.0/quotes/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Returns404_WhenIdDoesNotExist()
    {
        var response = await _client.DeleteAsync("/api/v1.0/quotes/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private async Task<QuoteDto> CreateQuoteAsync(string author, string quoteText)
    {
        var request = new QuoteRequest(new DateOnly(2024, 1, 1), author, null, quoteText);
        var response = await _client.PostAsJsonAsync("/api/v1.0/quotes", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QuoteDto>())!;
    }
}
```

- [ ] **Step 2: Run tests — expect FAIL (write endpoints not tested yet)**

```bash
dotnet test tests/WebApplication.IntegrationTests/ --filter "QuotesApiWriteTests"
```

Expected: FAIL — most tests should fail since write endpoints exist but this test class is new.

- [ ] **Step 3: Run full test suite — expect ALL PASS**

```bash
dotnet test tests/WebApplication.IntegrationTests/
```

Expected: All tests pass. (Write endpoints were implemented in Task 4 alongside read; tests now exercise them.)

If any test fails, check:
- `QuoteDto` JSON deserialization: ensure `DateOnly` is serialized as `yyyy-MM-dd`. If not, add to `Startup.ConfigureServices`:
  ```csharp
  services.ConfigureHttpJsonOptions(opts =>
      opts.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
  ```
  `DateOnly` serialization is built in as of .NET 7 — no extra configuration needed.
- Factory isolation: if tests interfere with each other, each `IClassFixture<QuotesApiFactory>` creates its own factory with a unique SQLite file. This is by design.

- [ ] **Step 4: Verify the legacy `/Api/*` endpoints still work**

```bash
dotnet run --project WebApplication/WebApplication.csproj &
sleep 3
curl -s http://localhost:5000/Api | head -c 200
curl -s http://localhost:5000/Api/random
kill %1
```

Expected: JSON responses from the legacy controller, no errors.

- [ ] **Step 5: Commit**

```bash
git add tests/WebApplication.IntegrationTests/QuotesApiWriteTests.cs
git commit -m "test(api): add integration tests for POST/PUT/DELETE /api/v1.0/quotes"
```

---

## Self-Review

**Spec coverage check:**

| Spec requirement | Task |
|---|---|
| `GET /api/v1.0/quotes` list, newest first | Task 4 |
| `GET /api/v1.0/quotes/{id}` or 404 | Task 4 |
| `POST /api/v1.0/quotes` → 201 + Location | Task 4 (code) + Task 5 (test) |
| `PUT /api/v1.0/quotes/{id}` → 200 | Task 4 (code) + Task 5 (test) |
| `DELETE /api/v1.0/quotes/{id}` → 204 | Task 4 (code) + Task 5 (test) |
| 401 ProblemDetails on wrong/missing key | Task 3 (filter) + Tasks 4/5 (tests) |
| 422 ProblemDetails on validation failure | Task 2 (validator) + Task 5 (tests) |
| Clean DTO (no computed fields) | Task 2 |
| FluentValidation: quoteText/author/date required | Task 2 |
| API key via `ApiKey:Value` config | Task 3 |
| OpenAPI + Scalar at `/scalar` | Task 1 |
| Existing `/Api/*` unaffected | Task 4 (mount strategy), Task 5 (manual verify step) |
| Integration tests with WebApplicationFactory | Tasks 1, 4, 5 |

All requirements covered. No placeholders found. Type names consistent across all tasks.
