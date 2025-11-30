using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using knkwebapi_v2.DependencyInjection; // added for DI extensions
using System;
using System.Linq;
using System.Text.Json.Serialization;

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

// Only redirect to HTTPS if an HTTPS listener is configured
var configuredUrls = (urlConfig ?? string.Empty)
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
bool hasHttpsListener = configuredUrls.Any(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
if (hasHttpsListener)
{
    app.UseHttpsRedirection();
}
app.MapControllers();

app.Run();
