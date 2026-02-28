using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// 2. Override YARP cluster destinations with Aspire-injected service URLs (if running under Aspire).
//    When running under Aspire AppHost, environment variables are injected as:
//      services__driver-service__https__0  (or http)
//    These map to configuration keys:  services:driver-service:https:0
//    We override the YARP config so hardcoded ports in appsettings.json are not used.
var serviceMap = new Dictionary<string, string>
{
    ["driver-service"]     = "driver-service",
    ["fleet-service"]      = "fleet-service",
    ["manager-service"]    = "manager-service",
    ["order-service"]      = "order-service",
    ["assignment-service"] = "assignment-service"
};

foreach (var (cluster, service) in serviceMap)
{
    var url = builder.Configuration[$"services:{service}:https:0"]
           ?? builder.Configuration[$"services:{service}:http:0"];

    if (url is not null)
    {
        builder.Configuration[$"ReverseProxy:Clusters:{cluster}:Destinations:destination1:Address"] = url;
    }
}

// 3. YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 4. Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapReverseProxy();
app.MapHealthChecks("/health");

app.Run();
