FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["src/Services/DriverService/EcoFleet.DriverService.API/EcoFleet.DriverService.API.csproj", "Services/DriverService/EcoFleet.DriverService.API/"]
COPY ["src/Services/DriverService/EcoFleet.DriverService.Application/EcoFleet.DriverService.Application.csproj", "Services/DriverService/EcoFleet.DriverService.Application/"]
COPY ["src/Services/DriverService/EcoFleet.DriverService.Domain/EcoFleet.DriverService.Domain.csproj", "Services/DriverService/EcoFleet.DriverService.Domain/"]
COPY ["src/Services/DriverService/EcoFleet.DriverService.Infrastructure/EcoFleet.DriverService.Infrastructure.csproj", "Services/DriverService/EcoFleet.DriverService.Infrastructure/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Domain/EcoFleet.BuildingBlocks.Domain.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Application/EcoFleet.BuildingBlocks.Application.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/EcoFleet.BuildingBlocks.Contracts.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/"]

RUN dotnet restore "Services/DriverService/EcoFleet.DriverService.API/EcoFleet.DriverService.API.csproj"

# Copy all source
COPY src/ .

RUN dotnet publish "Services/DriverService/EcoFleet.DriverService.API/EcoFleet.DriverService.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoFleet.DriverService.API.dll"]
