# Context
## What You Have Today (Current Architecture Analysis)
Your EcoFleetWebAPI is a well-structured monolith built on .NET 10 following Clean Architecture + DDD principles:
```
EcoFleetWebAPI (Monolith)
├── EcoFleet.API              → ASP.NET Core Web API (Controllers, Middleware)
├── EcoFleet.Application      → Use Cases (CQRS with MediatR 14, FluentValidation 12)
├── EcoFleet.Domain           → Entities, Value Objects, Domain Events, Enums
├── EcoFleet.Infrastructure   → EF Core 10 + SQL Server, Repositories, Outbox Pattern
├── EcoFleet.Emails           → SMTP Notification Service
├── EcoFleet.Domain.UnitTests → Domain unit tests
└── EcoFleet.Application.UnitTests → Application unit tests
```

### Identified Bounded Contexts (5 Aggregate Roots):

| Aggregate Root | Key Properties | Domain Events |
|---|---|---|
| Vehicle | Plate, Status (Idle/Active/Maintenance), Geolocation, CurrentDriverId | VehicleDriverAssignedEvent, VehicleMaintenanceStartedEvent |
| Driver | FullName, DriverLicense, Email, PhoneNumber, Status (Available/OnDuty/Suspended) | DriverSuspendedEvent, DriverReinstatedEvent |
| Manager | FullName, Email | (none) |
| ManagerDriverAssignment | ManagerId, DriverId, IsActive | (none) |
| Order | DriverId, Status (Pending/InProgress/Completed/Cancelled), Geolocations, Price | (none) |

### Existing Patterns Already In Place:

- CQRS with MediatR (Commands/Queries separated)
- Domain Events via `IDomainEvent : INotification`
- Outbox Pattern (OutboxMessage table + OutboxProcessor BackgroundService)
- Strongly Typed IDs (DriverId, VehicleId, etc.)
- Unit of Work with domain events → outbox conversion
- Generic Repository Pattern
- FluentValidation Pipeline Behavior
- Logging Pipeline Behavior with Serilog
- Global Exception Handler Middleware

### Cross-Aggregate Coupling Points Detected:

- `CreateVehicleHandler` — reads Driver repository to validate/assign driver
- `CreateOrderHandler` — reads Driver repository to validate driver exists
- `ManagerDriverAssignment` — references both ManagerId and DriverId
- Vehicle has `CurrentDriverId`, Driver has `CurrentVehicleId` (bidirectional reference)

---

# Target Architecture
```
                        ┌──────────────────────┐
                        │    API Gateway        │
                        │   (YARP / Ocelot)     │
                        └──────┬───────────────┘
                               │
          ┌────────────┬───────┼────────┬──────────────┐
          │            │       │        │              │
   ┌──────▼──────┐ ┌───▼───┐ ┌▼──────┐ ┌▼────────┐ ┌──▼───────────┐
   │  Fleet Svc  │ │Driver │ │Manager│ │Order    │ │ Assignment   │
   │ (Vehicles)  │ │  Svc  │ │  Svc  │ │  Svc    │ │    Svc       │
   └──────┬──────┘ └───┬───┘ └┬──────┘ └┬────────┘ └──┬───────────┘
          │            │      │         │              │
          │       ┌────▼──────▼─────────▼──────────────▼───┐
          │       │          RabbitMQ (MassTransit)         │
          │       │     Integration Events + Event Store    │
          │       └────────────────────────────────────────┘
          │
   ┌──────▼──────────────────────────────────┐
   │  Individual SQL Server DBs per Service  │
   │  + Marten Event Store (PostgreSQL)      │
   └─────────────────────────────────────────┘
```

## Technology Stack (.NET 10 — Latest as of 2026)

| Concern | Technology | NuGet Package |
|---|---|---|
| Framework | .NET 10 / ASP.NET Core 10 | Microsoft.NET.Sdk.Web |
| Message Broker | RabbitMQ via MassTransit 9 | MassTransit.RabbitMQ |
| Event Sourcing | Marten 7 (on PostgreSQL) | Marten, Marten.AspNetCore |
| CQRS | MediatR 14 (already in use) | MediatR |
| API Gateway | YARP (Yet Another Reverse Proxy) | Yarp.ReverseProxy |
| Service Discovery | Aspire Service Defaults / Consul | Aspire.Hosting |
| Observability | OpenTelemetry + Aspire Dashboard | OpenTelemetry.Extensions.Hosting |
| ORM (Read Models) | EF Core 10 (already in use) | Microsoft.EntityFrameworkCore |
| Validation | FluentValidation 12 (already in use) | FluentValidation.DependencyInjectionExtensions |
| Containers | Docker + Docker Compose | — |
| Orchestration | .NET Aspire AppHost | Aspire.Hosting.AppHost |
| Health Checks | ASP.NET Core Health Checks | AspNetCore.HealthChecks.RabbitMQ, AspNetCore.HealthChecks.NpgSql |
| Logging | Serilog (already in use) | Serilog.AspNetCore |

---

# STEP-BY-STEP GUIDE

## PHASE 1: PREPARATION (Solution Restructuring)

### Step 1.1 — Create the New Solution Structure in Visual Studio 2026

File → New → Blank Solution → name it `EcoFleet.Microservices`
Right-click Solution → Add → New Solution Folder for each:

