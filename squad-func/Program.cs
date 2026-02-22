using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using squad_func.Models;
using squad_func.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddDbContext<SquadContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")))
    .AddHttpClient<IApiService, ApiService>()
    .Services.AddScoped<IDatabaseService, DatabaseService>();

builder.Build().Run();
