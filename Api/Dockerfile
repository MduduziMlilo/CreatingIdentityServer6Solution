# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /src

# Restore packages.
COPY Api/Api.csproj ./
RUN dotnet restore Api.csproj;

# Publish project artifacts.
COPY Api/ ./
RUN dotnet publish Api.csproj -c Release -o /src/publish;

# Build runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /app

# Copy artifacts to runtime environment.
COPY --from=build-env /src/publish ./

# Execute command on container startup.
ENTRYPOINT ["dotnet", "Api.dll"]