```
EcoFleet.Microservices.sln
│
├── 📁 src/
│   ├── 📁 BuildingBlocks/                          ← Shared kernel
│   │   ├── EcoFleet.BuildingBlocks.Domain          ← Entity<T>, ValueObject, IDomainEvent, IAggregateRoot
│   │   ├── EcoFleet.BuildingBlocks.Application     ← IUnitOfWork, PaginatedDTO, Behaviors, Exceptions
│   │   ├── EcoFleet.BuildingBlocks.Infrastructure  ← Base Repository, Outbox, Marten config
│   │   └── EcoFleet.BuildingBlocks.Contracts       ← Integration Events (shared DTOs for messaging)
│   │
│   ├── 📁 Services/
│   │   ├── 📁 FleetService/                        ← Vehicle Microservice
│   │   │   ├── EcoFleet.FleetService.Domain
│   │   │   ├── EcoFleet.FleetService.Application
│   │   │   ├── EcoFleet.FleetService.Infrastructure
│   │   │   └── EcoFleet.FleetService.API
│   │   │
│   │   ├── 📁 DriverService/                       ← Driver Microservice
│   │   │   ├── EcoFleet.DriverService.Domain
│   │   │   ├── EcoFleet.DriverService.Application
│   │   │   ├── EcoFleet.DriverService.Infrastructure
│   │   │   └── EcoFleet.DriverService.API
│   │   │
│   │   ├── 📁 ManagerService/                      ← Manager Microservice
│   │   │   ├── EcoFleet.ManagerService.Domain
│   │   │   ├── EcoFleet.ManagerService.Application
│   │   │   ├── EcoFleet.ManagerService.Infrastructure
│   │   │   └── EcoFleet.ManagerService.API
│   │   │
│   │   ├── 📁 OrderService/                        ← Order Microservice
│   │   │   ├── EcoFleet.OrderService.Domain
│   │   │   ├── EcoFleet.OrderService.Application
│   │   │   ├── EcoFleet.OrderService.Infrastructure
│   │   │   └── EcoFleet.OrderService.API
│   │   │
│   │   ├── 📁 AssignmentService/                   ← Manager-Driver Assignment Microservice
│   │   │   ├── EcoFleet.AssignmentService.Domain
│   │   │   ├── EcoFleet.AssignmentService.Application
│   │   │   ├── EcoFleet.AssignmentService.Infrastructure
│   │   │   └── EcoFleet.AssignmentService.API
│   │   │
│   │   └── 📁 NotificationService/                 ← Email/Notification Microservice
│   │       ├── EcoFleet.NotificationService.Application
│   │       └── EcoFleet.NotificationService.API
│   │
│   └── 📁 Gateway/
│       └── EcoFleet.ApiGateway                     ← YARP Reverse Proxy
│
├── 📁 tests/
│   ├── EcoFleet.FleetService.UnitTests
│   ├── EcoFleet.DriverService.UnitTests
│   ├── EcoFleet.OrderService.UnitTests
│   └── EcoFleet.IntegrationTests
│
└── 📁 aspire/
    └── EcoFleet.AppHost                            ← .NET Aspire Orchestration
```

### Step 1.2 — Create Each Project

For each microservice, use these project templates:

| Project | Template | SDK |
|---|---|---|
| *.Domain | Class Library | Microsoft.NET.Sdk |
| *.Application | Class Library | Microsoft.NET.Sdk |
| *.Infrastructure | Class Library | Microsoft.NET.Sdk |
| *.API | ASP.NET Core Web API | Microsoft.NET.Sdk.Web |
| EcoFleet.ApiGateway | ASP.NET Core Empty | Microsoft.NET.Sdk.Web |
| EcoFleet.AppHost | .NET Aspire App Host | Aspire template |

In Visual Studio 2026:

- Right-click on the appropriate Solution Folder → Add → New Project
- Set Target Framework to `.NET 10` for all projects
- Enable `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`

---

## PHASE 2: BUILDING BLOCKS (Shared Kernel)

### Step 2.1 — EcoFleet.BuildingBlocks.Domain

Create this as a Class Library (.NET 10). Move these files from your current `EcoFleet.Domain/Common/`:

```
EcoFleet.BuildingBlocks.Domain/
├── Entity.cs                  ← from EcoFleet.Domain/Common/Entity.cs
├── IAggregateRoot.cs          ← from EcoFleet.Domain/Common/IAggregateRoot.cs
├── IDomainEvent.cs            ← from EcoFleet.Domain/Common/IDomainEvent.cs
├── IHasDomainEvents.cs        ← from EcoFleet.Domain/Common/IHasDomainEvents.cs
├── ValueObject.cs             ← from EcoFleet.Domain/Common/ValueObject.cs
└── Exceptions/
    └── DomainException.cs     ← from EcoFleet.Domain/Exceptions/DomainException.cs
```

NuGet packages:
```xml
<PackageReference Include="MediatR.Contracts" Version="2.0.1" />
```

Update namespace from `EcoFleet.Domain.Common` → `EcoFleet.BuildingBlocks.Domain`.

### Step 2.2 — EcoFleet.BuildingBlocks.Application

Move shared application concerns:
```
EcoFleet.BuildingBlocks.Application/
├── Behaviors/
│   ├── ValidationBehavior.cs    ← from EcoFleet.Application/Behaviors/
│   └── LoggingBehavior.cs       ← from EcoFleet.Application/Behaviors/
├── Exceptions/
│   ├── BusinessRuleException.cs ← from EcoFleet.Application/Exceptions/
│   ├── NotFoundException.cs     ← from EcoFleet.Application/Exceptions/
│   └── ValidationErrorException.cs ← from EcoFleet.Application/Exceptions/
├── Interfaces/
│   ├── IRepository.cs          ← from EcoFleet.Application/Interfaces/Data/IRepository.cs
│   └── IUnitOfWork.cs          ← from EcoFleet.Application/Interfaces/Data/IUnitOfWork.cs
└── Common/
    └── PaginatedDTO.cs          ← from EcoFleet.Application/Utilities/Common/
```

NuGet packages:
```xml
<PackageReference Include="MediatR" Version="14.0.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.2" />
```

### Step 2.3 — EcoFleet.BuildingBlocks.Infrastructure

