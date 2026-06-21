namespace BookStore.Api.Events;

public sealed record BookCreatedEvent(
    int BookId,
    string Title,
    string Author,
    decimal Price,
    DateTime OccurredAtUtc);