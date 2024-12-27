using System.Diagnostics;
using System.Diagnostics.Metrics;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;
using Npgsql;
using UrlShortening.Api.Models;

namespace UrlShortening.Api.Services;

internal sealed class UrlShorteningService(
    NpgsqlDataSource dataSource,
    HybridCache hybridCache,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UrlShorteningService> logger)
{
    private const int MaxRetries = 3;
    
    private static readonly Meter Meter = new("UrlShortening.Api");
    private static readonly Counter<int> RedirectsCounter = Meter.CreateCounter<int>(
        "url_shortener.redirects",
        "The number of successful redirects");
    private static readonly Counter<int> FailedRedirectsCounter = Meter.CreateCounter<int>(
        "url_shortener.failed_redirects",
        "The number of failed redirects");

    
    public async Task<string> ShortenUrl(string originalUrl)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var shortCode = GenerateShortCode();

                const string sql = 
                    $"""
                     INSERT INTO shortened_urls (short_code, original_url)
                     VALUES (@ShortCode, @OriginalUrl)
                     RETURNING short_code;
                     """;

                await using var connection = await dataSource.OpenConnectionAsync();

                var result = await connection.QuerySingleAsync<string>(
                    sql,
                    new { ShortCode = shortCode, OriginalUrl = originalUrl });

                await hybridCache.SetAsync(shortCode, originalUrl);
                
                return result;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                if (attempt == MaxRetries)
                {
                    logger.LogError(
                        ex,
                        "Failed to generate unique short code after {MaxRetries} attempts",
                        MaxRetries);

                    throw new InvalidOperationException("Failed to generate unique short code", ex);
                }

                logger.LogWarning(
                    "Short code collision occurred. Retrying... (Attempt {Attempt} of {MaxRetries})",
                    attempt + 1,
                    MaxRetries);
            }
        }

        throw new InvalidOperationException("Failed to generate unique short code");
    }
    
    private static string GenerateShortCode()
    {
        const int length = 7;
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var chars = Enumerable.Range(0, length)
            .Select(_ => alphabet[Random.Shared.Next(alphabet.Length)])
            .ToArray();

        return new string(chars);
    }
    
    public async Task<string?> GetOriginalUrl(string shortCode)
    {
        var originalUrl = await hybridCache.GetOrCreateAsync(shortCode, async token =>
        {
            const string sql = 
                $"""
                 SELECT original_url
                 FROM shortened_urls
                 WHERE short_code = @ShortCode;
                 """;

            await using var connection = await dataSource.OpenConnectionAsync(token);

            var originalUrl = await connection.QuerySingleOrDefaultAsync<string>(
                sql,
                new { ShortCode = shortCode });

            return originalUrl;
        });
        
        if (originalUrl is null)
        {
            FailedRedirectsCounter.Add(1,
                new TagList 
                {
                    {
                        "short_code", shortCode
                    }
                });
        }
        else
        {
            await RecordVisit(shortCode);
            RedirectsCounter.Add(1,
                new TagList
                {
                    {
                        "short_code", shortCode
                    }
                });
        }

        
        return originalUrl;
    }

    private async Task RecordVisit(string shortCode)
    {
        var context = httpContextAccessor.HttpContext;
        var userAgent = context?.Request.Headers.UserAgent.ToString();
        var referer = context?.Request.Headers.Referer.ToString();

        const string sql = 
            $"""
            INSERT INTO url_visits (short_code, user_agent, referer)
            VALUES (@ShortCode, @UserAgent, @Referer);
            """;

        await using var connection = await dataSource.OpenConnectionAsync();

        await connection.ExecuteAsync(
            sql,
            new
            {
                ShortCode = shortCode,
                UserAgent = userAgent,
                Referer = referer
            });
    }
    
    public async Task<IEnumerable<ShortenedUrl>> GetAllUrls()
    {
        const string sql = 
            $"""
            SELECT short_code as ShortCode, original_url as OriginalUrl, created_at as CreatedAt
            FROM shortened_urls
            ORDER BY created_at DESC;
            """;

        await using var connection = await dataSource.OpenConnectionAsync();

        return await connection.QueryAsync<ShortenedUrl>(sql);

    }
}