FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["src/Gateway/EcoFleet.ApiGateway/EcoFleet.ApiGateway.csproj", "Gateway/EcoFleet.ApiGateway/"]

RUN dotnet restore "Gateway/EcoFleet.ApiGateway/EcoFleet.ApiGateway.csproj"

# Copy all source
COPY src/ .

RUN dotnet publish "Gateway/EcoFleet.ApiGateway/EcoFleet.ApiGateway.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoFleet.ApiGateway.dll"]
