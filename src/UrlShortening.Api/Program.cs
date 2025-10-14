using UrlShortening.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDataSource("url-shortener");

builder.AddRedisDistributedCache("redis");

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter("UrlShortening.Api"));

#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddOpenApi();

builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddScoped<UrlShorteningService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI v1");
    });
}

app.MapPost("shorten", async (
    string url,
    UrlShorteningService urlShorteningService) =>
{
    if (!Uri.TryCreate(url, UriKind.Absolute, out _))
    {
        return Results.BadRequest("Invalid URL format");
    }

    var shortCode = await urlShorteningService.ShortenUrl(url);

    return Results.Ok(new { shortCode });
});

app.MapGet("{shortCode}", async (
    string shortCode,
    UrlShorteningService urlShorteningService) =>
{
    var originalUrl = await urlShorteningService.GetOriginalUrl(shortCode);

    return originalUrl is null ? Results.NotFound() : Results.Redirect(originalUrl);
});

app.MapGet("urls", async (UrlShorteningService urlShorteningService) =>
{
    return Results.Ok(await urlShorteningService.GetAllUrls());
});

app.UseHttpsRedirection();

app.Run();