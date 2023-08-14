# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /src

# Restore packages.
COPY IdentityServer/IdentityServer.csproj ./
RUN dotnet restore IdentityServer.csproj;

# Publish project artifacts.
COPY IdentityServer/ ./
RUN dotnet publish IdentityServer.csproj -c Release -o /src/publish;

# Build runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /app

# Copy artifacts to runtime environment.
COPY --from=build-env /src/publish ./

# Execute command on container startup.
ENTRYPOINT ["dotnet", "IdentityServer.dll"]
