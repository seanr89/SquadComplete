using Microsoft.EntityFrameworkCore;
using squad_api.Models;
using squad_api.Endpoints;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors();

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
