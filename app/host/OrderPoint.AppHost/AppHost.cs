using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// The hour the bar's day starts (local time), so a night past midnight is one day; the API and the bartender app use it
const string BusinessDayStartHour = "6";

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("tcp", endpoint => endpoint.Port = 59286);

IResourceBuilder<PostgresDatabaseResource> database = postgres
    .AddDatabase("order-point-db");

IResourceBuilder<AzureStorageResource> storage = builder
    .AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator
        .WithDataVolume()
        .WithLifetime(ContainerLifetime.Persistent)
        .WithBlobPort(59287));

IResourceBuilder<AzureBlobStorageContainerResource> images = storage
    .AddBlobContainer("images");

IResourceBuilder<RedisResource> cache = builder
    .AddRedis("cache");

IResourceBuilder<ProjectResource> api = builder
    .AddProject<Projects.OrderPoint_Api>("order-point-api")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("BusinessDay__StartHour", BusinessDayStartHour)
    .WithReference(database)
    .WithReference(images)
    .WithReference(cache)
    .WaitFor(database)
    .WaitFor(images)
    .WaitFor(cache);

// Browser tools for data; the API checks their health, but does not wait for them
postgres.WithPgWeb(pgWeb => api.WithReference(pgWeb.Resource.PrimaryEndpoint));

storage
    .AddBlobs("storage-blobs")
    .WithAzureStorageExplorer(explorer => api.WithReference(explorer.Resource.PrimaryEndpoint));

cache.WithRedisInsight(redisInsight => api.WithReference(redisInsight.Resource.PrimaryEndpoint));

IResourceBuilder<ProjectResource> admin = builder
    .AddProject<Projects.OrderPoint_Admin>("order-point-admin")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

IResourceBuilder<ProjectResource> bartender = builder
    .AddProject<Projects.OrderPoint_Bartender>("order-point-bartender")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithEnvironment("BusinessDay__StartHour", BusinessDayStartHour)
    .WithReference(api)
    .WaitFor(api);

IResourceBuilder<ProjectResource> customer = builder
    .AddProject<Projects.OrderPoint_Customer>("order-point-customer")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

// The Admin's table QR codes point to the customer app
admin.WithReference(customer);

// The API checks the apps' health for the Admin, but does not wait for them
api
    .WithReference(bartender)
    .WithReference(customer);

builder.Build().Run();