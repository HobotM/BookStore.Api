using BookStore.Api.Models;

namespace BookStore.Api.Repositories;

public interface IBookRepository
{
    IReadOnlyList<Book> GetAll();
    Book? GetById(int id);
    Task<Book> AddAsync(Book book);

    
}