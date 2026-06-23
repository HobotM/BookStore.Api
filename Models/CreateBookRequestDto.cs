
namespace BookStore.Api.Models;

public sealed record CreateBookRequestDto(
    string Title,
    string Author,
    decimal Price);