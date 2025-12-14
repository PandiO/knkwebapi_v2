using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using knkwebapi_v2.DependencyInjection; // added for DI extensions
using System;
using System.Linq;
using System.Text.Json.Serialization;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        // Add converter for enums to be serialized/deserialized as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Prevent automatic 400 so we can handle invalid enums (e.g., "Draft") gracefully in controllers.
        options.SuppressModelStateInvalidFilter = true;
    });

// Bind Kestrel to URLs from config or default to LAN-accessible HTTP
var urlConfig = builder.Configuration["ASPNETCORE_URLS"] ?? builder.Configuration["Urls"];
if (!string.IsNullOrWhiteSpace(urlConfig))
{
    builder.WebHost.UseUrls(urlConfig);
}
else
{
    builder.WebHost.UseUrls("http://0.0.0.0:5000");
}

string? connectionString = builder.Configuration.GetConnectionString("MySqlDbConnection");
builder.Services.AddDbContext<KnKDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Register app services/repositories in one place
builder.Services.AddApplicationServices(builder.Configuration);

// Health checks: add liveness/readiness
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Options binding for telemetry and client activity
builder.Services.Configure<knkwebapi_v2.Configuration.TelemetryOptions>(builder.Configuration.GetSection(knkwebapi_v2.Configuration.TelemetryOptions.SectionName));
builder.Services.Configure<knkwebapi_v2.Configuration.ClientActivityOptions>(builder.Configuration.GetSection(knkwebapi_v2.Configuration.ClientActivityOptions.SectionName));

// Register in-memory client activity store
builder.Services.AddSingleton<knkwebapi_v2.Services.Interfaces.IClientActivityStore>(sp =>
{
    var opts = builder.Configuration.GetSection(knkwebapi_v2.Configuration.ClientActivityOptions.SectionName).Get<knkwebapi_v2.Configuration.ClientActivityOptions>()
               ?? new knkwebapi_v2.Configuration.ClientActivityOptions();
    return new knkwebapi_v2.Services.InMemoryClientActivityStore(opts.MaxClients, opts.CleanupInterval);
});

// OpenTelemetry setup per configuration
var telemetryOptions = builder.Configuration.GetSection(knkwebapi_v2.Configuration.TelemetryOptions.SectionName)
    .Get<knkwebapi_v2.Configuration.TelemetryOptions>() ?? new knkwebapi_v2.Configuration.TelemetryOptions();

if (telemetryOptions.Enabled)
{
    builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            if (string.Equals(telemetryOptions.Exporter, "otlp", StringComparison.OrdinalIgnoreCase)
                && telemetryOptions.Otlp.EnableMetrics)
            {
                metrics.AddOtlpExporter(o => { o.Endpoint = new Uri(telemetryOptions.Otlp.Endpoint); });
            }
        })
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            if (string.Equals(telemetryOptions.Exporter, "otlp", StringComparison.OrdinalIgnoreCase)
                && telemetryOptions.Otlp.EnableTracing)
            {
                tracing.AddOtlpExporter(o => { o.Endpoint = new Uri(telemetryOptions.Otlp.Endpoint); });
            }
        });
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod();
        });
});

// Placeholder admin authorization policy.
// TODO: Bind this to your real authentication/authorization setup.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
    {
        // Placeholder: allow all in Development, require role claim otherwise.
        policy.RequireAssertion(ctx =>
            ctx.User?.IsInRole("Admin") == true || ctx.User?.Claims.Any(c => c.Type == "role" && c.Value == "Admin") == true);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapRazorPages();
app.UseRouting();

// Client activity tracking middleware
app.UseMiddleware<knkwebapi_v2.Middleware.ClientActivityMiddleware>();

// Only redirect to HTTPS if an HTTPS listener is configured
var configuredUrls = (urlConfig ?? string.Empty)
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
bool hasHttpsListener = configuredUrls.Any(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
if (hasHttpsListener)
{
    app.UseHttpsRedirection();
}

// TODO: Prometheus exporter endpoint can be added with OpenTelemetry Prometheus AspNetCore package

app.MapControllers();

app.Run();

