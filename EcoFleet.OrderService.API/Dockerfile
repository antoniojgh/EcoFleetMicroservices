FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["src/Services/OrderService/EcoFleet.OrderService.API/EcoFleet.OrderService.API.csproj", "Services/OrderService/EcoFleet.OrderService.API/"]
COPY ["src/Services/OrderService/EcoFleet.OrderService.Application/EcoFleet.OrderService.Application.csproj", "Services/OrderService/EcoFleet.OrderService.Application/"]
COPY ["src/Services/OrderService/EcoFleet.OrderService.Domain/EcoFleet.OrderService.Domain.csproj", "Services/OrderService/EcoFleet.OrderService.Domain/"]
COPY ["src/Services/OrderService/EcoFleet.OrderService.Infrastructure/EcoFleet.OrderService.Infrastructure.csproj", "Services/OrderService/EcoFleet.OrderService.Infrastructure/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Domain/EcoFleet.BuildingBlocks.Domain.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Application/EcoFleet.BuildingBlocks.Application.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/EcoFleet.BuildingBlocks.Contracts.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/"]

RUN dotnet restore "Services/OrderService/EcoFleet.OrderService.API/EcoFleet.OrderService.API.csproj"

# Copy all source
COPY src/ .

RUN dotnet publish "Services/OrderService/EcoFleet.OrderService.API/EcoFleet.OrderService.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoFleet.OrderService.API.dll"]
