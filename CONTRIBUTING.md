# Contributing

Thanks for contributing to Quotes.

## Build

```bash
dotnet build Quotes.sln -c Release
```

## Run (Docker)

```bash
docker build -t quotes .
docker run -p 8080:80 -e ASPNETCORE_URLS=http://+:80 quotes
```

## Notes

- The app self-creates its SQLite databases on first run.
- The admin user is seeded from the `AdminUserSettings` config section.

All changes go through a PR against `master`.
