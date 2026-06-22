using BookStore.Api.EventHandlers;
using BookStore.Api.Events;
using BookStore.Api.Middlewares;
using BookStore.Api.Repositories;
using BookStore.Api.Services;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Serilog;
using System.Diagnostics;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddSingleton<BookService>();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddSingleton<AuditSubscriber>();
builder.Services.AddSingleton<BookCreatedAuditHandler>();
builder.Services.AddSingleton<BookCreatedEmailHandler>();
builder.Services.AddSingleton<BlobStorageService>();


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

app.MapGet("/config", (IConfiguration configuration, IWebHostEnvironment environment) =>
{
    var welcomeMessage = configuration["MyApp:WelcomeMessage"];

    return Results.Ok(new
    {
        Environment = environment.EnvironmentName,
        WelcomeMessage = welcomeMessage
    });
});

app.MapGet("/connection", (IConfiguration configuration) =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    return Results.Ok(new
    {
        HasConnectionString = !string.IsNullOrWhiteSpace(connectionString),
        ConnectionStringPreview = string.IsNullOrWhiteSpace(connectionString)
            ? null
            : connectionString[..Math.Min(connectionString.Length, 20)] + "..."
    });
});

app.MapGet("/books", (BookService bookService) =>
{
    var books = bookService.GetAll();

    return Results.Ok(books);
});

app.MapGet("/books/{id:int}", (int id, BookService bookService) =>
{
    var book = bookService.GetById(id);

    return book is null
        ? Results.NotFound()
        : Results.Ok(book);
});

app.MapGet("/simulate-error", (BookService bookService) =>
{
    bookService.SimulateError();

    return Results.Ok();
});

app.MapPost("/books", async (
    CreateBookRequest request,
    BookService bookService,
    ILogger<Program> logger) =>
{
    using var activity = bookStoreActivitySource.StartActivity("BookCreated");

    var book = await bookService.CreateAsync(
        request.Title,
        request.Author,
        request.Price);

    activity?.SetTag("book.id", book.Id);
    activity?.SetTag("book.title", book.Title);
    activity?.SetTag("book.author", book.Author);

    logger.LogInformation(
        "Book created. BookId: {BookId}, Title: {Title}",
        book.Id,
        book.Title);

    

    return Results.Created($"/books/{book.Id}", book);
});


app.MapGet("/secret-test", (IConfiguration configuration) =>
{
    var apiKey = configuration["BookStore:ApiKey"];

    return Results.Ok (new
    {
        HasApiKey = !string.IsNullOrWhiteSpace(apiKey),
        ApiKeyPreview = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey[..Math.Min(apiKey.Length, 5)]+ "..."
    });


});



app.Run();

public sealed record CreateBookRequest(
    string Title,
    string Author,
    decimal Price);