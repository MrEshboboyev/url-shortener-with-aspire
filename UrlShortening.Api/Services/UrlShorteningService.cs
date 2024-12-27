using UrlShortening.Api.Models;

namespace UrlShortening.Api.Services;

internal sealed class UrlShorteningService
{
    public async Task<string> ShortenUrl(string originalUrl)
    {
        throw new NotImplementedException();
    }
    
    public async Task<string?> GetOriginalUrl(string shortCode)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ShortenedUrl>> GetAllUrls()
    {
        throw new NotImplementedException();
    }
}