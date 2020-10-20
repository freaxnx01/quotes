FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Copy additional files
COPY src/Views out/Views
COPY src/appsettings.json out/appsettings.json
COPY src/quote.db out/quote.db

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-alpine
EXPOSE 80

# alias for root
RUN echo "alias ll='ls -l --color=auto --human-readable'" >> /root/.bashrc && echo "alias ls='ls --color=auto'" >> /root/.bashrc && echo "alias ..='cd ..'" >> /root/.bashrc

WORKDIR /app
COPY --from=build-env /app/out .

# supervisor
RUN apt-get update
RUN apt-get -y install supervisor
COPY supervisord.conf /etc/supervisor/supervisord.conf

COPY init.sh /
RUN ["chmod", "+x", "/init.sh"]
ENTRYPOINT ["/init.sh"]