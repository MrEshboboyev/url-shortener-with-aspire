namespace UrlShortening.Api.Models;

public record ShortenedUrl(
    string ShortCode,
    string OriginalUrl,
    DateTime CreatedAt);