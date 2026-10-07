# Build context is the repository root:
#   docker build -f src/Hosts/Foosball.Migrator/Dockerfile --build-arg BUILD_VERSION=1.2.3 .

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
RUN dotnet restore src/Hosts/Foosball.Migrator/Foosball.Migrator.csproj --locked-mode
RUN dotnet publish src/Hosts/Foosball.Migrator/Foosball.Migrator.csproj -c Release --no-restore -o /app -p:UseAppHost=false -p:Version="$BUILD_VERSION"

#
# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
# SQLite lives in /data: create it owned by the non-root user so a mounted volume is writable.
USER root
RUN mkdir /data && chown $APP_UID /data
USER $APP_UID
COPY --from=build /app .
ENTRYPOINT ["dotnet", "ModularMonolith.Foosball.Migrator.dll"]
