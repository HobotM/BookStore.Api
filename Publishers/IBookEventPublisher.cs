using BookStore.Api.Events;

namespace BookStore.Api.Publishers;

public interface IBookEventPublisher
{
    Task PublishBookCreatedAsync(BookCreatedEvent bookCreatedEvent, CancellationToken cancellationToken);
}