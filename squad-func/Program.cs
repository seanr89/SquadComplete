using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Diagnostics;
using squad_func.Models;
using squad_func.Services;

var builder = FunctionsApplication.CreateBuilder(args);

// 1. Core Functions setup
builder.ConfigureFunctionsWebApplication();

// 2. Your Custom Services - Db and Api services
builder.Services.AddDbContext<SquadContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.CommandExecuted));
});

builder.Services.AddHttpClient<GeminiService>(client => client.Timeout = TimeSpan.FromSeconds(160));
builder.Services.AddTransient<StorageService>();
builder.Services.AddHttpClient<IApiService, ApiService>(client => client.Timeout = TimeSpan.FromSeconds(160));
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<AgentMappingService>();

builder.Build().Run();
