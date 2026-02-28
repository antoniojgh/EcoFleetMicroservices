var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure
var rabbitMq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var postgres = builder.AddPostgres("postgres");
var eventsDb = postgres.AddDatabase("EventStore");

var sqlServer = builder.AddSqlServer("sqlserver");
var driverDb = sqlServer.AddDatabase("DriverDb");
var fleetDb = sqlServer.AddDatabase("FleetDb");
var managerDb = sqlServer.AddDatabase("ManagerDb");
var assignmentDb = sqlServer.AddDatabase("AssignmentDb");

// Microservices — WaitFor ensures infrastructure is healthy before each service starts
var driverService = builder.AddProject<Projects.EcoFleet_DriverService_API>("driver-service")
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(eventsDb).WaitFor(eventsDb)
    .WithReference(driverDb).WaitFor(driverDb);

var fleetService = builder.AddProject<Projects.EcoFleet_FleetService_API>("fleet-service")
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(eventsDb).WaitFor(eventsDb)
    .WithReference(fleetDb).WaitFor(fleetDb);

var managerService = builder.AddProject<Projects.EcoFleet_ManagerService_API>("manager-service")
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(managerDb).WaitFor(managerDb);

var orderService = builder.AddProject<Projects.EcoFleet_OrderService_API>("order-service")
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(eventsDb).WaitFor(eventsDb);

var assignmentService = builder.AddProject<Projects.EcoFleet_AssignmentService_API>("assignment-service")
    .WithReference(rabbitMq).WaitFor(rabbitMq)
    .WithReference(assignmentDb).WaitFor(assignmentDb);

var notificationService = builder.AddProject<Projects.EcoFleet_NotificationService_API>("notification-service")
    .WithReference(rabbitMq).WaitFor(rabbitMq);

// API Gateway — waits for all microservices to be ready before routing traffic
builder.AddProject<Projects.EcoFleet_ApiGateway>("api-gateway")
    .WithReference(driverService).WaitFor(driverService)
    .WithReference(fleetService).WaitFor(fleetService)
    .WithReference(managerService).WaitFor(managerService)
    .WithReference(orderService).WaitFor(orderService)
    .WithReference(assignmentService).WaitFor(assignmentService);

builder.Build().Run();
