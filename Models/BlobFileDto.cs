namespace BookStore.Api.Models;

public sealed record BlobFileDto(
    string Name,
    long? Size,
    string? ContentType,
    DateTimeOffset? LastModified);