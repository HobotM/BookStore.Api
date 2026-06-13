using BookStore.Api.Models;

namespace BookStore.Api.Repositories;

public interface IBookRepository
{
    IReadOnlyCollection<Book> GetAll();
    Book? GetById(int id);
    Book Add(Book book);
    int GetNextId();
    
}