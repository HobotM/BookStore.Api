using BookStore.Api.Models;
using BookStore.Api.Repositories;
using BookStore.Api.EventHandlers;
using BookStore.Api.Events;


namespace BookStore.Api.Services;

public sealed class BookService
{
    private readonly ILogger<BookService> _logger;
    private readonly IBookRepository _bookRepository;
    private readonly BookCreatedAuditHandler _auditHandler;
    private readonly BookCreatedEmailHandler _emailHandler;


    
    public BookService(
        ILogger<BookService> logger,
        IBookRepository bookRepository, BookCreatedAuditHandler auditHandler,BookCreatedEmailHandler emailHandler )
    {
        _logger = logger;
        _bookRepository = bookRepository;
        _auditHandler = auditHandler;
        _emailHandler = emailHandler;
        
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
            _logger.LogWarning(
                "Book with id {BookId} was not found",
                id);

            throw new KeyNotFoundException(
                $"Book with id {id} was not found.");
        }

        return book;
    }

   public async Task<Book> CreateAsync(string title, string author, decimal price)
{
    if (price <= 0)
    {
        _logger.LogWarning("Invalid book price {Price}", price);
        throw new ArgumentException(
            "Price must be greater than zero.",
            nameof(price));
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

    var domainEvent = new BookCreatedEvent(
    createdBook.Id,
    createdBook.Title,
    createdBook.Author,
    createdBook.Price,
    DateTime.UtcNow);

    await _auditHandler.HandleAsync(domainEvent);    
    await _emailHandler.HandleAsync(domainEvent);    


    return createdBook;
}
    public void SimulateError()
    {
        
            throw new InvalidOperationException("Simulated database error.");
    }


}