using EcoFleet.ManagerService.API.Middlewares;
using EcoFleet.ManagerService.Application;
using EcoFleet.ManagerService.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// 2. Application Layer (MediatR + FluentValidation)
builder.Services.AddManagerApplication();

// 3. Infrastructure Layer (EF Core + own database)
builder.Services.AddManagerInfrastructure(builder.Configuration);

// 4. API Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 5. Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("ManagerDb")!);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.MapOpenApi();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
