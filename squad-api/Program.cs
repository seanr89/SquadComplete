using Microsoft.EntityFrameworkCore;
using squad_api.Models;
using squad_api.Endpoints;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using squad_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<SquadContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<GameRecordService>();

// Restrict CORS to the known squad-draft frontend origin(s), configured per-environment
// via appsettings (Cors:AllowedOrigins) rather than allowing any origin.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Per-IP rate limiting for public write endpoints (feedback, events, user squad submissions)
// that have no authentication and would otherwise be spammable.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("PublicWrite", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var app = builder.Build();

app.UseCors();
app.UseRateLimiter();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// All Service endpoints handled here
app.MapAllEndpoints();

app.Run();
