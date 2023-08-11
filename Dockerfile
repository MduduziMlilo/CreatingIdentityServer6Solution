# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /src

# Restore packages.
COPY **/*.csproj ./
RUN find . -name "*.csproj" -type f -exec dotnet restore {} \;

# Publish project artifacts.
COPY . .
RUN find . -name "*.csproj" -type f -exec dotnet publish {} -c Release -o /src/publish/$(dirname {}) \;

# Build runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /app

# Copy artifacts to runtime environment.
COPY --from=build-env /src/publish ./

# Execute command on container startup.
ENTRYPOINT ["dotnet", "IdentityServer.dll"]


