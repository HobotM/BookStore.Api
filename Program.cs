using BookStore.Api.EventHandlers;
using BookStore.Api.Events;
using BookStore.Api.Middlewares;
using BookStore.Api.Repositories;
using BookStore.Api.Services;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Serilog;
using System.Diagnostics;
using BookStore.Api.Data;
using Microsoft.EntityFrameworkCore;
using BookStore.Api.Publishers;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<IBookRepository, EfBookRepository>();
builder.Services.AddSingleton<AuditSubscriber>();
builder.Services.AddSingleton<BookCreatedAuditHandler>();
builder.Services.AddSingleton<BookCreatedEmailHandler>();
builder.Services.AddSingleton<BlobStorageService>();

builder.Services.AddDbContext<BookStoreDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString);
});
builder.Services.AddSingleton<IBookEventPublisher, AzureServiceBusBookEventPublisher>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string ActivitySourceName = "BookStore.Api";
var bookStoreActivitySource = new ActivitySource(ActivitySourceName);

var applicationInsightsConnectionString =
    builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    builder.Services
        .AddOpenTelemetry()
        .UseAzureMonitor(options =>
        {
            options.ConnectionString = applicationInsightsConnectionString;
        })
        .WithTracing(tracing =>
        {
            tracing.AddSource(ActivitySourceName);
        });
}


var app = builder.Build();


app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/", () =>
{
    return Results.Ok("BookStore.Api is running from Azure - v2");
});



app.Run();
