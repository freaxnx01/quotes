FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
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
RUN dotnet publish -c Release -r linux-musl-x64 -o out
#-p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true

FROM base AS final
WORKDIR /app
COPY --from=publish /src/WebApplication/out .
RUN mv out/* .. && rmdir out

# data-default
COPY WebApplication/data-default/* ./data-default/

# data
RUN mkdir /data
RUN ln -s /data data

ENTRYPOINT ["/app/WebApplication"]