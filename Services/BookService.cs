using BookStore.Api.EventHandlers;
using BookStore.Api.Events;
using BookStore.Api.Models;
using BookStore.Api.Repositories;

namespace BookStore.Api.Services;

public sealed class BookService
{
    private readonly ILogger<BookService> _logger;
    private readonly IBookRepository _bookRepository;
    private readonly BookCreatedAuditHandler _auditHandler;
    private readonly BookCreatedEmailHandler _emailHandler;

    public BookService(
        ILogger<BookService> logger,
        IBookRepository bookRepository,
        BookCreatedAuditHandler auditHandler,
        BookCreatedEmailHandler emailHandler)
    {
        _logger = logger;
        _bookRepository = bookRepository;
        _auditHandler = auditHandler;
        _emailHandler = emailHandler;
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all books");

        return await _bookRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting book with id {BookId}", id);

        var book = await _bookRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (book is null)
        {
            _logger.LogWarning(
                "Book with id {BookId} was not found",
                id);
        }

        return book;
    }

    public async Task<Book> CreateAsync(
        string title,
        string author,
        decimal price,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            _logger.LogWarning("Book title was empty");

            throw new ArgumentException(
                "Title is required.",
                nameof(title));
        }

        if (string.IsNullOrWhiteSpace(author))
        {
            _logger.LogWarning("Book author was empty");

            throw new ArgumentException(
                "Author is required.",
                nameof(author));
        }

        if (price <= 0)
        {
            _logger.LogWarning("Invalid book price {Price}", price);

            throw new ArgumentException(
                "Price must be greater than zero.",
                nameof(price));
        }

        var book = new Book
        {
            Title = title.Trim(),
            Author = author.Trim(),
            Price = price
        };

        var createdBook = await _bookRepository.AddAsync(
            book,
            cancellationToken);

        var domainEvent = new BookCreatedEvent(
            createdBook.Id,
            createdBook.Title,
            createdBook.Author,
            createdBook.Price,
            DateTime.UtcNow);

        await _auditHandler.HandleAsync(domainEvent);
        await _emailHandler.HandleAsync(domainEvent);

        _logger.LogInformation(
            "Book created successfully. BookId: {BookId}, Title: {Title}",
            createdBook.Id,
            createdBook.Title);

        return createdBook;
    }

    public void SimulateError()
    {
        throw new InvalidOperationException("Simulated database error.");
    }
}