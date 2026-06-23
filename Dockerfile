FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY *.sln ./
COPY DataModel/*.csproj ./DataModel/
COPY ConsoleApp/*.csproj ./ConsoleApp/
COPY WebApplication/*.csproj ./WebApplication/

RUN dotnet restore -r linux-musl-x64
COPY . .

WORKDIR /src/DataModel
RUN dotnet build -c Release -r linux-musl-x64 -o out

WORKDIR /src/ConsoleApp
RUN dotnet build -c Release -r linux-musl-x64 -o out

WORKDIR /src/WebApplication
RUN dotnet build -c Release -r linux-musl-x64 -o out

FROM build AS publish
# --self-contained is explicit: since .NET 7 `-r` no longer implies it (.NET 5 did),
# and the runtime-deps base ships no .NET runtime.
RUN dotnet publish -c Release -r linux-musl-x64 --self-contained true -o out
#-p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true

FROM base AS final
WORKDIR /app
COPY --from=publish /src/WebApplication/out ./

# data (DBs are created on first run by the app; persisted via this volume mount)
RUN mkdir /data && ln -s /data data

ENTRYPOINT ["/app/WebApplication"]