Move shared infrastructure:
```
EcoFleet.BuildingBlocks.Infrastructure/
├── Outbox/
│   ├── OutboxMessage.cs          ← from EcoFleet.Infrastructure/Outbox/
│   └── OutboxProcessor.cs        ← from EcoFleet.Infrastructure/Outbox/
├── Repositories/
│   ├── Repository.cs             ← Generic base repository
│   └── UnitOfWork.cs             ← Base UoW with outbox conversion
├── EventSourcing/
│   ├── MartenConfig.cs           ← NEW: Marten configuration helper
│   └── EventStoreRepository.cs   ← NEW: Event-sourced aggregate repository
└── Extensions/
    └── IQueryableExtensions.cs   ← from EcoFleet.Infrastructure/Utilities/
```

NuGet packages:
```xml
<PackageReference Include="Marten" Version="7.*" />
<PackageReference Include="Marten.AspNetCore" Version="7.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.2" />
<PackageReference Include="MediatR" Version="14.0.0" />
```

### Step 2.4 — EcoFleet.BuildingBlocks.Contracts (Integration Events)

This is the **most critical** project. It defines the contracts that microservices use to communicate via RabbitMQ. These are **NOT** Domain Events — they are **Integration Events** (cross-service).
```
EcoFleet.BuildingBlocks.Contracts/
├── IntegrationEvents/
│   ├── DriverEvents/
│   │   ├── DriverSuspendedIntegrationEvent.cs
│   │   ├── DriverReinstatedIntegrationEvent.cs
│   │   ├── DriverCreatedIntegrationEvent.cs
│   │   └── DriverStatusChangedIntegrationEvent.cs
│   │
│   ├── VehicleEvents/
│   │   ├── VehicleDriverAssignedIntegrationEvent.cs
│   │   ├── VehicleMaintenanceStartedIntegrationEvent.cs
│   │   └── VehicleCreatedIntegrationEvent.cs
│   │
│   ├── OrderEvents/
│   │   ├── OrderCreatedIntegrationEvent.cs
│   │   ├── OrderCompletedIntegrationEvent.cs
│   │   └── OrderCancelledIntegrationEvent.cs
│   │
│   └── AssignmentEvents/
│       ├── AssignmentCreatedIntegrationEvent.cs
│       └── AssignmentDeactivatedIntegrationEvent.cs
│
└── DTOs/                         ← Shared lightweight DTOs for inter-service queries
    ├── DriverSummaryDTO.cs
    └── VehicleSummaryDTO.cs
```

Example Integration Event:
```csharp
namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;

// MassTransit uses the class name as the message type (no interface needed)
public record DriverSuspendedIntegrationEvent
{
    public Guid DriverId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime OccurredOn { get; init; }
}
```

NuGet packages:
```xml
<PackageReference Include="MassTransit.Abstractions" Version="9.*" />
```

> **Key Principle:** Domain Events (MediatR `INotification`) stay **inside** each microservice. Integration Events (MassTransit messages) travel **between** microservices via RabbitMQ.

---

## PHASE 3: DRIVER MICROSERVICE (Reference Implementation)

This is the most complex service — use it as the template for all others.

### Step 3.1 — EcoFleet.DriverService.Domain
```
EcoFleet.DriverService.Domain/
├── Entities/
│   ├── Driver.cs                ← Copy from EcoFleet.Domain/Entities/Driver.cs
│   └── DriverId.cs              ← Copy from EcoFleet.Domain/Entities/DriverId.cs
├── ValueObjects/
│   ├── FullName.cs              ← Copy from EcoFleet.Domain/ValueObjects/
│   ├── Email.cs                 ← Copy
│   ├── PhoneNumber.cs           ← Copy
│   └── DriverLicense.cs         ← Copy
├── Enums/
│   └── DriverStatus.cs          ← Copy from EcoFleet.Domain/Enums/
└── Events/
    ├── DriverSuspendedEvent.cs   ← Copy from EcoFleet.Domain/Events/ (Domain Event - internal)
    └── DriverReinstatedEvent.cs  ← Copy (Domain Event - internal)
```

**Reference:** `EcoFleet.BuildingBlocks.Domain`

> Remove `VehicleId` reference from `Driver.cs` — in microservices, the Driver does NOT know about Vehicle directly. Replace `CurrentVehicleId` with a nullable `Guid? AssignedVehicleId` (primitive, no strong typing across boundaries).

### Step 3.2 — EcoFleet.DriverService.Application
```
EcoFleet.DriverService.Application/
├── Interfaces/
│   ├── IDriverRepository.cs     ← Specific to this service (adapted from IRepositoryDriver)
│   └── IDriverEventStore.cs     ← NEW: For event sourcing
├── UseCases/
│   ├── Commands/
│   │   ├── CreateDriver/        ← Copy & adapt from current
│   │   ├── UpdateDriver/
│   │   ├── DeleteDriver/
│   │   ├── SuspendDriver/
│   │   └── ReinstateDriver/
│   ├── Queries/
│   │   ├── GetAllDrivers/
│   │   └── GetDriverById/
│   └── IntegrationEventHandlers/      ← NEW: Consumers for events from other services
│       └── VehicleAssignedToDriverHandler.cs
├── DTOs/
│   ├── DriverDetailDTO.cs
│   └── FilterDriverDTO.cs
└── DependencyInjection.cs
```

Key Changes in Command Handlers:

The `SuspendDriverHandler` now publishes an Integration Event after the Domain Event:
```csharp
public class SuspendDriverHandler : IRequestHandler<SuspendDriverCommand>
{
    private readonly IDriverRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint; // MassTransit

    public async Task Handle(SuspendDriverCommand request, CancellationToken ct)
    {
        var driver = await _repository.GetByIdAsync(new DriverId(request.Id), ct)
            ?? throw new NotFoundException(nameof(Driver), request.Id);

        driver.Suspend(); // Raises internal DriverSuspendedEvent

        await _unitOfWork.SaveChangesAsync(ct); // Saves + creates outbox message

        // Publish Integration Event to RabbitMQ (for other services)
        await _publishEndpoint.Publish(new DriverSuspendedIntegrationEvent
        {
            DriverId = driver.Id.Value,
            FirstName = driver.Name.FirstName,
            LastName = driver.Name.LastName,
            Email = driver.Email.Value,
            OccurredOn = DateTime.UtcNow
        }, ct);
    }
}
```

