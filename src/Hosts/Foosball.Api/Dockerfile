# Build context is the repository root:
#   docker build -f src/Hosts/Foosball.Api/Dockerfile --build-arg BUILD_VERSION=1.2.3 .

#
# Build and publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Becomes the assembly version, logged by Serilog as ApplicationVersion.
ARG BUILD_VERSION=0.0.0
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Simple on purpose: any code change re-runs the restore. For a better layer cache, COPY only the
# .csproj and packages.lock.json files (global.json, nuget.config, Directory.Build.props and
# src/Directory.Packages.props too) of the host and every project it references, restore, then COPY src/.
COPY global.json nuget.config Directory.Build.props ./
COPY src/ src/
RUN dotnet restore src/Hosts/Foosball.Api/Foosball.Api.csproj --locked-mode
RUN dotnet publish src/Hosts/Foosball.Api/Foosball.Api.csproj -c Release --no-restore -o /app -p:UseAppHost=false -p:Version="$BUILD_VERSION"

#
# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# SQLite lives in /data: create it owned by the non-root user so a mounted volume is writable.
USER root
# curl is for the compose healthcheck (/health/ready), the aspnet image does not ship it.
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/* \
    && mkdir /data && chown $APP_UID /data
USER $APP_UID
EXPOSE 8080
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ModularMonolith.Foosball.Api.dll"]
