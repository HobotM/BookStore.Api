using BookStore.Api.Models;

namespace BookStore.Api.Service;

public sealed class BookService
{
    private readonly ILogger<BookService> _logger;
     private readonly List<Book> _books =
    [
        new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 120 },
        new Book { Id = 2, Title = "The Pragmatic Programmer", Author = "Andrew Hunt", Price = 140 },
        new Book { Id = 3, Title = "Domain-Driven Design", Author = "Eric Evans", Price = 180 }
    ];

    public BookService(ILogger<BookService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<Book> GetAll()
    {
        // TODO 1:
        // Zaloguj Information:
        // "Getting all books. Count: {BooksCount}"

         _logger.LogInformation("Getting all books. Count: {BooksCount}", _books.Count);

        return _books;
    }

    public Book? GetById(int id)
    {
        // TODO 2:
        // Zaloguj Information:
        // "Getting book with id {BookId}"
        _logger.LogInformation("Getting book with id {BookId}", id);

        var book = _books.FirstOrDefault(x => x.Id == id);

        if (book is null)
        {
            // TODO 3:
            // Zaloguj Warning:
            // "Book with id {BookId} was not found"
            _logger.LogWarning("Book with id {BookId} was not found", id);
        }

        return book;
    }


    public Book Create(string title, string author, decimal price)
    {
        // TODO 4:
        // Jeśli price <= 0, zaloguj Warning z ceną:
        // "Invalid book price {Price}"


        if (price <= 0)
        {
            _logger.LogWarning("Invalid book price {Price}", price);
            throw new ArgumentException("Price must be greater than zero.", nameof(price));
        }

        var nextId = _books.Max(x => x.Id) + 1;

        var book = new Book
        {
            Id = nextId,
            Title = title,
            Author = author,
            Price = price
        };

        _books.Add(book);

        // TODO 5:
        // Zaloguj Information:
        // "Created book {BookId} with title {BookTitle}"
        _logger.LogInformation("Created book {BookId} with title {BookTitle}", book.Id, book.Title);

        return book;
    }


    public void SimulateError()
    {
        try
        {
            throw new InvalidOperationException("Simulated database error.");
        }
        catch (Exception ex)
        {
            // TODO 6:
            // Zaloguj Error z wyjątkiem:
            // "Error while simulating database operation"
            _logger.LogError(ex, "Error while simulating database operation");

            throw;
        }
    }






}