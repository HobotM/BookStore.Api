using BookStore.Api.Services;
using Serilog;
using BookStore.Api.Repositories;
using BookStore.Api.Middlewares;
using BookStore.Api.Events;
using BookStore.Api.EventHandlers;

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
builder.Services.AddSingleton<BookCreatedAuditHandler>();
builder.Services.AddSingleton<BookCreatedEmailHandler>();



var app = builder.Build();

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



app.MapGet("/simulate-error", (BookService bookService) =>
{
    bookService.SimulateError();
    return Results.Ok();
});


app.MapPost("/books", async(CreateBookRequest request, BookService bookService) =>
{
    var book = await bookService.CreateAsync(request.Title, request.Author, request.Price);

    return Results.Created($"/books/{book.Id}", book);
});






app.Run();




public sealed record CreateBookRequest(
    string Title,
    string Author,
    decimal Price);