### Step 3.3 — EcoFleet.DriverService.Infrastructure
```
EcoFleet.DriverService.Infrastructure/
├── Persistence/
│   ├── DriverDbContext.cs        ← Own DbContext (Database-per-Service)
│   └── Configurations/
│       └── DriverConfiguration.cs ← Copy from EcoFleet.Infrastructure/Configurations/
├── Repositories/
│   └── DriverRepository.cs       ← Adapted from RepositoryDriver
├── EventStore/
│   └── DriverEventStoreRepository.cs  ← NEW: Marten-based event sourcing
└── DependencyInjection.cs
```

DriverDbContext (Database-per-Service pattern):
```csharp
public class DriverDbContext : DbContext
{
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DriverDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### Step 3.4 — EcoFleet.DriverService.API
```
EcoFleet.DriverService.API/
├── Controllers/
│   └── DriversController.cs      ← Copy from EcoFleet.API/Controllers/DriversController.cs
├── Middlewares/
│   └── GlobalExceptionHandlerMiddleware.cs  ← Copy from shared
├── Consumers/                     ← NEW: MassTransit Consumers
│   └── VehicleDriverAssignedConsumer.cs
├── Program.cs
├── appsettings.json
└── Dockerfile                    ← NEW
```

Program.cs for Driver Microservice:
```csharp
using MassTransit;
using Marten;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// 2. Application Layer (MediatR + FluentValidation)
builder.Services.AddDriverApplication();

// 3. Infrastructure Layer (EF Core + own database)
builder.Services.AddDriverInfrastructure(builder.Configuration);

// 4. Marten Event Store (PostgreSQL)
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("EventStore")!);
    options.DatabaseSchemaName = "driver_events";
}).UseLightweightSessions();

// 5. MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    // Register all consumers from this assembly
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        cfg.ConfigureEndpoints(context);
    });
});

// 6. API Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 7. Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DriverDb")!)
    .AddRabbitMQ(builder.Configuration.GetConnectionString("RabbitMQ")!);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapOpenApi();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

appsettings.json:
```json
{
  "ConnectionStrings": {
    "DriverDb": "Server=localhost;Database=EcoFleet_DriverDb;Trusted_Connection=true;TrustServerCertificate=true;",
    "EventStore": "Host=localhost;Database=ecofleet_events;Username=postgres;Password=postgres",
    "RabbitMQ": "amqp://guest:guest@localhost:5672"
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Information" },
    "WriteTo": [{ "Name": "Console" }]
  }
}
```

---

## PHASE 4: EVENT SOURCING WITH MARTEN

### Step 4.1 — Understanding the Event Sourcing Model

For each aggregate, instead of saving the **current state** to SQL Server, you append **events** to an event stream in PostgreSQL (Marten). The current state is rebuilt by replaying events.

**Event Flow:**
```
Command → Handler → Aggregate.Apply(event) → Marten.AppendEvents() → Marten.SaveChangesAsync()
                                                      ↓
                                              PostgreSQL Event Store
                                                      ↓
                                              Marten Projections → Read Model (EF Core SQL Server)
```

### Step 4.2 — Create Event-Sourced Aggregate Base Class

In `EcoFleet.BuildingBlocks.Infrastructure/EventSourcing/`:
```csharp
namespace EcoFleet.BuildingBlocks.Infrastructure.EventSourcing;

public abstract class EventSourcedAggregate
{
    public Guid Id { get; protected set; }
    public int Version { get; set; }

    private readonly List<object> _uncommittedEvents = new();
    public IReadOnlyList<object> UncommittedEvents => _uncommittedEvents;

    protected void RaiseEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
        Apply(@event);
    }

    // Each aggregate implements this to rebuild state from events
    public abstract void Apply(object @event);

    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();
}
```

### Step 4.3 — Define Granular Events for Driver Aggregate

In `EcoFleet.DriverService.Domain/Events/`:
```csharp
// These are the STORED events in the event store (not MediatR notifications)
public record DriverCreatedStoreEvent(
    Guid DriverId, string FirstName, string LastName,
    string License, string Email, string? PhoneNumber,
    DateTime? DateOfBirth, DateTime CreatedAt);

public record DriverNameUpdatedStoreEvent(
    Guid DriverId, string FirstName, string LastName, DateTime UpdatedAt);

public record DriverSuspendedStoreEvent(
    Guid DriverId, DateTime SuspendedAt);

public record DriverReinstatedStoreEvent(
    Guid DriverId, DateTime ReinstatedAt);

public record DriverVehicleAssignedStoreEvent(
    Guid DriverId, Guid VehicleId, DateTime AssignedAt);

public record DriverVehicleUnassignedStoreEvent(
    Guid DriverId, DateTime UnassignedAt);
```

### Step 4.4 — Implement Event-Sourced Driver Aggregate

