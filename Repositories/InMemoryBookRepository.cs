using BookStore.Api.Models;

namespace BookStore.Api.Repositories;

public sealed class InMemoryBookRepository : IBookRepository
{
    private readonly ILogger<InMemoryBookRepository> _logger;

    private readonly List<Book> _books =
    [
        new Book { Id = 1, Title = "Clean Code", Author = "Robert C. Martin", Price = 120 },
        new Book { Id = 2, Title = "The Pragmatic Programmer", Author = "Andrew Hunt", Price = 140 },
        new Book { Id = 3, Title = "Domain-Driven Design", Author = "Eric Evans", Price = 180 }
    ];

    public InMemoryBookRepository(ILogger<InMemoryBookRepository> logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<Book> GetAll()
    {
        // TODO:
        // Zaloguj Information:
        // "Loading all books from repository. Count: {BooksCount}"
        _logger.LogInformation("Loading all books from repository. Count: {BooksCount}", _books.Count);
        return _books;
    }

    public Book? GetById(int id)
    {
        // TODO:
        // Zaloguj Debug:
        // "Searching book with id {BookId} in repository"
        _logger.LogDebug("Searching book with {BookId}", id);
        return _books.FirstOrDefault(x => x.Id == id);
    }

    public Book Add(Book book)
    {
        // TODO:
        // Zaloguj Information:
        // "Adding book {BookId} to repository"
        _logger.LogInformation("Adding book {BookId} to repository", book.Id);
        _books.Add(book);

        return book;
    }

    public int GetNextId()
    {
        return _books.Max(x => x.Id) + 1;
    }
}