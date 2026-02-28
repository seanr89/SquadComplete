using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using squad_func.Models;
using squad_func.Services;

var builder = FunctionsApplication.CreateBuilder(args);

// 1. Core Functions setup
builder.ConfigureFunctionsWebApplication();

// 2. Telemetry - Order matters here for the Worker
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

// 3. Your Custom Services
builder.Services.AddDbContext<SquadContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Warning));

builder.Services.AddHttpClient<IApiService, ApiService>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

builder.Build().Run();
