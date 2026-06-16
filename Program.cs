using BookStore.Api.Services;
using Serilog;
using BookStore.Api.Repositories;
using BookStore.Api.Middlewares;
using BookStore.Api.Models;
using System.Reflection;
using BookStore.Api.Events;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(
            context.Configuration);
    });



builder.Services.AddSingleton<BookService>();
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddSingleton<AuditSubscriber>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var bookService = app.Services.GetRequiredService<BookService>();
var auditSubscriber = app.Services.GetRequiredService<AuditSubscriber>();

bookService.BookCreated += book =>
{
    Log.Information(
        "EVENT: Book created. Id: {BookId}, Title: {BookTitle}",
        book.Id,
        book.Title);
};

bookService.BookCreated += auditSubscriber.onBookCreated;




app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
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