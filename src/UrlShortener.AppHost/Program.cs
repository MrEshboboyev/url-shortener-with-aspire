var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("url-shortener");

var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.UrlShortening_Api>("urlshortening-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.Build().Run();
