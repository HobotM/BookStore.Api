using BookStore.Api.Events;

namespace BookStore.Api.EventHandlers;

public sealed class BookCreatedEmailHandler
{
    private readonly ILogger<BookCreatedEmailHandler> _logger;

    public BookCreatedEmailHandler(ILogger<BookCreatedEmailHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(BookCreatedEvent domainEvent)
    {
         _logger.LogInformation(
            "EMAIL HANDLER: New book created. Sending email about book {BookId}: {Title}",
            domainEvent.BookId,
            domainEvent.Title);

        return Task.CompletedTask;
    }
}