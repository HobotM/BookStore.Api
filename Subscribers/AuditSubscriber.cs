using BookStore.Api.Models;
using BookStore.Api.Services;


namespace BookStore.Api.Events;

public sealed class AuditSubscriber
{
    private readonly ILogger<AuditSubscriber> _logger;

    public AuditSubscriber(ILogger<AuditSubscriber> logger)
    {
        _logger = logger;
    }
    
    public void onBookCreated(Book book)
    {
        _logger.LogInformation("AUDIT: Book created: Id: {BookId}, Title: {BookTitle}", book.Id, book.Title);
            
    }





}






