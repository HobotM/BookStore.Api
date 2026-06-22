using BookStore.Api.Data;
using BookStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Repositories;

public sealed class EfBookRepository : IBookRepository
{
    private readonly BookStoreDbContext _context;
    private readonly ILogger<EfBookRepository> _logger;

    public EfBookRepository(
        BookStoreDbContext context,
        ILogger<EfBookRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IReadOnlyList<Book> GetAll()
    {
        var books = _context.Books
            .AsNoTracking()
            .OrderBy(book => book.Id)
            .ToList();

        _logger.LogInformation(
            "Loaded all books from database. Count: {BooksCount}",
            books.Count);

        return books;
    }

    public Book? GetById(int id)
    {
        _logger.LogInformation(
            "Loading book from database. BookId: {BookId}",
            id);

        return _context.Books
            .AsNoTracking()
            .FirstOrDefault(book => book.Id == id);
    }

    public async Task<Book> AddAsync(Book book)
    {
        _context.Books.Add(book);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Book saved to database. BookId: {BookId}, Title: {Title}",
            book.Id,
            book.Title);

        return book;
    }
}