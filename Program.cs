using BookStore.Api.Services;
using Serilog;
using BookStore.Api.Repositories;
using BookStore.Api.Middlewares;
using BookStore.Api.Events;
using BookStore.Api.EventHandlers;
using Microsoft.ApplicationInsights;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(
            context.Configuration);
    });



builder.Services.AddSingleton<BookService>();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddSingleton<AuditSubscriber>();
builder.Services.AddSingleton<BookCreatedAuditHandler>();
builder.Services.AddSingleton<BookCreatedEmailHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry();




var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();


app.MapGet("/", () => Results.Ok("BookStore.Api is running from Azure - v2"));

app.MapGet("/config", (IConfiguration configuration, IWebHostEnvironment environment) =>
{
   var welcomeMessage = configuration["MyApp:WelcomeMessage"]; 

    return Results.Ok( new
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
        ConnectionStringPreview = string.IsNullOrWhiteSpace(connectionString) ? null : connectionString[..Math.Min(connectionString.Length, 20)] + "..."
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


app.MapPost("/books", async(CreateBookRequest request, BookService bookService, TelemetryClient telemetryClient) =>
{
    var book = await bookService.CreateAsync(request.Title, request.Author, request.Price);

    telemetryClient.TrackEvent("BookCreated", new Dictionary<string, string>
    {
        ["BookId"] = book.Id.ToString(),
        ["Title"] = book.Title,
        ["Author"] = book.Author

    });

    return Results.Created($"/books/{book.Id}", book);
});






app.Run();




public sealed record CreateBookRequest(
    string Title,
    string Author,
    decimal Price);