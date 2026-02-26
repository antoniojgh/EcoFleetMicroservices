FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["src/Services/AssignmentService/EcoFleet.AssignmentService.API/EcoFleet.AssignmentService.API.csproj", "Services/AssignmentService/EcoFleet.AssignmentService.API/"]
COPY ["src/Services/AssignmentService/EcoFleet.AssignmentService.Application/EcoFleet.AssignmentService.Application.csproj", "Services/AssignmentService/EcoFleet.AssignmentService.Application/"]
COPY ["src/Services/AssignmentService/EcoFleet.AssignmentService.Domain/EcoFleet.AssignmentService.Domain.csproj", "Services/AssignmentService/EcoFleet.AssignmentService.Domain/"]
COPY ["src/Services/AssignmentService/EcoFleet.AssignmentService.Infrastructure/EcoFleet.AssignmentService.Infrastructure.csproj", "Services/AssignmentService/EcoFleet.AssignmentService.Infrastructure/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Domain/EcoFleet.BuildingBlocks.Domain.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Application/EcoFleet.BuildingBlocks.Application.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/EcoFleet.BuildingBlocks.Contracts.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/"]

RUN dotnet restore "Services/AssignmentService/EcoFleet.AssignmentService.API/EcoFleet.AssignmentService.API.csproj"

# Copy all source
COPY src/ .

RUN dotnet publish "Services/AssignmentService/EcoFleet.AssignmentService.API/EcoFleet.AssignmentService.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoFleet.AssignmentService.API.dll"]
