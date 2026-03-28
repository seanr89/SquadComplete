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

// 2. Your Custom Services - Db and Api services
builder.Services.AddDbContext<SquadContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddHttpClient<IApiService, ApiService>();
builder.Services.AddScoped<DatabaseService>();

builder.Build().Run();
