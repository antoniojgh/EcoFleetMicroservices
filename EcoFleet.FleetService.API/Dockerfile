FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["src/Services/FleetService/EcoFleet.FleetService.API/EcoFleet.FleetService.API.csproj", "Services/FleetService/EcoFleet.FleetService.API/"]
COPY ["src/Services/FleetService/EcoFleet.FleetService.Application/EcoFleet.FleetService.Application.csproj", "Services/FleetService/EcoFleet.FleetService.Application/"]
COPY ["src/Services/FleetService/EcoFleet.FleetService.Domain/EcoFleet.FleetService.Domain.csproj", "Services/FleetService/EcoFleet.FleetService.Domain/"]
COPY ["src/Services/FleetService/EcoFleet.FleetService.Infrastructure/EcoFleet.FleetService.Infrastructure.csproj", "Services/FleetService/EcoFleet.FleetService.Infrastructure/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Domain/EcoFleet.BuildingBlocks.Domain.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Application/EcoFleet.BuildingBlocks.Application.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/EcoFleet.BuildingBlocks.Contracts.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/"]

RUN dotnet restore "Services/FleetService/EcoFleet.FleetService.API/EcoFleet.FleetService.API.csproj"

# Copy all source
COPY src/ .

RUN dotnet publish "Services/FleetService/EcoFleet.FleetService.API/EcoFleet.FleetService.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoFleet.FleetService.API.dll"]
