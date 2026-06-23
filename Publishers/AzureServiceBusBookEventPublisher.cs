using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using BookStore.Api.Events;
using BookStore.Api.Publishers;


public sealed class AzureServiceBusBookEventPublisher : IBookEventPublisher
{
    private readonly ServiceBusSender _sender;

    private readonly ILogger<AzureServiceBusBookEventPublisher> _logger;

    public AzureServiceBusBookEventPublisher(ILogger<AzureServiceBusBookEventPublisher> logger, IConfiguration configuration)
    {
        _logger = logger;

        var fullyQualifiedNamespace = configuration["ServiceBus:FullyQualifiedNamespace"];

        var queueName = configuration["ServiceBus:BookCreatedQueueName"];

        if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
        {
            throw new InvalidOperationException(
                "ServiceBus:FullyQualifiedNamespace is missing.");
        }

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new InvalidOperationException(
                "ServiceBus:BookCreatedQueueName is missing.");
        }

        var client = new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential());

        _sender = client.CreateSender(queueName);

    }


    public async Task PublishBookCreatedAsync(BookCreatedEvent bookCreatedEvent, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(bookCreatedEvent);


        var message = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = nameof(BookCreatedEvent)
        };

        await _sender.SendMessageAsync(message, cancellationToken);
        _logger.LogInformation("BookCreatedEvent published to Service Bus. Book id: {BookId}", bookCreatedEvent.BookId);









    }

}