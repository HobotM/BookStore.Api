using BookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookStore.Api.Controllers;

[ApiController]
[Route("books")]
public sealed class BooksController : ControllerBase
{
    private const string ActivitySourceName = "BookStore.Api";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private readonly BookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(
        BookService bookService,
        ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var books = await _bookService.GetAllAsync(cancellationToken);

        return Ok(books);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(
            id,
            cancellationToken);

        if (book is null)
        {
            return NotFound(new
            {
                Message = $"Book with id {id} was not found."
            });
        }

        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        CreateBookRequest request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("BookCreated");

        var book = await _bookService.CreateAsync(
            request.Title,
            request.Author,
            request.Price,
            cancellationToken);

        activity?.SetTag("book.id", book.Id);
        activity?.SetTag("book.title", book.Title);
        activity?.SetTag("book.author", book.Author);

        _logger.LogInformation(
            "Book created through BooksController. BookId: {BookId}",
            book.Id);

        return Created($"/books/{book.Id}", book);
    }
}

public sealed record CreateBookRequest(
    string Title,
    string Author,
    decimal Price);