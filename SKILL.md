[//]: # (Source of truth: .ai/base-instructions.md + .ai/stacks/dotnet-webapi.md — update those, then regenerate by re-running /sync-ai-instructions)

# SKILL.md — OpenClaw Agent Skill

This skill configures OpenClaw for this project.

# AI Agent Base Instructions

Canonical, **stack-agnostic** reference for all AI coding agents. Applies to every project regardless of language or framework. Stack-specific overlays live in `.ai/stacks/<stack>.md` and are loaded alongside this file. A project loads **base + exactly one stack overlay**. Tool-specific files (`CLAUDE.md`, `.github/copilot-instructions.md`, `SKILL.md`) derive from base + the chosen stack.

> **Workflow role:** If a `WORKFLOW-ROLE.md` exists at the repo root, read it before continuing — it describes this repo's place in the personal dev workflow (implementer / consumer / workflow infrastructure). See `ai-instructions/workflows/personal-dev-workflow.md` for the workflow doc itself.
>
> **Project context:** If a `PROJECT-OVERVIEW.md` exists at the repo root, read it before continuing — it describes this repo's product/project context (name, purpose, stakeholders, vision, core customer need, key features, architecture in one paragraph). Per-feature PRDs live under `docs/specs/` or `designs/`; ADRs under `docs/adr/`.
>
> **Agent notes:** If an `AGENT-NOTES.md` exists at the repo root, read it before continuing — it holds project-specific agent-facing context that doesn't fit in the regenerated CLAUDE.md: operational gotchas, project-specific commands, repo-local workflow conventions (branch naming, PR conventions, etc.).

---

## Working Method (before any code)

Meta-rules for *how* to approach a task. Framing adapted from [multica-ai/andrej-karpathy-skills](https://github.com/multica-ai/andrej-karpathy-skills).

- **State assumptions explicitly.** If multiple interpretations exist, present them — don't pick silently.
- **Ask when unclear.** Don't hide confusion behind plausible-looking code.
- **Push back when a simpler approach exists.** Minimum code that solves the problem; nothing speculative (no unrequested flexibility, configurability, or error handling for impossible cases).
- **Surgical edits.** Every changed line must trace to the request. Don't "improve" adjacent code, comments, or formatting. Match existing style. Remove orphans *your* change created — leave pre-existing dead code alone (mention it instead).
- **Goal-driven execution.** Restate the task as a verifiable success criterion before starting. For multi-step work, write a brief numbered plan with a `verify:` check per step, then loop until each check passes.

---

## Clean Code Principles

Apply to all generated and modified code, regardless of language:

- **Small methods/functions** — each does one thing at one level of abstraction; aim for ≤20 lines
- **Guard clauses** — validate and return/throw early at the top; avoid nested `if/else` pyramids
- **Command-Query Separation** — a function either performs an action (command, returns nothing) or returns data (query), never both
- **No flag arguments** — avoid boolean parameters that switch behaviour; split into two clearly named functions instead
- **Meaningful names** — names reveal intent; no abbreviations (`cnt`, `mgr`, `svc`) except universally understood ones (`id`, `url`, `dto`)
- **One level of abstraction per function** — don't mix high-level orchestration with low-level detail; extract helpers
- **Fail fast** — detect invalid state as early as possible and throw specific errors; don't let bad data travel deep into the call stack
- **DRY** — if the same logic exists in two places, extract it; but prefer duplication over the wrong abstraction — wait until the pattern is clear before generalising
- **No dead code** — delete unreachable branches, unused parameters, and vestigial methods; git has history
- **No commented-out code blocks** — delete them, git has history

---

## Testing — TDD, Tests First, No Shortcuts

Applies to every language and framework:

1. Write the failing test first
2. Write the minimum implementation to make it pass
3. Refactor
4. **Never modify a test to make it green** — fix the implementation
5. **Never hardcode return values, mock results, or stub logic** to satisfy a test
6. **Never silently swallow exceptions** to make a test green
7. **After implementation, run the full test suite** — not just the new test
8. **If a test fails after 3 attempts, STOP** and explain what's going wrong instead of continuing to iterate
9. Test naming: `MethodName_StateUnderTest_ExpectedBehavior` (or the idiomatic equivalent for the target language)
10. E2E tests must be independent and idempotent — seed and clean up their own data

Framework-specific test project layout, mocking library choice, and assertion library live in the stack overlay.

---

## UI Development Workflow (Mandatory Phase Order)

**Never skip phases. Never write component code before wireframe approval.**

| Phase | Skill | Gate |
|---|---|---|
| 1 — Brainstorm | `/ui-brainstorm` | ASCII wireframe approved |
| 2 — Flow       | `/ui-flow`       | Mermaid diagrams approved |
| 3 — Build      | `/ui-build`      | Shell → logic → interactions → polish |
| 4 — Review     | `/ui-review`     | Checklist passes |

Skill files live in `.ai/skills/`. The skills themselves are stack-neutral — UI component library preferences (e.g. MudBlazor, shadcn/ui, Material, Flutter widgets) are captured in the active stack overlay.

### What to check before writing UI code

- [ ] Does a similar component already exist in a shared folder?
- [ ] Has the ASCII wireframe been approved?
- [ ] Has the Mermaid flow been approved?
- [ ] Are you building the shell first (no business logic yet)?
- [ ] Does the component need a unit/component test?

---

## Localization (i18n) & Regional Formatting

User-facing apps must support **`de` and `en`**. CI tooling and developer-only utilities are exempt.

### Language

- Default language resolved from the OS / browser locale at first launch
- User can override at runtime via an in-app language switcher
- The user's choice is persisted (cookie, preferences store, or user profile — stack-specific)

### Regional formatting (decoupled from language)

Regional formatting (date, time, number, currency separators) is selected from the OS region — **not** dictated by the language.

- Auto-detect any `de-*` OS region (`de-CH`, `de-DE`, `de-AT`, …) and use the matching culture
- If the language is `de` but the OS region is missing or unrecognized: fall back to **`de-CH`**
- For `en`: use the OS-provided region (typically `en-US` / `en-GB`) — do not force a default

### Rules

- All date / number / currency rendering goes through the platform's localization API — never hand-format with raw `string.Format` / `toString()` / template literals.
- Do not couple regional formatting to the UI language. A user can read German text with US formatting, or English text with Swiss formatting; both must work.
- Stack overlays specify the concrete API (`CultureInfo` + `RequestLocalization` for .NET, `flutter_localizations` + `intl` for Flutter, etc.).

---

## Versioning (SemVer)

All projects follow [Semantic Versioning 2.0.0](https://semver.org/): `MAJOR.MINOR.PATCH` — `MAJOR` = breaking, `MINOR` = new feature (backwards-compatible), `PATCH` = bug fix.

Conventional Commits mapping: `BREAKING CHANGE:` footer or `!` after type → MAJOR; `feat` → MINOR; `fix`, `perf` → PATCH; `chore`, `docs`, `ci`, `test`, `refactor` → no bump.

- Git tags follow `v<MAJOR>.<MINOR>.<PATCH>` (e.g. `v1.3.0`) — tag on `main` after merge
- Pre-release: `v1.0.0-alpha.1`, `v1.0.0-beta.2`, `v1.0.0-rc.1`
- **git-cliff** is the changelog and release notes tool — configured via `cliff.toml`
- Where the version is declared in the project (build file, manifest, etc.) is defined by the stack overlay — but it must be declared in **exactly one place**

---

## Changelog

All projects maintain a `CHANGELOG.md` in the repo root following [Keep a Changelog](https://keepachangelog.com) conventions. **Sections per release:** `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`.

- `[Unreleased]` section accumulates changes until a release is cut
- Auto-generation: **git-cliff** with `cliff.toml` configured for Conventional Commits
- CI integration: `orhun/git-cliff-action` in GitHub Actions generates release notes into GitHub Releases
- CI can validate that `[Unreleased]` is not empty before allowing a release branch

Example: [`.ai/references/base/changelog-example.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/base/changelog-example.md)

---

## 12-Factor App Compliance

Projects follow the [12-Factor App](https://www.12factor.net/) methodology: one repo per service, all deps declared, env-var config, attached backing services, separate build/release/run stages, stateless processes, port binding, scale via replicas not threads, fast disposability, dev/prod parity, logs to stdout, admin processes as one-offs.

Stack-specific enforcement details (logging library, migrations, etc.) live in the stack overlay.

Full per-factor table: [`.ai/references/base/12-factor.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/base/12-factor.md)

---

## Branching Strategy (GitHub Flow + protection rules)

```text
main              ← always deployable, protected
  └── feature/<issue-id>-short-description
  └── fix/<issue-id>-short-description
  └── chore/<short-description>
  └── release/<version>   ← only if needed for staged releases
```

- `main` requires: passing CI, at least 1 PR review, no direct push
- Branch from `main`, PR back to `main`
- Delete branch after merge
- Rebase or squash merge — no merge commits on `main`

---

## Git Worktrees

### Worktree directory

- Use **project-local** worktrees under `.worktrees/` at the repo root (hidden directory)
- `.worktrees/` must be listed in `.gitignore` — add and commit it before creating the first worktree in a repo
- Use a **random, short branch name** when the user does not specify one (e.g. `wt/<8-hex-chars>`); do not prompt for a branch name

Agent tooling that automates worktree creation should discover these rules from `CLAUDE.md` / `AGENTS.md` (e.g. a `worktree.*director` grep) and honour them without asking.

---

## Commit Messages (Conventional Commits)

```text
<type>(<scope>): <short summary>

[optional body]

[optional footer: Closes #<issue>]
```

**Types:** `feat`, `fix`, `test`, `refactor`, `chore`, `docs`, `ci`, `perf`
**Scope:** module or layer name, e.g. `orders`, `auth`, `infra`, `ui`

```text
feat(orders): add order cancellation endpoint

Implements POST /api/v1/orders/{id}/cancel.
Validates order is in Pending state before cancelling.

Closes #42
```

- Subject line: imperative mood, ≤72 chars, no period
- Body: explain *why*, not *what*
- Breaking changes: add `BREAKING CHANGE:` footer (or `!` after the type)

---

## Pull Request Conventions

### PR Title

Follow Conventional Commits format: `feat(orders): add cancellation endpoint`

### PR Description Template

Body sections: **Summary** · **Changes** · **Testing** (unit, component/integration, E2E, local) · **Checklist** (tests pass, no new vulnerable deps, no secrets, migrations included if schema changed, API/OpenAPI spec still valid).

Template: [`.ai/references/base/pr-description-template.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/base/pr-description-template.md)

### Review Guidelines

- PRs should be small and focused — one concern per PR
- Reviewers check: architecture adherence, test quality, security, no shortcuts that make tests green
- Auto-assign reviewers via `CODEOWNERS`

---

## CI/CD (generic outline)

Pipeline stages: `build` → `test` → `security-scan` → `container-build` → `push`

- Build and test run on every PR
- Vulnerable-dependency scan fails the build on HIGH/CRITICAL
- Container image built and pushed only on `main` after tests pass
- E2E tests run against the built image before it is marked as a release candidate

Concrete CI configuration (GitHub Actions YAML, commands, package scanners) lives in the stack overlay.

---

## Documentation Structure

Repo-root `docs/` contains:

- `design/<feature-name>/` — UI wireframes (`wireframe.md`) & Mermaid flows (`flow.md`) per feature
- `adr/` — Architecture Decision Records
- `ai-notes/` — AI agent working notes

Rules:

- `README.md` and `CHANGELOG.md` live in the repo root
- UI design artifacts are saved per feature during the UI workflow phases
- AI agents write working notes to `docs/ai-notes/`, not `.ai/`
- `.ai/` is reserved for agent instructions and skill files only

Layout: [`.ai/references/base/documentation-structure.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/base/documentation-structure.md)

---

## Security (baseline)

- Transport security enforced (HTTPS + HSTS)
- No secrets in source files or per-environment config files — environment variables or a secrets manager only
- Validate all inputs at system boundaries before any domain logic
- Run a vulnerable-dependency scan in CI — fail the build on HIGH/CRITICAL findings
- Standard security response headers on every HTTP response

Language- and framework-specific enforcement (specific scanners, validation libraries, header mechanisms) lives in the stack overlay.

---

## Agent Guardrails

- Do not install additional packages without asking first
- Do not change the project's target runtime or framework version
- Do not modify build/project files unless the task requires it
- Do not introduce new architectural patterns unless explicitly asked
- Do not touch files outside the scope of the current task
- Keep changes minimal and focused — do not refactor unrelated code unless asked
- Never skip git hooks (`--no-verify`) unless the user explicitly asks
- Never commit secrets or credential files

Stack-specific guardrails (e.g. "do not add NuGet packages") live in the stack overlay.

---

## Project Scaffold Checklist (baseline)

Init-time checklist (every project, regardless of stack) — including baseline, .NET, and WebAPI layers — lives at [`.ai/references/scaffold-checklists.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/scaffold-checklists.md). Stack-specific additions are in the same file under their respective sections.

[//]: # (GENERATED FILE — do not edit directly. Source: .ai/stacks/_partials/dotnet-core.md + .ai/stacks/_layers/dotnet-webapi.md. Run scripts/build-stacks.sh to regenerate.)

[//]: # (Stack partial — shared .NET conventions. Composed with a layer file under .ai/stacks/_layers/ by `scripts/build-stacks.sh` to produce a flat .ai/stacks/dotnet-*.md. Do not edit the generated file directly.)

# .NET Core Conventions

Shared baseline for every .NET stack overlay. Composed with a layer file (`dotnet-blazor` or `dotnet-webapi`) into the published flat overlay.

---

## Tech Stack (.NET baseline)

.NET 10 / C# · ASP.NET Core Minimal API · EF Core (SQLite small / PostgreSQL non-small) · FluentValidation · Serilog · OpenTelemetry · OpenAPI + Scalar · Docker + docker-compose (Alpine) · xUnit + FluentAssertions + NSubstitute.

Full table: [`.ai/references/dotnet/tech-stack.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/tech-stack.md)

---

## Architecture — Modular Monolith

- Separate top-level folders per module: `src/Modules/<ModuleName>/`
- Each module owns its Domain / Application / Infrastructure layers
- Modules communicate via in-process interfaces — never direct project references across modules
- Shared kernel in `src/Shared/` for cross-cutting types only
- Modules register their own DI services via `IServiceCollection` extension methods
- Apply Hexagonal (Ports & Adapters) inside a module when it has multiple infrastructure adapters (e.g. REST + messaging) or needs strong testability isolation

Directory layouts (modular-monolith and hexagonal): [`.ai/references/dotnet/architecture-layout.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/architecture-layout.md)

---

## C# Conventions

`Directory.Build.props` at repo root pins (mandatory): `TargetFramework=net10.0`, `Nullable=enable`, `ImplicitUsings=enable`, `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, `AnalysisLevel=latest-recommended`, `DebugType=embedded`, `DebugSymbols=true`. Full file: [`.ai/references/dotnet/directory-build-props.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/directory-build-props.md)

- File-scoped namespaces always
- `global using` for framework namespaces in each project
- `record` types for DTOs and value objects
- `sealed` by default on non-base classes
- No `var` when the type is not obvious from the right-hand side
- Prefer primary constructors (.NET 8+)
- Central Package Management via `Directory.Packages.props` — no versions in `.csproj`
- Use `ILogger<T>` for logging — never `Console.WriteLine`
- Use specific exception types — not generic `catch (Exception)`
- Use `CancellationToken` in all async methods that call external resources
- Use `async`/`await` end-to-end — never `Task.Result` or `.GetAwaiter().GetResult()`
- No `#nullable disable` or warning suppressions to fix build errors
- Never suppress nullable warnings with `!` without a clear comment

---

## API Design — Minimal API baseline

Every ASP.NET Core project (whether it exposes a REST surface or just a few endpoints for a Blazor app) follows these baseline conventions. The `dotnet-webapi` layer adds the deeper REST conventions on top.

- All endpoints grouped by module via `IEndpointRouteBuilder` extension methods
- One handler per file when the body is non-trivial; inline lambdas only for true one-liners
- Input validation via FluentValidation, run at the boundary before any handler logic
- Error responses are always `ProblemDetails` (RFC 9457) — never raw strings, anonymous error objects, or HTML error pages
- OpenAPI via `Microsoft.AspNetCore.OpenApi`; Scalar UI mounted at `/scalar`

Scaffold: [`.ai/references/dotnet/endpoint-group.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/endpoint-group.md)

---

## Entity Framework Core

- One `DbContext` per module (not one global context)
- Migrations in `<Module>/Infrastructure/Persistence/Migrations/`
- `IEntityTypeConfiguration<T>` per entity — no data annotations on domain models
- Never use `EF.Functions` in domain/application layers — only in infrastructure queries
- Always use `AsNoTracking()` for read-only queries
- Seed data via `IEntityTypeConfiguration.HasData()` or a dedicated seeder run at startup

CLI scaffold: [`.ai/references/dotnet/ef-core-cli.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/ef-core-cli.md)

---

## Localization & Regional Formatting (server-side baseline)

Base rules for `de` / `en` support and regional formatting live in `base-instructions.md`. For every ASP.NET Core project on this stack:

- Configure `RequestLocalizationMiddleware` in `Program.cs` with supported cultures `de-CH, de-DE, de-AT, en-US, en-GB` and default `de-CH` / `de`
- Culture resolution order: cookie (`.AspNetCore.Culture`) → `Accept-Language` header → default (`de-CH` / `de`)
- For language `de` with no recognized region (or a `de-*` region not in `SupportedCultures`), fall back to `de-CH` — never `de-DE`
- Format dates / numbers / currency via `CurrentCulture` — never `string.Format` with a hardcoded culture or `CultureInfo.InvariantCulture` for user-visible text

Middleware scaffold: [`.ai/references/dotnet/request-localization.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/request-localization.md)

UI-specific localization rules (resource files for component strings, picker behaviour, language-switcher widgets) live in the Blazor layer.

---

## Testing Strategy

The base testing rules (TDD, no test modification to make green, full suite after implementation) live in `base-instructions.md`.

### Test project layout (baseline)

```text
tests/
  <Module>.UnitTests/         ← xUnit, no I/O
  <Module>.IntegrationTests/  ← xUnit, real I/O via Testcontainers
```

Layer-specific test projects (Blazor component tests, Playwright E2E, API integration tests with `WebApplicationFactory`) are added by the layer overlay.

### Unit tests (xUnit)

- One test class per production class
- Naming: `MethodName_StateUnderTest_ExpectedBehavior`
- Use `FluentAssertions` for assertions
- Use `NSubstitute` for mocks/stubs
- No `[Fact]` with logic — use `[Theory]` + `[InlineData]` / `[MemberData]`
- After implementation, run the full test suite (`dotnet test`) — not just the new test

Test class scaffold: [`.ai/references/dotnet/xunit-example.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/xunit-example.md)

---

## Essential Commands

```bash
# Restore / build (warnings as errors) / run
dotnet restore
dotnet build -c Release
dotnet run --project src/Host

# Run full stack locally
docker-compose -f docker-compose.yml -f docker-compose.override.yml up --build

# Tests
dotnet test                                         # all
dotnet test tests/<Module>.UnitTests                # unit only
dotnet test tests/<Module>.IntegrationTests         # integration (needs Docker)
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Security / package checks
dotnet list package --vulnerable --fail-on-severity high
dotnet list package --outdated
```

**PDB symbols:** Release builds embed PDB symbols (`<DebugType>embedded</DebugType>` in `Directory.Build.props`) so production stack traces carry source file + line numbers. Never strip them from release or Docker builds.

---

## Essential just Recipes

Projects ship a repo-root `justfile` ([casey/just](https://github.com/casey/just)) standardizing common commands — canonical recipe names, project-local bodies. Canonical groups: build/run, testing, Docker Compose, quality (`lint`, `outdated`, `vuln`), versioning (`version`, `bump-*`), release (`changelog`, `release`, `package`), `clean`. Document each with a leading `# <description>`; the default recipe runs `just --list --unsorted`.

A reference `justfile` lives at `.ai/examples/dotnet/justfile` — copy it and customize the top-of-file variables. Host-specific recipes ship as `[unix]` + `[windows]` pairs (no WSL needed); tool/project-specific ones (`release-notes`, `package`) ship as stubs with per-OS examples in comments.

Install (just ≥ 1.20): `cargo install just` / `brew install just` / `winget install Casey.Just` / `sudo apt install just`. CI: `extractions/setup-just@v2`.

Full recipe list with descriptions: [`.ai/references/dotnet/justfile-recipes.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/justfile-recipes.md)

---

## Docker

- Runtime base: `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`
- Build base: `mcr.microsoft.com/dotnet/sdk:10.0-alpine`
- Multi-stage Dockerfile always
- Run as non-root user in final stage
- `docker-compose.yml` — production-like config
- `docker-compose.override.yml` — local dev overrides (ports, volumes, hot-reload)
- Secrets via environment variables or Docker secrets — **never in image or appsettings**

Dockerfile scaffold: [`.ai/references/dotnet/dockerfile.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/dockerfile.md)

---

## Logging & Observability

- Serilog configured in `Program.cs` via `UseSerilog()`
- Structured properties on every log entry: `{ModuleName}`, `{CorrelationId}`
- Use `LoggerMessage.Define` source-generated logging for hot paths
- Log levels: `Debug` local, `Information` production minimum
- OpenTelemetry: export traces to OTLP collector; expose `/metrics` (Prometheus format)
- Health checks: `/health/live` (liveness) and `/health/ready` (readiness, checks DB)

**12-Factor enforcement points for this stack:**

- Never write to the local filesystem inside a container for application state
- Never use `appsettings.Development.json` for secrets — always env vars
- EF Core migrations must be applied as a separate init container or pre-deploy step — **never** auto-migrated on `app.Run()`
- Serilog sink in production: stdout or OTLP — never file sink in Docker

---

## Security (stack baseline)

Base security rules live in `base-instructions.md`. For every project on this stack:

- HTTPS enforced in all environments; HSTS enabled
- Security response headers: `X-Content-Type-Options`, `X-Frame-Options`, `Content-Security-Policy`
- No secrets in `appsettings.json` — use `IConfiguration` with environment variable binding
- Run `dotnet list package --vulnerable --fail-on-severity high` in CI — fail build on HIGH/CRITICAL
- Validate all inputs at the API boundary with FluentValidation before any domain logic
- Error responses use `ProblemDetails` (no raw messages)

---

## Versioning (stack binding)

Base rules (SemVer, Conventional Commits → bump mapping, git-cliff) live in `base-instructions.md`. For this stack:

- One global version for all assemblies — defined once in `Directory.Build.props` as `<Version>`, never in individual `.csproj` files
- Docker images tagged with the same version + `latest` on stable releases

---

## CI/CD (GitHub Actions baseline)

Pipeline stages: `build` → `test` → `security-scan` → `docker-build` → `push` (base CI rules apply): build/test on every PR, vuln scan fails on HIGH/CRITICAL, image pushed only on `main` after tests pass.

Layer-specific CI jobs (E2E with Playwright for Blazor, k6 perf smoke for WebAPI) are added by the layer overlay.

Workflow scaffold: [`.ai/references/dotnet/github-actions.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/github-actions.md)

---

## Project Scaffold Checklist (.NET baseline)

.NET-specific init-time checklist (inherits the base checklist) lives at [`.ai/references/scaffold-checklists.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/scaffold-checklists.md) under "**.NET baseline**". Layer additions are in the same file.

---

## Agent Guardrails (.NET baseline)

In addition to the base guardrails:

- Do not install additional NuGet packages without asking first
- Do not change project target frameworks
- Do not modify `.csproj` files unless the task requires it
- Do not introduce new patterns (e.g. MediatR, CQRS) unless explicitly asked

### Never generate (this stack)

- `async void` (except UI event handlers — see the Blazor layer)
- `Task.Result` or `.GetAwaiter().GetResult()` — always `await`
- Magic strings — use `const` or `nameof()`
- Direct `HttpClient` instantiation — always via `IHttpClientFactory`
- Cross-module project references (use shared interfaces)

---

[//]: # (Stack layer — composed with .ai/stacks/_partials/dotnet-core.md by `make build-stacks` to produce .ai/stacks/dotnet-webapi.md. Do not edit the generated file directly.)

# .NET WebAPI Layer

Backend-only ASP.NET Core REST API projects (no Blazor/UI), composed on the shared `dotnet-core` partial.

---

## Tech Stack (WebAPI additions)

REST · ASP.NET Core Minimal API · `Asp.Versioning.Http` (URL-segment) · auth: pass-through / API-key / JWT (single scheme per project) · `ProblemDetails` (RFC 9457) · OpenAPI + Scalar at `/scalar` · Bruno · `WebApplicationFactory` + Testcontainers · k6 · Kiota.

Full table: [`.ai/references/dotnet/tech-stack.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/tech-stack.md)

---

## API Design — Minimal API

- Endpoints grouped by module via `IEndpointRouteBuilder` extension methods
- Route prefix `/api/v{version}/{module}/...` — URL format under *API versioning* below
- One handler per file when the body is non-trivial; inline lambdas only for true one-liners
- FluentValidation runs at the boundary, before any handler logic

Endpoint group scaffold: [`.ai/references/dotnet/endpoint-group.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet/endpoint-group.md) (use the versioned route variant)

### HTTP status code conventions

Non-obvious rules: `201 Created` and `202 Accepted` must include a `Location` header (to the new resource / status resource respectively); use `422` (not `400`) for semantic validation failures (body parsed OK, content invalid); `429` must include `Retry-After`.

### HTTP GET with request body — forbidden for new endpoints

GET bodies have undefined semantics (RFC 9110) — proxies and caches may drop them. New endpoints: use query params, or `POST /search` for large/sensitive filter sets. Legacy: allowed for backwards-compat only; mark `[Obsolete]` and emit a `Sunset` header.

### Errors — always ProblemDetails

- Every error response — including from middleware and model binding — is RFC 9457 `ProblemDetails`
- Never return raw strings, anonymous `{ error: "..." }` objects, or HTML error pages
- Populate `type`, `title`, `status`, `detail`, `instance`; add a `traceId` extension from the current `Activity.TraceId`

Registration scaffold: [`.ai/references/dotnet-webapi/problem-details.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/problem-details.md)

---

## API Versioning

`Asp.Versioning.Http` with **URL-segment** versioning. Format `v1.0`, `v2.0`, `v2.1` (`MAJOR.MINOR`). The minor segment stays in the URL even when only the major bumps, keeping the URL shape stable across the API's lifetime.

- **Unversioned URLs (`/api/orders/...`) are allowed only for backward compatibility** — they resolve to v1.0 explicitly, never "latest". Rolling out v2.0 must not change what an unversioned caller hits.
- Deprecate with `.HasDeprecatedApiVersion(1.0)` plus a `Sunset: <RFC 7231 date>` response header.
- Removal is separate from deprecation — no version is removed without an announced sunset window.

Registration scaffold: [`.ai/references/dotnet-webapi/api-versioning.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/api-versioning.md)

---

## Authentication

**One scheme per API project**, chosen at bootstrap and applied to every endpoint — never mixed. Three approved schemes (full rules in the reference doc):

- **Pass-through** (BFF / wrapper APIs): forward `Authorization` upstream verbatim; do not validate, decode, log, or call `AddAuthentication()`. Any non-proxied endpoint disqualifies the project from pass-through.
- **API key** (`X-API-Key` header, no query-string fallback): custom handler, keys in secret store, constant-time compare (`CryptographicOperations.FixedTimeEquals`), accept a small rotating set.
- **JWT bearer**: validate issuer/audience/lifetime/signing key in every environment (no exceptions, including local); authorize via named policies, not raw roles. This API **consumes** tokens — issuance belongs in a dedicated identity service.

Cross-cutting:

- `[Authorize]` / `.RequireAuthorization()` is the default for API key + JWT projects; opt out per-endpoint with `[AllowAnonymous]`. Pass-through projects register no scheme.
- Anonymous endpoints are limited to `/health/*`, `/scalar`, and the OpenAPI document.

Full per-scheme rules: [`.ai/references/dotnet-webapi/authentication-schemes.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/authentication-schemes.md)

---

## Pagination

**Default to cursor-based** for new endpoints (offset is unstable under concurrent inserts): `GET .../orders?pageSize=50&pageToken=<opaque>` → `{ "items": [...], "nextPageToken": "<opaque>" }` (null when exhausted).

- `pageToken` is opaque base64 of an internal cursor (`{lastId, lastCreatedAt}`), never a row offset
- `pageSize` bounded server-side; over-limit requests return `400`
- Offset pagination only for small bounded admin lists where insert-stability is guaranteed

---

## Idempotency for unsafe methods

Accept `Idempotency-Key` on `POST`/`PATCH` (and `DELETE` if it has side-effects beyond removing a row).

- Cache the response keyed by `(route, key, principal)` for 24 h
- Same key → cached response returned (no duplicate side-effect, no second `201`)
- Same key but a *different* request body → `409 Conflict`
- Keys are client-supplied opaque strings; the API never generates them

---

## Optimistic concurrency

For mutable resources, surface the row version as an `ETag` and require `If-Match` on writes.

- `GET /resources/{id}` returns `ETag: "<rowversion>"`
- `PUT|PATCH|DELETE /resources/{id}` accepts `If-Match: "<rowversion>"` — present + mismatch → `412 Precondition Failed`; absent → write proceeds (lenient default; clients opt in by sending the header)
- EF Core: `[Timestamp] public byte[] RowVersion { get; set; }`; the handler maps `DbUpdateConcurrencyException` to `412`

---

## Rate limiting

- Named policies per endpoint group — never a single global limit
- Always emit `Retry-After` on `429`
- Partition by authenticated principal first; fall back to remote IP only for anonymous endpoints

Registration scaffold: [`.ai/references/dotnet-webapi/rate-limiting.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/rate-limiting.md)

---

## CORS

- Explicit origin allowlist per environment via `WithOrigins(...)`
- **Never** combine `AllowAnyOrigin()` with `AllowCredentials()` — browsers reject it; it signals a misconfiguration
- Scope methods and headers to what the API accepts — no blanket `AllowAnyMethod()`
- Preflight cache via `SetPreflightMaxAge(TimeSpan.FromHours(1))`

---

## HTTP logging

**Never log** `Authorization`, `Cookie`, `Set-Cookie`, `X-API-Key`, or any header that may carry credentials. Calling `RequestHeaders.Clear()` before adding a curated allowlist is **mandatory** — the framework defaults include sensitive headers.

Registration scaffold: [`.ai/references/dotnet-webapi/http-logging.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/http-logging.md)

---

## Long-running operations

For work too long for one request: the kickoff `POST` returns `202 Accepted` + `Location: /api/v1.0/operations/{opId}`; polling `GET` returns `200 OK` (`running | succeeded | failed`) in progress, then `303 See Other` + `Location: <result-resource>` on completion. Retain operations ≥ 24 h so clients can observe the terminal state.

---

## Response compression

- Brotli first, gzip fallback
- Exclude already-compressed media types (`image/*`, `application/zip`, `application/x-protobuf`, etc.) — wasted CPU otherwise

Registration scaffold: [`.ai/references/dotnet-webapi/response-compression.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/response-compression.md)

---

## OpenAPI & Scalar

- API metadata (Title / Version / Description / Contact / License) is mandatory — published APIs without it are rejected in review
- Scalar UI at `/scalar`; OpenAPI document at `/openapi/v1.0.json`
- Code samples enabled for **bash curl** and **PowerShell** at minimum; other clients opt-in
- Deprecated endpoints carry the OpenAPI `deprecated: true` flag *and* return a `Sunset` response header

Registration scaffold: [`.ai/references/dotnet-webapi/openapi-scalar.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/openapi-scalar.md)

---

## Client SDK generation

**Kiota** is the default for first-party `.NET` and `TypeScript` consumers:

```bash
kiota generate -l CSharp -d https://api.example.com/openapi/v1.0.json -o ./clients/dotnet -n Acme.Orders.Client
```

- Other languages consume the OpenAPI document directly
- Do **not** introduce NSwag, Refit, AutoREST, or hand-rolled `HttpClient` wrappers without an explicit ask — the OpenAPI document is the contract

---

## Testing (WebAPI additions)

Unit-test conventions and the baseline `<Module>.UnitTests` / `<Module>.IntegrationTests` layout live in the `dotnet-core` partial. For WebAPI, the integration project uses `WebApplicationFactory` + Testcontainers, plus one optional contract project:

```text
tests/
  Api.ContractTests/          ← optional — pinned OpenAPI snapshot
```

No bUnit, no Playwright — those are Blazor-stack concerns.

### Integration tests — WebApplicationFactory + Testcontainers

- `WebApiFactory : WebApplicationFactory<Program>` swaps real infrastructure for Testcontainers (Postgres, Redis, etc.)
- Each test class owns its database via Testcontainers — no shared mutable state across classes
- Auth in tests: register a test scheme injecting a known principal — never call the real identity provider

Test class scaffold: [`.ai/references/dotnet-webapi/integration-test.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/integration-test.md)

### Manual / exploratory testing — Bruno

Collections in `bruno/`, one folder per module, committed to Git. Base URLs and tokens come from Bruno environments — never hardcoded. When an endpoint changes, update its Bruno request in the same PR with realistic bodies and useful assertions.

Layout + naming: [`.ai/references/dotnet-webapi/bruno-layout.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/bruno-layout.md)

### Performance / load testing — k6

Scripts in `perf/`, one scenario per critical journey or hot endpoint. Naming: `<endpoint-or-journey>.<profile>.js`, profile ∈ `smoke | load | stress | soak`. Every script declares `thresholds` for `http_req_duration` and `http_req_failed` — a failed threshold fails CI. Env via `K6_BASE_URL`; auth via `perf/lib/` helpers — never hardcoded. CI: smoke blocks every PR; load / stress / soak on demand.

Layout + sample script + profile defs: [`.ai/references/dotnet-webapi/k6-scenarios.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/dotnet-webapi/k6-scenarios.md)

---

## Project Scaffold Checklist (WebAPI additions)

WebAPI-specific init-time checklist (inherits the base + .NET checklists) lives at [`.ai/references/scaffold-checklists.md`](https://github.com/freaxnx01/ai-instructions/blob/main/.ai/references/scaffold-checklists.md) under "**.NET WebAPI**".

---

## Agent Guardrails (WebAPI additions)

Every rule in this layer is enforced as written above. One additional guardrail:

- Do not create POST or PATCH endpoints without considering whether `Idempotency-Key` should be supported
