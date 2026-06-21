using BookStore.Api.Events;

namespace BookStore.Api.EventHandlers;

public sealed class BookCreatedAuditHandler
{
    private readonly ILogger<BookCreatedAuditHandler> _logger;

    public BookCreatedAuditHandler(ILogger<BookCreatedAuditHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(BookCreatedEvent domainEvent)
    {
        _logger.LogInformation(
            "AUDIT HANDLER: Book {BookId} created: {Title}",
            domainEvent.BookId,
            domainEvent.Title);

        return Task.CompletedTask;
    }
}