FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["src/Services/NotificationService/EcoFleet.NotificationService.API/EcoFleet.NotificationService.API.csproj", "Services/NotificationService/EcoFleet.NotificationService.API/"]
COPY ["src/Services/NotificationService/EcoFleet.NotificationService.Application/EcoFleet.NotificationService.Application.csproj", "Services/NotificationService/EcoFleet.NotificationService.Application/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/EcoFleet.BuildingBlocks.Contracts.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/"]

RUN dotnet restore "Services/NotificationService/EcoFleet.NotificationService.API/EcoFleet.NotificationService.API.csproj"

# Copy all source
COPY src/ .

RUN dotnet publish "Services/NotificationService/EcoFleet.NotificationService.API/EcoFleet.NotificationService.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoFleet.NotificationService.API.dll"]