```csharp
public class DriverAggregate : EventSourcedAggregate
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string License { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public DriverStatus Status { get; private set; }
    public Guid? AssignedVehicleId { get; private set; }

    // For Marten deserialization
    public DriverAggregate() { }

    // Factory method
    public static DriverAggregate Create(string firstName, string lastName,
        string license, string email, string? phoneNumber, DateTime? dateOfBirth)
    {
        var aggregate = new DriverAggregate();
        aggregate.RaiseEvent(new DriverCreatedStoreEvent(
            Guid.NewGuid(), firstName, lastName, license, email,
            phoneNumber, dateOfBirth, DateTime.UtcNow));
        return aggregate;
    }

    public void Suspend()
    {
        if (Status == DriverStatus.OnDuty)
            throw new DomainException("Cannot suspend a driver currently on duty.");
        RaiseEvent(new DriverSuspendedStoreEvent(Id, DateTime.UtcNow));
    }

    public void Reinstate()
    {
        if (Status != DriverStatus.Suspended)
            throw new DomainException("Only suspended drivers can be reinstated.");
        RaiseEvent(new DriverReinstatedStoreEvent(Id, DateTime.UtcNow));
    }

    // Marten calls this to rebuild state
    public override void Apply(object @event)
    {
        switch (@event)
        {
            case DriverCreatedStoreEvent e:
                Id = e.DriverId;
                FirstName = e.FirstName;
                LastName = e.LastName;
                License = e.License;
                Email = e.Email;
                PhoneNumber = e.PhoneNumber;
                Status = DriverStatus.Available;
                break;
            case DriverSuspendedStoreEvent:
                Status = DriverStatus.Suspended;
                AssignedVehicleId = null;
                break;
            case DriverReinstatedStoreEvent:
                Status = DriverStatus.Available;
                break;
            case DriverVehicleAssignedStoreEvent e:
                AssignedVehicleId = e.VehicleId;
                Status = DriverStatus.OnDuty;
                break;
            case DriverVehicleUnassignedStoreEvent:
                AssignedVehicleId = null;
                Status = DriverStatus.Available;
                break;
        }
    }
}
```

### Step 4.5 — Marten Projections (Event Store → Read Model)

Create a Marten Projection that materializes the read model for queries:
```csharp
using Marten.Events.Aggregation;

public class DriverReadModelProjection : SingleStreamProjection<DriverReadModel>
{
    public DriverReadModel Create(DriverCreatedStoreEvent @event) => new()
    {
        Id = @event.DriverId,
        FirstName = @event.FirstName,
        LastName = @event.LastName,
        License = @event.License,
        Email = @event.Email,
        PhoneNumber = @event.PhoneNumber,
        Status = DriverStatus.Available.ToString(),
        CreatedAt = @event.CreatedAt
    };

    public void Apply(DriverSuspendedStoreEvent @event, DriverReadModel model)
    {
        model.Status = DriverStatus.Suspended.ToString();
        model.AssignedVehicleId = null;
    }

    public void Apply(DriverReinstatedStoreEvent @event, DriverReadModel model)
    {
        model.Status = DriverStatus.Available.ToString();
    }

    public void Apply(DriverVehicleAssignedStoreEvent @event, DriverReadModel model)
    {
        model.AssignedVehicleId = @event.VehicleId;
        model.Status = DriverStatus.OnDuty.ToString();
    }
}
```

Register projection in Program.cs:
```csharp
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("EventStore")!);
    options.Projections.Add<DriverReadModelProjection>(ProjectionLifecycle.Inline);
}).UseLightweightSessions();
```

### Step 4.6 — Command Handler Using Marten Event Store

```csharp
public class SuspendDriverHandler : IRequestHandler<SuspendDriverCommand>
{
    private readonly IDocumentSession _session;         // Marten
    private readonly IPublishEndpoint _publishEndpoint; // MassTransit

    public async Task Handle(SuspendDriverCommand request, CancellationToken ct)
    {
        // 1. Load aggregate from event stream
        var driver = await _session.Events.AggregateStreamAsync<DriverAggregate>(request.Id, token: ct)
            ?? throw new NotFoundException(nameof(Driver), request.Id);

        // 2. Execute domain logic (raises internal events)
        driver.Suspend();

        // 3. Append new events to the stream
        _session.Events.Append(driver.Id, driver.UncommittedEvents.ToArray());
        await _session.SaveChangesAsync(ct);

        // 4. Publish Integration Event for other microservices
        await _publishEndpoint.Publish(new DriverSuspendedIntegrationEvent
        {
            DriverId = driver.Id,
            FirstName = driver.FirstName,
            LastName = driver.LastName,
            Email = driver.Email,
            OccurredOn = DateTime.UtcNow
        }, ct);
    }
}
```

---

## PHASE 5: MASSTRANSIT + RABBITMQ INTEGRATION

### Step 5.1 — Install RabbitMQ

**Option A — Docker (Recommended for Development):**
```bash
docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:4-management
```
Access management UI: http://localhost:15672 (guest/guest)

**Option B — Visual Studio 2026 with .NET Aspire:**
RabbitMQ is auto-provisioned (see Phase 8).

### Step 5.2 — Add MassTransit to Each Microservice

In each microservice's `.API.csproj`:
```xml
<PackageReference Include="MassTransit.RabbitMQ" Version="9.*" />
```

In each `Program.cs`:
```csharp
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));

        // Enable retries
        cfg.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));

        cfg.ConfigureEndpoints(context);
    });
});
```

### Step 5.3 — Create MassTransit Consumers

In NotificationService — Consuming `DriverSuspendedIntegrationEvent`:
```csharp
using MassTransit;
using EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;

namespace EcoFleet.NotificationService.API.Consumers;

public class DriverSuspendedConsumer : IConsumer<DriverSuspendedIntegrationEvent>
{
    private readonly INotificationsService _notificationsService;
    private readonly ILogger<DriverSuspendedConsumer> _logger;

    public DriverSuspendedConsumer(INotificationsService notificationsService,
        ILogger<DriverSuspendedConsumer> logger)
    {
        _notificationsService = notificationsService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DriverSuspendedIntegrationEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received DriverSuspended event for {DriverId}", msg.DriverId);

        await _notificationsService.SendDriverSuspendedNotification(new DriverSuspendedEventDTO
        {
            FirstName = msg.FirstName,
            LastName = msg.LastName,
            Email = msg.Email
        });
    }
}
```

