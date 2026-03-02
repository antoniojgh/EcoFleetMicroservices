FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["src/Services/ManagerService/EcoFleet.ManagerService.API/EcoFleet.ManagerService.API.csproj", "Services/ManagerService/EcoFleet.ManagerService.API/"]
COPY ["src/Services/ManagerService/EcoFleet.ManagerService.Application/EcoFleet.ManagerService.Application.csproj", "Services/ManagerService/EcoFleet.ManagerService.Application/"]
COPY ["src/Services/ManagerService/EcoFleet.ManagerService.Domain/EcoFleet.ManagerService.Domain.csproj", "Services/ManagerService/EcoFleet.ManagerService.Domain/"]
COPY ["src/Services/ManagerService/EcoFleet.ManagerService.Infrastructure/EcoFleet.ManagerService.Infrastructure.csproj", "Services/ManagerService/EcoFleet.ManagerService.Infrastructure/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Domain/EcoFleet.BuildingBlocks.Domain.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Application/EcoFleet.BuildingBlocks.Application.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Application/"]

RUN dotnet restore "Services/ManagerService/EcoFleet.ManagerService.API/EcoFleet.ManagerService.API.csproj"

# Copy all source
COPY src/ .

RUN dotnet publish "Services/ManagerService/EcoFleet.ManagerService.API/EcoFleet.ManagerService.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoFleet.ManagerService.API.dll"]
