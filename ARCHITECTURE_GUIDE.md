# EcoFleet Microservices — Architecture Guide

This guide explains how the solution is structured, what every component is responsible for, and how all the pieces connect at runtime. It is written to be read top-to-bottom, starting from the big picture and drilling into the details of each layer.

---

## Table of Contents

1. [Big Picture — What the System Does](#1-big-picture)
2. [Solution Layout — Folders and Projects](#2-solution-layout)
3. [Building Blocks — The Shared Kernel](#3-building-blocks)
4. [Microservice Internal Architecture (4 Layers)](#4-microservice-internal-architecture)
5. [Event Sourcing with Marten](#5-event-sourcing-with-marten)
6. [CQRS with MediatR](#6-cqrs-with-mediatr)
7. [Integration Events and RabbitMQ with MassTransit](#7-integration-events-and-rabbitmq)
8. [The Transactional Outbox Pattern](#8-the-transactional-outbox-pattern)
9. [API Gateway with YARP](#9-api-gateway-with-yarp)
10. [Database Strategy — Database-per-Service](#10-database-strategy)
11. [Local Orchestration with .NET Aspire](#11-local-orchestration-with-net-aspire)
12. [Docker and docker-compose](#12-docker-and-docker-compose)
13. [Request Lifecycle — Full End-to-End Walkthrough](#13-request-lifecycle-end-to-end)
14. [Cross-Service Event Cascade — Suspend Driver Example](#14-cross-service-event-cascade)
15. [Service-by-Service Reference](#15-service-by-service-reference)

---

## 1. Big Picture

EcoFleet is a fleet management platform that tracks vehicles, drivers, managers, orders, and manager-driver assignments. The entire solution was migrated from a single monolith into six independently deployable microservices, each owning its own data, its own logic, and communicating with the others exclusively through messages on RabbitMQ.

```
                ┌─────────────────────────┐
                │      API Gateway        │  ← single entry point for all clients
                │    (YARP — port 5000)   │
                └────────────┬────────────┘
                             │ routes by path prefix
      ┌──────────┬───────────┼───────────┬──────────────┐
      ▼          ▼           ▼           ▼              ▼
 DriverSvc  FleetSvc   ManagerSvc   OrderSvc    AssignmentSvc
  :7145      :7052       :7143        :7226         :7023
      │          │                     │              │
      └──────────┴──────────┬──────────┴──────────────┘
                            │ async messages
                     ┌──────▼───────┐
                     │   RabbitMQ   │   ← also consumed by NotificationSvc
                     └──────────────┘
      ┌────────────────────────────────────────────────┐
      │  PostgreSQL (Marten Event Store)               │
      │  schema per service: driver_events             │
      │                      fleet_events              │
      │                      order_events              │
      └────────────────────────────────────────────────┘
      ┌────────────────────────────────────────────────┐
      │  SQL Server (EF Core read models & CRUD)        │
      │  EcoFleet_DriverDb    EcoFleet_FleetDb          │
      │  EcoFleet_ManagerDb   EcoFleet_AssignmentDb     │
      └────────────────────────────────────────────────┘
```

**Key rules:**
- No service calls another service's database directly.
- No service imports another service's domain assembly.
- State changes flow outward as integration events on RabbitMQ.
- Clients always talk to the API Gateway, never to individual services.

---

## 2. Solution Layout

```
EcoFleetMicroservices/
│
├── EcoFleet.Microservices.slnx             ← Visual Studio solution
│
├── EcoFleet.BuildingBlocks.Domain/         ─┐
├── EcoFleet.BuildingBlocks.Application/    ─┤  Shared kernel
├── EcoFleet.BuildingBlocks.Infrastructure/ ─┤  (no business logic)
├── EcoFleet.BuildingBlocks.Contracts/      ─┘
│
├── EcoFleet.DriverService.Domain/          ─┐
├── EcoFleet.DriverService.Application/     ─┤  Driver microservice
├── EcoFleet.DriverService.Infrastructure/  ─┤
├── EcoFleet.DriverService.API/             ─┘
│
├── EcoFleet.FleetService.Domain/           ─┐
├── EcoFleet.FleetService.Application/      ─┤  Fleet (Vehicles) microservice
├── EcoFleet.FleetService.Infrastructure/   ─┤
├── EcoFleet.FleetService.API/              ─┘
│
├── EcoFleet.ManagerService.Domain/         ─┐
├── EcoFleet.ManagerService.Application/    ─┤  Manager microservice
├── EcoFleet.ManagerService.Infrastructure/ ─┤
├── EcoFleet.ManagerService.API/            ─┘
│
├── EcoFleet.OrderService.Domain/           ─┐
├── EcoFleet.OrderService.Application/      ─┤  Order microservice
├── EcoFleet.OrderService.Infrastructure/   ─┤
├── EcoFleet.OrderService.API/              ─┘
│
├── EcoFleet.AssignmentService.Domain/      ─┐
├── EcoFleet.AssignmentService.Application/ ─┤  Assignment microservice
├── EcoFleet.AssignmentService.Infrastructure/─┤
├── EcoFleet.AssignmentService.API/         ─┘
│
├── EcoFleet.NotificationService.Application/ ─┐  Notification microservice
├── EcoFleet.NotificationService.API/        ─┘  (event-driven, no domain layer)
│
├── EcoFleet.ApiGateway/                    ← YARP reverse proxy
├── EcoFleet.AppHost/                       ← .NET Aspire local orchestration
│
├── docker-compose.yml                      ← Docker deployment
└── CLAUDE.md                               ← Architecture instructions
```

**Project dependencies follow a strict one-way rule:**
```
API → Infrastructure → Application → Domain → BuildingBlocks.Domain
 └──────────────────→ BuildingBlocks.Application
 └──────────────────→ BuildingBlocks.Contracts
```
The domain projects have no dependency on anything outside the BuildingBlocks.Domain assembly.

---

## 3. Building Blocks

The four `BuildingBlocks` projects are the shared kernel — generic base classes, behaviors, and contracts that every microservice reuses without duplication.

### 3.1 `EcoFleet.BuildingBlocks.Domain`

Contains the lowest-level primitives:

| Type | Purpose |
|---|---|
| `EventSourcedAggregate` | Base class for aggregates that use event sourcing (Driver, Vehicle, Order). Tracks uncommitted events and applies them to rebuild state. |
| `Entity<T>` | Base class for simple EF Core entities (Manager, Assignment). |
| `ValueObject` | Base class for immutable value objects with structural equality. |
| `IDomainEvent` | Marker interface for domain events (extends `INotification` from MediatR). |
| `IAggregateRoot` | Marker interface identifying aggregate roots. |
| `DomainException` | Exception type for domain rule violations — caught by the global middleware and returned as 422 Unprocessable Entity. |

`EventSourcedAggregate` is the most important class in this project:

```csharp
public abstract class EventSourcedAggregate
{
    public Guid Id { get; protected set; }
    public int Version { get; set; }

    private readonly List<object> _uncommittedEvents = new();
    public IReadOnlyList<object> UncommittedEvents => _uncommittedEvents;

    protected void RaiseEvent(object @event)
    {
        _uncommittedEvents.Add(@event);  // queue for persistence
        Apply(@event);                   // immediately update in-memory state
    }

    public abstract void Apply(object @event);  // each aggregate implements this
    public void ClearUncommittedEvents() => _uncommittedEvents.Clear();
}
```

When an aggregate method like `driver.Suspend()` is called, it calls `RaiseEvent(new DriverSuspendedStoreEvent(...))`. This does two things simultaneously: adds the event to the uncommitted list (which will be persisted to PostgreSQL) and immediately mutates the in-memory aggregate state by calling `Apply`. No aggregate state is ever set directly — it always flows through events.

### 3.2 `EcoFleet.BuildingBlocks.Application`

Contains shared application-layer concerns:

| Type | Purpose |
|---|---|
| `ValidationBehavior<TRequest, TResponse>` | MediatR pipeline behavior. Runs all registered `IValidator<T>` implementations for a request before the handler executes. Throws `ValidationErrorException` on failure. |
| `LoggingBehavior<TRequest, TResponse>` | MediatR pipeline behavior. Logs request entry and exit with elapsed time using Serilog. |
| `IRepository<T>` | Generic repository contract (load by ID, add, remove). |
| `IUnitOfWork` | Unit of work contract (`SaveChangesAsync`). |
| `NotFoundException` | Thrown by handlers when an aggregate/entity is not found → mapped to 404. |
| `BusinessRuleException` | Thrown for business violations that are not domain-layer errors → mapped to 409. |
| `ValidationErrorException` | Thrown by the validation behavior → mapped to 400. |
| `PaginatedDTO<T>` | Generic wrapper for paginated query results (items list + total count). |

### 3.3 `EcoFleet.BuildingBlocks.Infrastructure`

Contains shared infrastructure concerns:

| Type | Purpose |
|---|---|
| `Repository<T>` | Generic EF Core repository base implementation. |
| `OutboxMessage` | The entity persisted to SQL Server for the transactional outbox. Fields: `Id`, `Type` (fully-qualified CLR type name), `Content` (JSON), `OccurredOn`, `ProcessedOn`, `Error`. |
| `OutboxProcessor` | `BackgroundService` that polls the `OutboxMessages` table every 60 seconds, deserializes each pending message, and publishes it via MediatR `IPublisher`. Uses `UPDLOCK, READPAST` SQL hints to support multi-replica deployments without double-processing. |

### 3.4 `EcoFleet.BuildingBlocks.Contracts`

The **most critical** shared project. It defines all integration events — the messages that travel between microservices on RabbitMQ.

```
IntegrationEvents/
├── DriverEvents/
│   ├── DriverCreatedIntegrationEvent
│   ├── DriverSuspendedIntegrationEvent   ← cascades to 4 services
│   ├── DriverReinstatedIntegrationEvent
│   └── DriverStatusChangedIntegrationEvent
├── VehicleEvents/
│   ├── VehicleCreatedIntegrationEvent
│   ├── VehicleDriverAssignedIntegrationEvent
│   ├── VehicleDriverUnassignedIntegrationEvent
│   ├── VehicleMaintenanceStartedIntegrationEvent
│   └── VehicleUpdatedIntegrationEvent
├── OrderEvents/
│   ├── OrderCreatedIntegrationEvent
│   ├── OrderCompletedIntegrationEvent
│   └── OrderCancelledIntegrationEvent
└── AssignmentEvents/
    ├── AssignmentCreatedIntegrationEvent
    └── AssignmentDeactivatedIntegrationEvent
```

All integration events are `record` types with `init`-only properties. They carry only primitive types (no domain objects, no strong IDs from other services). MassTransit uses the class name as the RabbitMQ message type.

Both publisher and consumer must reference this same assembly — this is what makes the contract explicit and shared.

---

## 4. Microservice Internal Architecture

Every microservice (except NotificationService) uses the same four-layer Clean Architecture structure:

```
API (entry point — HTTP + message consumers)
 └── Application (use cases: commands, queries, integration event handlers)
      └── Domain (aggregate, value objects, domain events, enums)
           └── BuildingBlocks.Domain (base classes)
      └── Infrastructure (persistence: DbContext, repositories, event store, projections)
           └── BuildingBlocks.Infrastructure (outbox, generic repository)
```

### Layer 1 — Domain

Contains pure business logic with zero infrastructure dependencies. Nothing here knows about databases, HTTP, or messaging.

**For event-sourced services (Driver, Fleet, Order):**
- An `Aggregate` class that extends `EventSourcedAggregate`. Its public methods never mutate state directly — they call `RaiseEvent(new SomeStoreEvent(...))`.
- A set of `StoreEvents` — immutable records that represent something that happened. These are what gets persisted to PostgreSQL.
- `DomainException` is thrown when a business invariant is violated (e.g., cannot suspend a driver who is OnDuty).

**For CRUD services (Manager, Assignment):**
- A simple entity class that extends `Entity<T>`.
- No events, no aggregate behavior.

### Layer 2 — Application

Orchestrates use cases. Has no infrastructure dependencies — only interfaces.

**Commands** — represent intent to change state:
```
CreateDriverCommand  → CreateDriverHandler  + CreateDriverValidator
UpdateDriverCommand  → UpdateDriverHandler  + UpdateDriverValidator
SuspendDriverCommand → SuspendDriverHandler
DeleteDriverCommand  → DeleteDriverHandler
```

**Queries** — represent read requests:
```
GetAllDriversQuery   → GetAllDriversHandler
GetDriverByIdQuery   → GetDriverByIdHandler
```

**DependencyInjection.cs** — registers MediatR, all validators from the assembly, and both pipeline behaviors:
```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));  // runs before every handler
});
services.AddValidatorsFromAssembly(assembly);
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

**Interfaces** — contracts for repositories and event stores that infrastructure will implement:
- `IDriverRepository` — EF Core read model operations (find by ID, find by license, paginated listing).
- `IDriverEventStore` — Marten operations (load aggregate by replaying events, save uncommitted events, archive/delete stream).

### Layer 3 — Infrastructure

Implements the interfaces declared by the Application layer.

**Persistence (EF Core + SQL Server):**
- `DriverDbContext` — owns two tables: `Drivers` (the materialized read model) and `OutboxMessages`. The schema is created automatically at startup via `db.Database.EnsureCreatedAsync()` — no migration files needed.
- `DriverConfiguration` — Fluent API mappings (column types, indexes, value conversions).
- `DriverRepository` — implements `IDriverRepository`. Used only for reads and for writing the read model when it is maintained separately (e.g. Assignment, Manager services).

**Event Store (Marten + PostgreSQL):**
- `DriverEventStoreRepository` — implements `IDriverEventStore`.
  - `LoadAsync(Guid id)` → calls `_session.Events.AggregateStreamAsync<DriverAggregate>(id)` which replays all events for that stream to rebuild the aggregate.
  - `SaveAsync(aggregate)` → calls `_session.Events.Append(aggregate.Id, uncommittedEvents)` then `SaveChangesAsync`.
  - `DeleteAsync(Guid id)` → calls `_session.Events.ArchiveStream(id)` — events are preserved for audit but the stream is hidden from projections.

**Projections (Marten):**
- `DriverReadModelProjection` extends `SingleStreamProjection<DriverReadModel, Guid>`.
- Registered with `ProjectionLifecycle.Inline` — Marten updates the read model in the same database transaction as the event append, so reads are always consistent with writes.
- Each `Apply` overload mutates the `DriverReadModel` document stored in PostgreSQL.

**DependencyInjection.cs** — registers: `DriverDbContext`, `DriverRepository`, `DriverEventStoreRepository`, `IUnitOfWork`, and the `OutboxProcessor` background service.

### Layer 4 — API

The process entry point and the HTTP surface.

**Program.cs** — wires everything together in this order:
1. Serilog structured logging
2. Application layer (`AddDriverApplication()`)
3. Infrastructure layer (`AddDriverInfrastructure(config)`)
4. Marten (event store + projections configuration)
5. MassTransit + RabbitMQ (consumers registered from the API assembly)
6. ASP.NET Core controllers + OpenAPI
7. Health checks (SQL Server + RabbitMQ)
8. Middleware pipeline (GlobalExceptionHandler → HTTPS redirect → controllers → health endpoint)
9. `EnsureCreatedAsync()` for SQL Server on startup

**Controllers** — thin; each action creates a command/query, sends it through MediatR, and returns the result:
```csharp
[HttpPatch("suspend/{id}")]
public async Task<IActionResult> SuspendDriver(Guid id)
{
    await _sender.Send(new SuspendDriverCommand(id));
    return NoContent();
}
```

**Consumers** — MassTransit `IConsumer<T>` implementations. Each consumer handles one integration event type arriving from RabbitMQ:
```
VehicleDriverAssignedConsumer     ← listens in DriverService
VehicleDriverUnassignedConsumer   ← listens in DriverService
VehicleMaintenanceStartedConsumer ← listens in DriverService
DriverSuspendedConsumer           ← listens in FleetService, OrderService, AssignmentService, NotificationService
```

**Middlewares** — `GlobalExceptionHandlerMiddleware` catches all unhandled exceptions and maps them to HTTP status codes:

| Exception type | HTTP status |
|---|---|
| `NotFoundException` | 404 Not Found |
| `ValidationErrorException` | 400 Bad Request |
| `DomainException` | 422 Unprocessable Entity |
| `BusinessRuleException` | 409 Conflict |
| Any other | 500 Internal Server Error |

**Dockerfiles** — multi-stage build: SDK image to compile and publish, ASP.NET Core runtime image for the final container. Each service has its own Dockerfile at the root of its API project.

---

## 5. Event Sourcing with Marten

Three services use event sourcing: DriverService, FleetService, and OrderService.

### What it means

Instead of storing the current state of an aggregate in a SQL row and overwriting it on every update, every change is stored as an immutable event appended to a stream. The current state is reconstructed by replaying all events from the beginning of the stream.

```
PostgreSQL event stream for driver 7a3f...
─────────────────────────────────────────
 1. DriverCreatedStoreEvent      (2025-01-10)
 2. DriverLicenseUpdatedStoreEvent (2025-02-01)
 3. DriverVehicleAssignedStoreEvent (2025-03-01)
 4. DriverSuspendedStoreEvent    (2026-02-15)
─────────────────────────────────────────
Replay all → current state: Suspended, no vehicle assigned
```

### Store Events vs Integration Events

| | Store Events | Integration Events |
|---|---|---|
| **Location** | Inside the service's Domain project | In `BuildingBlocks.Contracts` (shared) |
| **Purpose** | Represent "what happened" inside one aggregate | Notify other services of something that happened |
| **Transport** | PostgreSQL (Marten event stream) | RabbitMQ (MassTransit) |
| **Type** | `record` types, not INotification | `record` types, no interface needed |
| **Example** | `DriverSuspendedStoreEvent` | `DriverSuspendedIntegrationEvent` |

### Marten configuration

Registered in `Program.cs` per service:
```csharp
builder.Services.AddMarten(options =>
{
    options.Connection("Host=postgres;Database=ecofleet_events;...");
    options.DatabaseSchemaName = "driver_events";  // each service gets its own schema
    options.Projections.Add<DriverReadModelProjection>(ProjectionLifecycle.Inline);
}).UseLightweightSessions();
```

`UseLightweightSessions()` means Marten does not track identity (no unit-of-work within Marten itself), which is appropriate since each command opens one session, appends events, and closes.

### Read models (projections)

The event store is optimized for writes, not reads. Marten projections solve this by materializing a flat read model document (a PostgreSQL table) updated every time an event is appended:

```
Event appended → DriverReadModelProjection.Apply() called → DriverReadModel document updated
                 (same transaction — ProjectionLifecycle.Inline)
```

Query handlers read from the `DriverReadModel` via Marten's `IDocumentSession.Query<DriverReadModel>()`, not from the event stream directly. This makes reads fast (simple document query) and decoupled from aggregate complexity.

---

## 6. CQRS with MediatR

Every microservice uses CQRS — Commands and Queries are distinct types that flow through MediatR. The controller never directly calls a repository or service; it always sends a request to MediatR and returns whatever comes back.

```
HTTP Request
    │
    ▼
Controller.Action()
    │  _sender.Send(new CreateDriverCommand(...))
    ▼
MediatR Pipeline
    │
    ├── LoggingBehavior        (logs request name + elapsed time)
    │
    ├── ValidationBehavior     (runs all IValidator<CreateDriverCommand>)
    │                          throws ValidationErrorException if invalid
    │
    └── CreateDriverHandler    (the actual business logic)
            │
            ├── eventStore.SaveAsync(aggregate)     (Marten)
            └── publishEndpoint.Publish(event)      (MassTransit)
```

**Commands** implement `IRequest` or `IRequest<TResult>`. They change state and may return a result (e.g., `IRequest<Guid>` for create operations).

**Queries** implement `IRequest<TResult>`. They only read data — no state changes, no events published.

**Validators** implement `AbstractValidator<TCommand>` from FluentValidation. The `ValidationBehavior` finds all registered validators for the incoming request type and runs them before the handler ever executes.

---

## 7. Integration Events and RabbitMQ

This is how microservices communicate. When something important happens in one service, it publishes an integration event to RabbitMQ. Other services that care about that event consume it.

### Publishing (from a command handler)

```csharp
// SuspendDriverHandler.cs
await _eventStore.SaveAsync(driver, ct);  // 1. persist state change

await _publishEndpoint.Publish(new DriverSuspendedIntegrationEvent  // 2. notify others
{
    DriverId = driver.Id,
    FirstName = driver.FirstName,
    Email = driver.Email,
    OccurredOn = DateTime.UtcNow
}, ct);
```

`IPublishEndpoint` is injected by MassTransit. `Publish` sends the message to all consumers registered for that type.

### Consuming (in a consumer class)

```csharp
// DriverSuspendedConsumer.cs in FleetService
public class DriverSuspendedConsumer : IConsumer<DriverSuspendedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<DriverSuspendedIntegrationEvent> context)
    {
        // find vehicles assigned to the suspended driver
        var vehicles = await _querySession.Query<VehicleReadModel>()
            .Where(v => v.CurrentDriverId == context.Message.DriverId)
            .ToListAsync();

        // unassign each vehicle using event sourcing
        foreach (var vehicle in vehicles)
        {
            var aggregate = await _eventStore.LoadAsync(vehicle.Id);
            aggregate.UnassignDriver();
            await _eventStore.SaveAsync(aggregate);
        }
    }
}
```

### How MassTransit auto-registers consumers

In `Program.cs` of each API project:
```csharp
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumers(typeof(Program).Assembly);  // finds all IConsumer<T> in the assembly

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        cfg.ConfigureEndpoints(context);  // creates one queue per consumer
    });
});
```

`SetKebabCaseEndpointNameFormatter()` + `ConfigureEndpoints(context)` means MassTransit automatically creates a dedicated RabbitMQ queue for each consumer named after the consumer class in kebab-case (e.g. `driver-suspended-consumer`). No manual queue or binding configuration is needed.

### Complete integration event map

| Event | Published by | Consumed by |
|---|---|---|
| `DriverCreatedIntegrationEvent` | DriverService | (notification if enabled) |
| `DriverSuspendedIntegrationEvent` | DriverService | FleetService, OrderService, AssignmentService, NotificationService |
| `DriverReinstatedIntegrationEvent` | DriverService | NotificationService |
| `VehicleDriverAssignedIntegrationEvent` | FleetService | DriverService |
| `VehicleDriverUnassignedIntegrationEvent` | FleetService | DriverService |
| `VehicleMaintenanceStartedIntegrationEvent` | FleetService | DriverService |
| `OrderCompletedIntegrationEvent` | OrderService | NotificationService |
| `AssignmentCreatedIntegrationEvent` | AssignmentService | — |
| `AssignmentDeactivatedIntegrationEvent` | AssignmentService | — |

---

## 8. The Transactional Outbox Pattern

The outbox solves the dual-write problem: if the application crashes after saving the aggregate to the event store but before publishing to RabbitMQ, the event is lost. The outbox guarantees at-least-once delivery.

### How it works

Domain events (internal `IDomainEvent` notifications) are converted to `OutboxMessage` rows by the `IUnitOfWork` implementation as part of the same SQL transaction as the entity save. Later, a background service picks them up and dispatches them.

**OutboxMessage schema:**
```
Id          GUID     — unique message ID
Type        string   — fully-qualified CLR type name (for deserialization)
Content     string   — JSON-serialized domain event
OccurredOn  datetime — when the event was raised
ProcessedOn datetime — set when successfully dispatched (null = pending)
Error       string   — set if dispatch failed
```

**OutboxProcessor (BackgroundService):**
1. Opens a SQL transaction.
2. Selects up to 20 unprocessed messages using `SELECT ... WITH (UPDLOCK, READPAST)`:
   - `UPDLOCK` locks selected rows so a second replica cannot pick the same messages.
   - `READPAST` means other replicas skip locked rows instead of blocking — safe for horizontal scaling.
3. For each message: resolves the CLR type, deserializes the JSON, publishes via MediatR `IPublisher`.
4. Sets `ProcessedOn = UtcNow` on success, `Error = ex.Message` on failure.
5. Commits the transaction.
6. Waits 60 seconds and repeats.

Note: The OutboxProcessor dispatches **domain events** (internal) via MediatR. The command handlers publish **integration events** (external) directly via MassTransit immediately after saving the aggregate. These are two separate reliability mechanisms for two different audiences.

---

## 9. API Gateway with YARP

YARP (Yet Another Reverse Proxy) is a .NET library that acts as the single entry point. Clients call `https://gateway:5000/api/v1/...` and never need to know which service handles each path.

**Routing configuration (`appsettings.json`):**

| Route | Path pattern | Target cluster |
|---|---|---|
| `drivers-route` | `/api/v1/drivers/{**catch-all}` | `driver-service` |
| `vehicles-route` | `/api/v1/vehicles/{**catch-all}` | `fleet-service` |
| `managers-route` | `/api/v1/managers/{**catch-all}` | `manager-service` |
| `orders-route` | `/api/v1/orders/{**catch-all}` | `order-service` |
| `assignments-route` | `/api/v1/managerdriverassignments/{**catch-all}` | `assignment-service` |

**Aspire integration (Program.cs):**

When running under Aspire, each microservice's URL is injected as an environment variable (`services__driver-service__https__0`). The gateway reads these variables at startup and overrides the hardcoded `appsettings.json` cluster addresses:

```csharp
foreach (var (cluster, service) in serviceMap)
{
    var url = builder.Configuration[$"services:{service}:https:0"]
           ?? builder.Configuration[$"services:{service}:http:0"];
    if (url is not null)
        builder.Configuration[$"ReverseProxy:Clusters:{cluster}:Destinations:destination1:Address"] = url;
}
```

This means the gateway always discovers the correct dynamic ports assigned by Aspire, while the `appsettings.json` fallback values work for local-only runs without Aspire.

---

## 10. Database Strategy

### Database-per-Service (SQL Server)

Each service owns its SQL Server database. No service can read or write another service's tables.

| Service | Database | Tables |
|---|---|---|
| DriverService | `EcoFleet_DriverDb` | `Drivers`, `OutboxMessages` |
| FleetService | `EcoFleet_FleetDb` | `Vehicles`, `OutboxMessages` |
| ManagerService | `EcoFleet_ManagerDb` | `Managers` |
| AssignmentService | `EcoFleet_AssignmentDb` | `ManagerDriverAssignments`, `OutboxMessages` |
| OrderService | `EcoFleet_OrderDb` | (minimal — mostly uses Marten) |

The schema is created by `db.Database.EnsureCreatedAsync()` on startup. There are no EF Core migration files — the schema is generated from the `DbContext` and entity configuration classes.

### Event Store (PostgreSQL + Marten)

Three services share one PostgreSQL server but use separate schemas:

| Service | PostgreSQL schema | Contents |
|---|---|---|
| DriverService | `driver_events` | Driver event streams + `DriverReadModel` documents |
| FleetService | `fleet_events` | Vehicle event streams + `VehicleReadModel` documents |
| OrderService | `order_events` | Order event streams + `OrderReadModel` documents |

Marten creates all tables automatically on startup. Each `mt_events` table within a schema holds the raw event rows; `mt_doc_driverreadmodel` (for example) holds the materialized projection documents.

### Why two databases per event-sourced service?

| SQL Server | PostgreSQL |
|---|---|
| Outbox messages (domain events awaiting internal dispatch) | Event streams (source of truth for aggregate state) |
| Read models materialized by EF Core (for Assignment, Manager) | Read model documents materialized by Marten projections |
| Driver/Vehicle/Order read model is actually in PostgreSQL (Marten projects there) | |

In practice, for event-sourced services, the SQL Server database is mainly used for the `OutboxMessages` table and potentially a backup EF Core read model. The Marten read model (in PostgreSQL) is the primary query target.

---

## 11. Local Orchestration with .NET Aspire

`.NET Aspire AppHost` (`EcoFleet.AppHost/Program.cs`) is the local development orchestrator. Running it (F5 in Visual Studio) starts everything automatically in the correct order.

```csharp
var rabbitMq = builder.AddRabbitMQ("rabbitmq").WithManagementPlugin().WithDataVolume();
var postgres = builder.AddPostgres("postgres").WithDataVolume();
var eventsDb = postgres.AddDatabase("EventStore");
var sqlServer = builder.AddSqlServer("sqlserver").WithDataVolume();
var driverDb = sqlServer.AddDatabase("DriverDb");
// ...

var driverService = builder.AddProject<Projects.EcoFleet_DriverService_API>("driver-service")
    .WithReference(rabbitMq).WaitFor(rabbitMq)   // won't start until RabbitMQ is healthy
    .WithReference(eventsDb).WaitFor(eventsDb)   // connection string injected automatically
    .WithReference(driverDb).WaitFor(driverDb);

builder.AddProject<Projects.EcoFleet_ApiGateway>("api-gateway")
    .WithReference(driverService).WaitFor(driverService)  // gateway starts last
    // ...
```

**What Aspire does automatically:**
- Starts Docker containers for RabbitMQ, PostgreSQL, SQL Server with persistent data volumes.
- Injects connection strings into each service as environment variables — no hardcoded values needed.
- Resolves dynamic service URLs and injects them into the gateway.
- Enforces startup order via `WaitFor` — each service waits for its dependencies to pass health checks.
- Provides the Aspire Dashboard at `https://localhost:18888` with real-time logs, traces, and resource health.

---

## 12. Docker and docker-compose

For production or Docker-only environments (without Aspire), `docker-compose.yml` defines the entire stack:

```
Infrastructure containers:
  rabbitmq   → ports 5672 (AMQP), 15672 (management UI)
  postgres   → port 5432 (ecofleet_events database)
  sqlserver  → port 1433 (DriverDb, FleetDb, ManagerDb, AssignmentDb)

Microservice containers (built from Dockerfiles):
  driver-service      → port 5101
  fleet-service       → port 5102
  manager-service     → port 5103
  order-service       → port 5104
  assignment-service  → port 5105
  notification-service → no external port
  api-gateway         → port 5000
```

Each service container declares `depends_on` for its infrastructure containers. The multi-stage Dockerfiles compile from the SDK image and copy only the published output into the slim ASP.NET Core runtime image.

Connection strings in docker-compose override the appsettings defaults using service names as hostnames (`rabbitmq`, `postgres`, `sqlserver`).

---

## 13. Request Lifecycle — End-to-End

Here is what happens for a `POST /api/v1/drivers` request:

```
1. Client sends HTTP POST to https://gateway:5000/api/v1/drivers
   └── Body: { firstName, lastName, license, email, ... }

2. YARP matches "drivers-route" → forwards to DriverService:7145/api/v1/drivers

3. DriversController.CreateDriver([FromBody] CreateDriverCommand command)
   └── _sender.Send(command)  [MediatR]

4. MediatR Pipeline executes in order:
   ├── LoggingBehavior    → logs "Handling CreateDriverCommand"
   └── ValidationBehavior → runs CreateDriverValidator
        ├── FirstName not empty ✓
        ├── License format valid ✓
        └── Email format valid ✓

5. CreateDriverHandler.Handle(command, ct)
   ├── a. DriverAggregate.Create(firstName, lastName, license, email, ...)
   │        → RaiseEvent(new DriverCreatedStoreEvent(...))
   │              → _uncommittedEvents.Add(event)
   │              → Apply(event) → sets Id, FirstName, Status = Available, etc.
   │
   ├── b. _eventStore.SaveAsync(aggregate)
   │        → _session.Events.Append(aggregate.Id, [DriverCreatedStoreEvent])
   │        → _session.SaveChangesAsync()
   │              → appends event to PostgreSQL stream
   │              → DriverReadModelProjection.Create(event) runs inline
   │              → DriverReadModel document created in PostgreSQL
   │
   └── c. _publishEndpoint.Publish(new DriverCreatedIntegrationEvent { ... })
             → MassTransit sends message to RabbitMQ exchange

6. CreateDriverHandler returns driverId (Guid)

7. Controller returns HTTP 201 Created with { driverId } in body

8. RabbitMQ delivers DriverCreatedIntegrationEvent to subscribed consumers
   (NotificationService etc. react asynchronously)
```

---

## 14. Cross-Service Event Cascade — Suspend Driver Example

This is the most important event in the system. Suspending a driver cascades to four other services.

```
Client: PATCH /api/v1/drivers/suspend/{driverId}
            │
            ▼
    [YARP] → DriverService
            │
            ▼
    SuspendDriverHandler
     1. Load DriverAggregate from Marten (replays events)
     2. driver.Suspend()
           ├── Guard: throws DomainException if Status == OnDuty
           └── RaiseEvent(new DriverSuspendedStoreEvent)
                 → Status = Suspended, AssignedVehicleId = null (in memory)
     3. _eventStore.SaveAsync(driver)
           → Appends DriverSuspendedStoreEvent to PostgreSQL stream
           → Projection updates DriverReadModel: Status="Suspended", AssignedVehicleId=null
     4. _publishEndpoint.Publish(new DriverSuspendedIntegrationEvent { ... })
           → Message sent to RabbitMQ
            │
            ├──────────────────────────────────────────────────┐
            ▼                                                  ▼
    [NotificationService]                              [FleetService]
    DriverSuspendedConsumer                            DriverSuspendedConsumer
     → _notificationsService                           → query VehicleReadModel
       .SendDriverSuspendedNotification(...)             for vehicles with
       → SMTP email sent to driver                        CurrentDriverId == driverId
                                                        → foreach vehicle:
                                                            LoadAsync(vehicleId)
                                                            vehicle.UnassignDriver()
                                                            SaveAsync → appends
                                                              VehicleDriverUnassignedStoreEvent
            │
            ├───────────────────────────────┐
            ▼                               ▼
    [OrderService]                  [AssignmentService]
    DriverSuspendedConsumer         DriverSuspendedConsumer
     → find Pending orders with      → find active assignments
       DriverId == driverId            with DriverId == driverId
     → cancel each order             → deactivate each assignment
     → append OrderCancelledEvent    → save changes
```

All four consumers run in parallel (RabbitMQ delivers the same message to all subscribed queues simultaneously). Each service reacts independently without knowing about the others.

**FleetService also publishes a secondary event:**
When it saves `VehicleDriverUnassignedStoreEvent`, it publishes `VehicleDriverUnassignedIntegrationEvent` to RabbitMQ, which DriverService consumes to confirm the `AssignedVehicleId = null` state on its side.

---

## 15. Service-by-Service Reference

### DriverService

- **Domain model:** `DriverAggregate` (event-sourced), `DriverStatus` enum (Available / OnDuty / Suspended)
- **Store events:** Created, NameUpdated, LicenseUpdated, EmailUpdated, PhoneNumberUpdated, DateOfBirthUpdated, VehicleAssigned, VehicleUnassigned, Suspended, Reinstated (10 events)
- **Publishes:** `DriverCreated`, `DriverSuspended`, `DriverReinstated`, `DriverStatusChanged`
- **Consumes:** `VehicleDriverAssigned`, `VehicleDriverUnassigned`, `VehicleMaintenanceStarted`
- **Database:** SQL Server `EcoFleet_DriverDb` + PostgreSQL schema `driver_events`
- **API endpoints:** `GET /api/v1/drivers`, `GET /api/v1/drivers/{id}`, `POST`, `PUT/{id}`, `PATCH suspend/{id}`, `PATCH reinstate/{id}`, `DELETE/{id}`

### FleetService (Vehicles)

- **Domain model:** `VehicleAggregate` (event-sourced), `VehicleStatus` enum
- **Store events:** Created, PlateUpdated, LocationUpdated, DriverAssigned, DriverUnassigned, MaintenanceStarted
- **Publishes:** `VehicleCreated`, `VehicleDriverAssigned`, `VehicleDriverUnassigned`, `VehicleMaintenanceStarted`, `VehicleUpdated`
- **Consumes:** `DriverSuspended`
- **Database:** SQL Server `EcoFleet_FleetDb` + PostgreSQL schema `fleet_events`
- **API endpoints:** `GET /api/v1/vehicles`, `GET /{id}`, `POST`, `PUT/{id}`, `PATCH maintenance/{id}`, `DELETE/{id}`

### ManagerService

- **Domain model:** `Manager` entity (simple CRUD, no event sourcing)
- **Publishes:** nothing
- **Consumes:** nothing
- **Database:** SQL Server `EcoFleet_ManagerDb` only (no PostgreSQL, no RabbitMQ consumers)
- **API endpoints:** `GET /api/v1/managers`, `GET /{id}`, `POST`, `PUT/{id}`, `DELETE/{id}`
- **Note:** The simplest service — pure EF Core CRUD with MediatR and FluentValidation.

### OrderService

- **Domain model:** `OrderAggregate` (event-sourced), `OrderStatus` enum (Pending / InProgress / Completed / Cancelled)
- **Store events:** Created, Started, Completed, Cancelled
- **Publishes:** `OrderCreated`, `OrderCompleted`, `OrderCancelled`
- **Consumes:** `DriverSuspended` (to auto-cancel pending orders)
- **Database:** PostgreSQL schema `order_events` (Marten handles everything, no SQL Server dependency at runtime)
- **API endpoints:** `GET /api/v1/orders`, `GET /{id}`, `POST`, `PATCH start/{id}`, `PATCH complete/{id}`, `PATCH cancel/{id}`

### AssignmentService

- **Domain model:** `ManagerDriverAssignment` entity (simple CRUD). References `ManagerId` and `DriverId` as primitive `Guid` — no cross-boundary strong typing.
- **Publishes:** `AssignmentCreated`, `AssignmentDeactivated`
- **Consumes:** `DriverSuspended` (to deactivate assignments for suspended drivers)
- **Database:** SQL Server `EcoFleet_AssignmentDb`
- **API endpoints:** `GET /api/v1/managerdriverassignments`, `GET /{id}`, `POST`, `PATCH deactivate/{id}`, `DELETE/{id}`

### NotificationService

- **Domain model:** none
- **Publishes:** nothing
- **Consumes:** `DriverSuspended`, `DriverReinstated`, `OrderCompleted`
- **Database:** none — purely event-driven, stateless
- **API endpoints:** none (only `/health`)
- **Implementation:** Uses SMTP to send emails. The email logic from the original monolith's `EcoFleet.Emails` project lives here.

### ApiGateway

- **Technology:** YARP (Yet Another Reverse Proxy)
- **Responsibility:** Routes incoming HTTP requests by path prefix to the correct microservice.
- **Aspire integration:** Reads dynamically injected service URLs from environment variables, overriding the static appsettings.json cluster addresses at startup.
- **No authentication** is implemented at the gateway level in this version.

---

## Key Architectural Rules — Summary

| Rule | Why |
|---|---|
| No service reads another service's database | Prevents coupling; each service evolves independently |
| No service imports another service's domain assembly | Domain concepts must not leak across boundaries |
| Integration events carry only primitive types (`Guid`, `string`, `DateTime`) | Avoids creating implicit dependencies on domain types |
| Domain events stay inside a service; integration events travel between services | Domain events are internal implementation details |
| Aggregate state is never set directly — always flows through `RaiseEvent` | Ensures the event stream is always the complete and accurate history |
| Each service has its own `DbContext` targeting its own database | Enforces the database-per-service pattern at the code level |
| Outbox pattern for domain events; direct MassTransit publish for integration events | Outbox ensures internal domain event handlers never miss an event; direct publish is sufficient for external events since Marten save + MassTransit publish are both fast |
| YARP gateway preserves the original API paths | Clients of the old monolith need zero changes to their request URLs |