In FleetService — Consuming driver status changes to sync vehicle state:
```csharp
public class DriverSuspendedConsumer : IConsumer<DriverSuspendedIntegrationEvent>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task Consume(ConsumeContext<DriverSuspendedIntegrationEvent> context)
    {
        // Find vehicles assigned to this driver and unassign them
        var vehicles = await _vehicleRepository.GetByDriverIdAsync(context.Message.DriverId);
        foreach (var vehicle in vehicles)
        {
            vehicle.UnassignDriver();
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### Step 5.4 — Replace Cross-Aggregate Calls with Integration Events

**Current Monolith Problem:** `CreateVehicleHandler` directly calls `IRepositoryDriver` to validate and assign a driver.

**Microservices Solution — Saga/Choreography Pattern:**
```
FleetService                          DriverService
    │                                      │
    │  POST /api/v1/vehicles               │
    │  (with DriverId)                     │
    │                                      │
    │──Publish──►VehicleCreatedWithDriverAssignmentRequest──►│
    │                                      │
    │                                      │  Validate driver exists & available
    │                                      │  Mark driver OnDuty
    │                                      │
    │◄──Publish──DriverAssignedToVehicleEvent◄──│
    │                                      │
    │  Update vehicle.CurrentDriverId      │
    │  Set vehicle status = Active         │
    │                                      │
```

Alternatively, for synchronous validation where eventual consistency is not acceptable, use gRPC or an HTTP call between services:
```csharp
// In FleetService: call DriverService API to validate
public class CreateVehicleHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly HttpClient _driverServiceClient; // Named HttpClient

    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken ct)
    {
        if (request.CurrentDriverId is not null)
        {
            // Synchronous check via HTTP
            var response = await _driverServiceClient.GetAsync(
                $"/api/v1/drivers/{request.CurrentDriverId}", ct);

            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("Driver", request.CurrentDriverId.Value);
        }
        // ... create vehicle
    }
}
```

---

## PHASE 6: REMAINING MICROSERVICES

### Step 6.1 — FleetService (Vehicles)

Follow the same structure as DriverService. Key differences:

- **Domain:** Vehicle, VehicleId, LicensePlate, Geolocation, VehicleStatus
- **Events Stored:** VehicleCreated, VehicleDriverAssigned, VehicleMaintenanceStarted, VehicleDriverUnassigned, VehiclePlateUpdated
- **Consumes from RabbitMQ:** `DriverSuspendedIntegrationEvent` (to unassign driver from vehicle)
- **Publishes to RabbitMQ:** `VehicleDriverAssignedIntegrationEvent`, `VehicleMaintenanceStartedIntegrationEvent`
- Remove strong reference to `DriverId` type — use `Guid? CurrentDriverId` instead

### Step 6.2 — ManagerService

Simplest service:

- **Domain:** Manager, ManagerId, FullName, Email
- No Event Sourcing needed (simple CRUD — keep EF Core only)
- No integration events consumed or published (unless you want manager-created notifications)
- Keep existing CQRS pattern with EF Core

### Step 6.3 — OrderService

- **Domain:** Order, OrderId, OrderStatus, Geolocation
- **Events Stored:** OrderCreated, OrderStarted, OrderCompleted, OrderCancelled
- **Consumes:** `DriverSuspendedIntegrationEvent` (to auto-cancel pending orders for that driver)
- **Publishes:** `OrderCompletedIntegrationEvent`
- Replace `DriverId` strong type with `Guid DriverId` (no cross-boundary typing)
- For driver validation on order creation: use HTTP call to DriverService or accept eventual consistency

### Step 6.4 — AssignmentService

- **Domain:** ManagerDriverAssignment, ManagerDriverAssignmentId
- **Consumes:** `DriverSuspendedIntegrationEvent` (to deactivate assignments for suspended drivers)
- **Publishes:** `AssignmentCreatedIntegrationEvent`
- For manager/driver validation: HTTP calls to respective services or eventual consistency

### Step 6.5 — NotificationService

- No domain layer needed (pure infrastructure service)
- Consumes ALL notification-worthy events:
  - `DriverSuspendedIntegrationEvent` → send suspension email
  - `DriverReinstatedIntegrationEvent` → send reinstatement email
  - `OrderCompletedIntegrationEvent` → send delivery confirmation
- Move the `EcoFleet.Emails` project logic here
- This service has no API (purely event-driven) or a minimal API for health checks only

---

## PHASE 7: API GATEWAY WITH YARP

### Step 7.1 — Create the Gateway Project

In Visual Studio 2026: Add → New Project → ASP.NET Core Empty → `EcoFleet.ApiGateway`

NuGet:
```xml
<PackageReference Include="Yarp.ReverseProxy" Version="2.*" />
```

### Step 7.2 — Configure YARP

Program.cs:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.MapReverseProxy();
app.Run();
```

appsettings.json:
```json
{
  "ReverseProxy": {
    "Routes": {
      "drivers-route": {
        "ClusterId": "driver-service",
        "Match": { "Path": "/api/v1/drivers/{**catch-all}" }
      },
      "vehicles-route": {
        "ClusterId": "fleet-service",
        "Match": { "Path": "/api/v1/vehicles/{**catch-all}" }
      },
      "managers-route": {
        "ClusterId": "manager-service",
        "Match": { "Path": "/api/v1/managers/{**catch-all}" }
      },
      "orders-route": {
        "ClusterId": "order-service",
        "Match": { "Path": "/api/v1/orders/{**catch-all}" }
      },
      "assignments-route": {
        "ClusterId": "assignment-service",
        "Match": { "Path": "/api/v1/managerdriverassignments/{**catch-all}" }
      }
    },
    "Clusters": {
      "driver-service": {
        "Destinations": {
          "destination1": { "Address": "https://localhost:5101" }
        }
      },
      "fleet-service": {
        "Destinations": {
          "destination1": { "Address": "https://localhost:5102" }
        }
      },
      "manager-service": {
        "Destinations": {
          "destination1": { "Address": "https://localhost:5103" }
        }
      },
      "order-service": {
        "Destinations": {
          "destination1": { "Address": "https://localhost:5104" }
        }
      },
      "assignment-service": {
        "Destinations": {
          "destination1": { "Address": "https://localhost:5105" }
        }
      }
    }
  }
}
```

