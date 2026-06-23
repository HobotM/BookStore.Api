using BookStore.Api.Models;

namespace BookStore.Api.Repositories;

public interface IBookRepository
{
    Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken);

    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task AddAsync(Book book, CancellationToken cancellationToken);
}