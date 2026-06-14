using BookStore.Api.Services;
using Serilog;
using Swashbuckle;
using BookStore.Api.Repositories;
using BookStore.Api.Middlewares;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/bookstore-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSingleton<BookService>();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

app.MapPost("/books", (CreateBookRequest request, BookService bookService) =>
{
    var book = bookService.Create(
        request.Title,
        request.Author,
        request.Price);

    return Results.Created($"/books/{book.Id}", book);
});

app.MapGet("/simulate-error", (BookService bookService) =>
{
    bookService.SimulateError();

    return Results.Ok();
});

app.Run();

public sealed record CreateBookRequest(
    string Title,
    string Author,
    decimal Price);