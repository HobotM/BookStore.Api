using BookStore.Api.Events;
using BookStore.Api.Models;
using BookStore.Api.Publishers;
using BookStore.Api.Repositories;
using System.Text.Json;
using BookStore.Api.Data;


namespace BookStore.Api.Services;

public sealed class BookService
{
    private readonly ILogger<BookService> _logger;
    private readonly IBookRepository _bookRepository;
    private readonly IBookEventPublisher _bookEventPublisher;
    private readonly BookStoreDbContext _dbContext;

    public BookService(
        ILogger<BookService> logger,
        IBookRepository bookRepository,
        IBookEventPublisher bookEventPublisher,
        BookStoreDbContext dbContext)
    {
        _logger = logger;
        _bookRepository = bookRepository;
        _bookEventPublisher = bookEventPublisher;
        _dbContext = dbContext;
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

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _bookRepository.AddAsync(book, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);


        var domainEvent = new BookCreatedEvent(
            book.Id,
            book.Title,
            book.Author,
            book.Price,
            DateTime.UtcNow);

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = nameof(BookCreatedEvent),
            Content = JsonSerializer.Serialize(domainEvent),
            OccurredAtUtc = domainEvent.OccurredAtUtc
        };
        await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return book;
    }

    public void SimulateError()
    {
        throw new InvalidOperationException("Simulated database error.");
    }
}