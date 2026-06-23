namespace BookStore.Api.Models;


public class BookAudit
{
    public int Id {get;set;}
    public int BookId {get;set;}
    public string EventType {get; set;} =string.Empty;
    public DateTime CreatedAtUtc {get;set;}
    public string MessageId { get; set; } = string.Empty;
}