Clients now call `https://gateway:5000/api/v1/drivers` and YARP forwards to the correct microservice. The API contract remains identical to your current monolith.

---

## PHASE 8: .NET ASPIRE ORCHESTRATION

### Step 8.1 — Create the AppHost Project

In Visual Studio 2026: Add → New Project → .NET Aspire App Host

Program.cs:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("ecofleet-events");

var sqlServer = builder.AddSqlServer("sqlserver")
    .AddDatabase("driver-db")
    .AddDatabase("fleet-db")
    .AddDatabase("manager-db")
    .AddDatabase("order-db")
    .AddDatabase("assignment-db");

// Microservices
var driverService = builder.AddProject<Projects.EcoFleet_DriverService_API>("driver-service")
    .WithReference(rabbitMq)
    .WithReference(postgres)
    .WithReference(sqlServer.GetDatabase("driver-db"));

var fleetService = builder.AddProject<Projects.EcoFleet_FleetService_API>("fleet-service")
    .WithReference(rabbitMq)
    .WithReference(postgres)
    .WithReference(sqlServer.GetDatabase("fleet-db"));

var managerService = builder.AddProject<Projects.EcoFleet_ManagerService_API>("manager-service")
    .WithReference(rabbitMq)
    .WithReference(sqlServer.GetDatabase("manager-db"));

var orderService = builder.AddProject<Projects.EcoFleet_OrderService_API>("order-service")
    .WithReference(rabbitMq)
    .WithReference(postgres)
    .WithReference(sqlServer.GetDatabase("order-db"));

var assignmentService = builder.AddProject<Projects.EcoFleet_AssignmentService_API>("assignment-service")
    .WithReference(rabbitMq)
    .WithReference(sqlServer.GetDatabase("assignment-db"));

var notificationService = builder.AddProject<Projects.EcoFleet_NotificationService_API>("notification-service")
    .WithReference(rabbitMq);

// API Gateway
builder.AddProject<Projects.EcoFleet_ApiGateway>("api-gateway")
    .WithReference(driverService)
    .WithReference(fleetService)
    .WithReference(managerService)
    .WithReference(orderService)
    .WithReference(assignmentService);

builder.Build().Run();
```

### Step 8.2 — Running with Aspire

- Set `EcoFleet.AppHost` as the Startup Project
- Press F5 in Visual Studio 2026
- Aspire Dashboard opens at `https://localhost:18888` showing:
  - All microservices with health status
  - RabbitMQ with management plugin
  - PostgreSQL (event store)
  - SQL Server databases
  - Distributed traces (OpenTelemetry)
  - Structured logs (Serilog → OpenTelemetry)

---

## PHASE 9: DOCKER CONTAINERIZATION

### Step 9.1 — Add Dockerfile to Each Microservice

In each `*.API` project, add `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Services/DriverService/EcoFleet.DriverService.API/EcoFleet.DriverService.API.csproj", "Services/DriverService/EcoFleet.DriverService.API/"]
COPY ["src/Services/DriverService/EcoFleet.DriverService.Application/EcoFleet.DriverService.Application.csproj", "Services/DriverService/EcoFleet.DriverService.Application/"]
COPY ["src/Services/DriverService/EcoFleet.DriverService.Domain/EcoFleet.DriverService.Domain.csproj", "Services/DriverService/EcoFleet.DriverService.Domain/"]
COPY ["src/Services/DriverService/EcoFleet.DriverService.Infrastructure/EcoFleet.DriverService.Infrastructure.csproj", "Services/DriverService/EcoFleet.DriverService.Infrastructure/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Domain/EcoFleet.BuildingBlocks.Domain.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Application/EcoFleet.BuildingBlocks.Application.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/EcoFleet.BuildingBlocks.Contracts.csproj", "BuildingBlocks/EcoFleet.BuildingBlocks.Contracts/"]

RUN dotnet restore "Services/DriverService/EcoFleet.DriverService.API/EcoFleet.DriverService.API.csproj"

COPY . .
RUN dotnet publish "Services/DriverService/EcoFleet.DriverService.API/EcoFleet.DriverService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoFleet.DriverService.API.dll"]
```

### Step 9.2 — docker-compose.yml

```yaml
services:
  # Infrastructure
  rabbitmq:
    image: rabbitmq:4-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest

  postgres:
    image: postgres:17
    ports:
      - "5432:5432"
    environment:
      POSTGRES_DB: ecofleet_events
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1433:1433"
    environment:
      SA_PASSWORD: "YourStrong!Password123"
      ACCEPT_EULA: "Y"

  # Microservices
  driver-service:
    build:
      context: .
      dockerfile: src/Services/DriverService/EcoFleet.DriverService.API/Dockerfile
    ports:
      - "5101:8080"
    depends_on:
      - rabbitmq
      - postgres
      - sqlserver

  fleet-service:
    build:
      context: .
      dockerfile: src/Services/FleetService/EcoFleet.FleetService.API/Dockerfile
    ports:
      - "5102:8080"
    depends_on:
      - rabbitmq
      - postgres
      - sqlserver

  manager-service:
    build:
      context: .
      dockerfile: src/Services/ManagerService/EcoFleet.ManagerService.API/Dockerfile
    ports:
      - "5103:8080"
    depends_on:
      - rabbitmq
      - sqlserver

  order-service:
    build:
      context: .
      dockerfile: src/Services/OrderService/EcoFleet.OrderService.API/Dockerfile
    ports:
      - "5104:8080"
    depends_on:
      - rabbitmq
      - postgres
      - sqlserver

  assignment-service:
    build:
      context: .
      dockerfile: src/Services/AssignmentService/EcoFleet.AssignmentService.API/Dockerfile
    ports:
      - "5105:8080"
    depends_on:
      - rabbitmq
      - sqlserver

  notification-service:
    build:
      context: .
      dockerfile: src/Services/NotificationService/EcoFleet.NotificationService.API/Dockerfile
    depends_on:
      - rabbitmq

  # API Gateway
  api-gateway:
    build:
      context: .
      dockerfile: src/Gateway/EcoFleet.ApiGateway/Dockerfile
    ports:
      - "5000:8080"
    depends_on:
      - driver-service
      - fleet-service
      - manager-service
      - order-service
      - assignment-service
```

