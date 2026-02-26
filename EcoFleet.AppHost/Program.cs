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

// Microservices
var driverService = builder.AddProject<Projects.EcoFleet_DriverService_API>("driver-service")
    .WithReference(rabbitMq)
    .WithReference(eventsDb)
    .WithReference(driverDb);

var fleetService = builder.AddProject<Projects.EcoFleet_FleetService_API>("fleet-service")
    .WithReference(rabbitMq)
    .WithReference(eventsDb)
    .WithReference(fleetDb);

var managerService = builder.AddProject<Projects.EcoFleet_ManagerService_API>("manager-service")
    .WithReference(rabbitMq)
    .WithReference(managerDb);

var orderService = builder.AddProject<Projects.EcoFleet_OrderService_API>("order-service")
    .WithReference(rabbitMq)
    .WithReference(eventsDb);

var assignmentService = builder.AddProject<Projects.EcoFleet_AssignmentService_API>("assignment-service")
    .WithReference(rabbitMq)
    .WithReference(assignmentDb);

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
