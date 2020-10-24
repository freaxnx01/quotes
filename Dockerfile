FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY *.sln ./
COPY Data/*.csproj ./Data/
COPY ConsoleApp/*.csproj ./ConsoleApp/
COPY WebApplication/*.csproj ./WebApplication/

RUN dotnet restore -r linux-musl-x64
COPY . .

WORKDIR /src/Data
RUN dotnet build -c Release -r linux-musl-x64 -o out

WORKDIR /src/ConsoleApp
RUN dotnet build -c Release -r linux-musl-x64 -o out

WORKDIR /src/WebApplication
RUN dotnet build -c Release -r linux-musl-x64 -o out

FROM build AS publish
RUN dotnet publish -c Release -r linux-musl-x64 -o out
#-p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true

FROM base AS final
WORKDIR /app
COPY --from=publish /src/WebApplication/out .
# COPY --from=publish /app .
# ENTRYPOINT ["dotnet", "WebAPIProject.dll"]
ENTRYPOINT ["/app/WebApplication"]