---

## PHASE 10: DATABASE MIGRATION STRATEGY

### Step 10.1 — Database-per-Service

Each microservice gets its own SQL Server database (no shared tables):

| Microservice | Database | Tables |
|---|---|---|
| DriverService | EcoFleet_DriverDb | Drivers, OutboxMessages |
| FleetService | EcoFleet_FleetDb | Vehicles, OutboxMessages |
| ManagerService | EcoFleet_ManagerDb | Managers |
| OrderService | EcoFleet_OrderDb | Orders, OutboxMessages |
| AssignmentService | EcoFleet_AssignmentDb | ManagerDriverAssignments |

**Event Store (shared PostgreSQL):**

| Schema | Streams |
|---|---|
| driver_events | Driver aggregate event streams |
| fleet_events | Vehicle aggregate event streams |
| order_events | Order aggregate event streams |

### Step 10.2 — Data Migration from Monolith

1. Export current data from the monolith's single SQL Server database
2. For each microservice, run `dotnet ef migrations add InitialCreate` using the new DbContext
3. Run `dotnet ef database update` on each individual database
4. Write a one-time migration script to copy existing data:

```sql
-- Example: Migrate drivers from monolith to Driver microservice DB
INSERT INTO EcoFleet_DriverDb.dbo.Drivers
SELECT * FROM EcoFleet_Monolith.dbo.Drivers;
```

5. For event-sourced services, optionally create an "initial snapshot" event for each existing record

---

## PHASE 11: TESTING STRATEGY

### Step 11.1 — Unit Tests (Per Microservice)

Keep your existing test patterns. Each microservice has its own test project:

- `EcoFleet.DriverService.UnitTests` → Test domain logic, handlers
- `EcoFleet.FleetService.UnitTests` → Test vehicle aggregate behavior

### Step 11.2 — Integration Tests

Use Testcontainers to spin up real infrastructure in tests:
```csharp
// Install: Testcontainers.RabbitMq, Testcontainers.PostgreSql, Testcontainers.MsSql
public class DriverServiceIntegrationTests : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder().Build();
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _rabbitMq.StartAsync();
        await _postgres.StartAsync();
    }

    [Fact]
    public async Task SuspendDriver_PublishesIntegrationEvent()
    {
        // Arrange: create WebApplicationFactory with test containers
        // Act: POST suspend command
        // Assert: verify message published to RabbitMQ
    }
}
```

---

## PHASE 12: OBSERVABILITY

### Step 12.1 — OpenTelemetry in Each Microservice

Add to each `Program.cs`:
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddMassTransitInstrumentation()
            .AddSource("Marten");
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    })
    .UseOtlpExporter();
```

### Step 12.2 — Centralized Logging

Serilog is already configured. Add a sink to send logs to a central location:
```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.WithProperty("ServiceName", "DriverService")
    .WriteTo.OpenTelemetry());
```

---

## RECOMMENDED MIGRATION ORDER

Execute the phases in this order to minimize risk:

| Order | Phase | Risk Level | Estimated Effort |
|---|---|---|---|
| 1 | Phase 1: Solution Restructuring | Low | Foundation |
| 2 | Phase 2: Building Blocks | Low | 1-2 days |
| 3 | Phase 3: Driver Microservice (reference) | Medium | 3-4 days |
| 4 | Phase 5: MassTransit + RabbitMQ | Medium | 2-3 days |
| 5 | Phase 4: Event Sourcing (Marten) | High | 3-4 days |
| 6 | Phase 6: Remaining Microservices | Medium | 1-2 days each |
| 7 | Phase 7: API Gateway (YARP) | Low | 1 day |
| 8 | Phase 8: Aspire Orchestration | Low | 1 day |
| 9 | Phase 9: Docker | Low | 1 day |
| 10 | Phase 10: Data Migration | High | 2-3 days |
| 11 | Phase 11: Testing | Medium | 3-4 days |
| 12 | Phase 12: Observability | Low | 1 day |

---

## KEY ARCHITECTURAL DECISIONS SUMMARY

- **Domain Events** (MediatR `INotification`) → stay **INSIDE** a microservice
- **Integration Events** (MassTransit messages) → travel **BETWEEN** microservices via RabbitMQ
- **Outbox Pattern** → you already have it; keep it as a reliability mechanism for publishing integration events
- **Event Sourcing** → apply to aggregates with complex state transitions (Driver, Vehicle, Order). Skip for simple CRUD (Manager)
- **Cross-service data needs** → use asynchronous events (eventual consistency) or synchronous HTTP/gRPC calls where immediate consistency is required
- **API Gateway** → YARP preserves the exact same API contract as your current monolith, making migration transparent to clients
- **Database-per-Service** → each microservice owns its data; no shared database

---

## VERIFICATION

After completing all phases:

1. Start all services via Aspire AppHost (F5 in Visual Studio 2026)
2. Verify API Gateway routes at `https://localhost:5000/api/v1/drivers` (should return same response as current monolith)
3. Test event flow: Suspend a driver → verify:
   - DriverService stores `DriverSuspendedStoreEvent` in Marten
   - RabbitMQ receives `DriverSuspendedIntegrationEvent`
   - NotificationService sends email
   - FleetService unassigns driver from vehicles
   - OrderService cancels pending orders
4. Check RabbitMQ Management at `http://localhost:15672` → verify exchanges, queues, and message flow
5. Check Aspire Dashboard at `https://localhost:18888` → verify distributed traces span across services
6. Run all unit tests: `dotnet test` in solution root
7. Run integration tests with Testcontainers
