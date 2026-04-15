using Microsoft.EntityFrameworkCore;
using PopulationService.Data;
using PopulationService.ExternalServices;
using PopulationService.Interfaces;
using PopulationService.Services;

var builder = WebApplication.CreateBuilder(args);

// EF Core - SQLite
var connectionString = builder.Configuration.GetConnectionString("PopulationDb")
    ?? throw new InvalidOperationException("Connection string 'PopulationDb' not found.");

builder.Services.AddDbContext<CountryPopulationDbContext>(options =>
    options.UseSqlite(connectionString));

// Services
builder.Services.AddScoped<ICountryPopulationRepository, CountryPopulationRepository>();
builder.Services.AddScoped<ICountryStatsService, CountryStatsService>();
builder.Services.AddScoped<IPopulationAggregationService, PopulationAggregationService>();

// Web API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program { }
