FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY *.sln ./
COPY Data/*.csproj ./Data/
COPY WebApplication/*.csproj ./WebApplication/

RUN dotnet restore -r linux-musl-x64
COPY . .
WORKDIR /src/Data
RUN dotnet build -c Release -r linux-musl-x64 -o out --no-restore

WORKDIR /src/WebApplication
RUN dotnet build -c Release -r linux-musl-x64 -o out --no-restore

FROM build AS publish
RUN dotnet publish -c Release -r linux-musl-x64 -o out --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
# ENTRYPOINT ["dotnet", "WebAPIProject.dll"]
ENTRYPOINT ["/app/quotes"]