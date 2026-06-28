# Quotes

A small ASP.NET Core web application that serves and manages quotes.

🌐 **Live:** https://freaxnx01.ch/

## Projects

- **WebApplication** — ASP.NET Core MVC web frontend (with Identity-based admin) and a small API.
- **DataModel** — Entity Framework Core data model (`Quote`) and `DbContext`.
- **ConsoleApp** — console utility for working with the quotes data.

## Running

### Docker

```bash
docker compose up
```

The app is then available at http://localhost:8123.

### Local

```bash
dotnet run --project WebApplication
```

## Stack

- .NET (currently net5.0)
- ASP.NET Core MVC + Identity
- Entity Framework Core (SQLite)
