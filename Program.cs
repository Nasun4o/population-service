using Microsoft.EntityFrameworkCore;
using PopulationService.Data;
using PopulationService.ExternalServices;
using PopulationService.Interfaces;
using PopulationService.Services;

var builder = WebApplication.CreateBuilder(args);

// EF Core - SQLite
builder.Services.AddDbContext<PopulationDbContext>(options =>
    options.UseSqlite("Data Source=citystatecountry.db"));

// Services
builder.Services.AddScoped<IPopulationRepository, PopulationRepository>();
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

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
