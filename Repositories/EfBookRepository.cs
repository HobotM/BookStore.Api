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

    public async Task<IReadOnlyList<Book>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var books = await _context.Books
            .AsNoTracking()
            .OrderBy(book => book.Id)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Loaded all books from database. Count: {BooksCount}",
            books.Count);

        return books;
    }

    public async Task<Book?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Loading book from database. BookId: {BookId}",
            id);

        return await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(
                book => book.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Book book,
        CancellationToken cancellationToken)
    {
        
        await _context.Books.AddAsync(book, cancellationToken);

        _logger.LogInformation("Book saved to dbContext. Title: {Title}",book.Title);

         
    }
}