using BookStore.Api.Models;
using BookStore.Api.Repositories;

namespace BookStore.Api.Services;

public sealed class BookService
{
    private readonly ILogger<BookService> _logger;
    private readonly IBookRepository _bookRepository;

    public BookService(
        ILogger<BookService> logger,
        IBookRepository bookRepository)
    {
        _logger = logger;
        _bookRepository = bookRepository;
    }

    public IReadOnlyCollection<Book> GetAll()
    {
        _logger.LogInformation("Getting all books");

        return _bookRepository.GetAll();
    }

    public Book? GetById(int id)
    {
        _logger.LogInformation("Getting book with id {BookId}", id);

        var book = _bookRepository.GetById(id);

        if (book is null)
        {
            _logger.LogWarning("Book with id {BookId} was not found", id);
        }

        return book;
    }

    public Book Create(string title, string author, decimal price)
    {
        if (price <= 0)
        {
            _logger.LogWarning("Invalid book price {Price}", price);
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        }

        var nextId = _bookRepository.GetNextId();

        var book = new Book
        {
            Id = nextId,
            Title = title,
            Author = author,
            Price = price
        };

        var createdBook = _bookRepository.Add(book);

        _logger.LogInformation(
            "Created book {BookId} with title {BookTitle}",
            createdBook.Id,
            createdBook.Title);

        return createdBook;
    }

    public void SimulateError()
    {
        
            throw new InvalidOperationException("Simulated database error.");
    }
}