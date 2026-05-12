using BhDream.Application.Abstractions.ExternalServices;
using BhDream.Application.Abstractions.Repositories;
using BhDream.Application.Helpers;
using BhDream.Application.Services;
using BhDream.Application.Services.Contracts;
using BhDream.Infrastructure.ExternalServices.Messaging;
using BhDream.Infrastructure.Persistence;
using BhDream.Infrastructure.Repositories;
using BhDream.WebAPI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<QuantDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("BhDream.Infrastructure"))
    );
builder.Services.AddScoped<IOptionHistoryRepository, OptionHistoryRepository>();
builder.Services.AddScoped<IUnderlyingRepository, UnderlyingRepository>();
builder.Services.AddScoped<IOptionContractRepository, OptionContractRepository>();
builder.Services.AddScoped<IRiskFreeRateRepository, RiskFreeRateRepository>();
builder.Services.AddScoped<IOptionPricingParameterSnapshotRepository, OptionPricingParameterSnapshotRepository>();
builder.Services.AddScoped<IOptionHistoryRfrSyncRepository, OptionHistoryRfrSyncRepository>();
builder.Services.AddScoped<IOptionGreeksAndIvRepository, OptionGreeksAndIvRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IOptionCsvImportService, OptionCsvImportService>();
builder.Services.AddScoped<IRfrCsvImportService, RfrCsvImportService>();
builder.Services.AddScoped<IOptionHistoryCsvParser, OptionHistoryCsvParser>();
builder.Services.AddScoped<IRfrCsvParser, RfrCsvParser>();
builder.Services.AddScoped<IOptionsAnalyticsService, OptionsAnalyticsService>();
builder.Services.AddScoped<IOptionProcessingService, OptionProcessingService>();

builder.Services.AddSingleton<IOptionPricingDispatcher, ZmqOptionPricingDispatcher>();
builder.Services.AddSingleton<IOptionGreeksResultReceiver, ZmqOptionGreeksResultReceiver>();
builder.Services.AddHostedService<OptionGreekCalculationParameterFeeder>();
builder.Services.AddHostedService<OptionGreekCalculationResultCollector>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();


app.UseHttpsRedirection();
app.UseCors();

app.MapControllers(); 
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}



app.Run();
