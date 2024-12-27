using Dapper;
using Npgsql;

namespace UrlShortening.Api.Services;

public class DatabaseInitializer(
    NpgsqlDataSource dataSource,
    IConfiguration configuration,
    ILogger<DatabaseInitializer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await CreateDatabaseIfNotExist(stoppingToken);
            await InitializeSchema(stoppingToken);

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing database");
            throw;
        }
    }

    private async Task CreateDatabaseIfNotExist(CancellationToken stoppingToken)
    {
        var connectionString = configuration.GetConnectionString("url-shortener");
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        builder.Database = "postgres"; // Connect to the default 'postgres' database

        await using var connection = new NpgsqlConnection(builder.ToString());
        await connection.OpenAsync(stoppingToken);

        bool databaseExists = await connection.ExecuteScalarAsync<bool>(
            $"""SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @databaseName)""",
            new { databaseName });

        if (!databaseExists)
        {
            logger.LogInformation("Creating database {DatabaseName}", databaseName);
            await connection.ExecuteAsync($"CREATE DATABASE \"{databaseName}\"");
        }
    }
    
    private async Task InitializeSchema(CancellationToken stoppingToken)
    {
        const string createTablesSql = 
            $"""
             CREATE TABLE IF NOT EXISTS shortened_urls (
                 id SERIAL PRIMARY KEY,
                 short_code VARCHAR(10) UNIQUE NOT NULL,
                 original_url TEXT NOT NULL,
                 created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
             );
             CREATE INDEX IF NOT EXISTS idx_short_code ON shortened_urls(short_code);

             CREATE TABLE IF NOT EXISTS url_visits (
                 id SERIAL PRIMARY KEY,
                 short_code VARCHAR(10) NOT NULL,
                 visited_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
                 user_agent TEXT,
                 referer TEXT,
                 FOREIGN KEY (short_code) REFERENCES shortened_urls(short_code)
             );
             CREATE INDEX IF NOT EXISTS idx_visits_short_code ON url_visits(short_code);
             """;

        await using var command = dataSource.CreateCommand(createTablesSql);
        await command.ExecuteNonQueryAsync(stoppingToken);
    }
}