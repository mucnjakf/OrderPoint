using Aspire.Hosting.Azure;
using Aspire.Hosting.DevTunnels;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// The hour the bar's day starts (local time), so a night past midnight is one day; the API and the staff apps use it
const string businessDayStartHour = "6";

// Fixed, because image URLs are stored with it
const int blobPort = 59287;

// Every stored image URL starts with it; the apps serve images from it, so phones see them through the tunnel
string imagesStorageUrl = $"http://127.0.0.1:{blobPort}/devstoreaccount1/images";

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
        .WithBlobPort(blobPort));

IResourceBuilder<AzureBlobStorageContainerResource> images = storage
    .AddBlobContainer("images");

IResourceBuilder<RedisResource> cache = builder
    .AddRedis("cache");

IResourceBuilder<ProjectResource> api = builder
    .AddProject<Projects.OrderPoint_Api>("order-point-api")
    .WithHttpHealthCheck("/health")
    .WithEnvironment("BusinessDay__StartHour", businessDayStartHour)
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
    .WithEnvironment("Images__StorageUrl", imagesStorageUrl)
    .WithReference(api)
    .WaitFor(api);

IResourceBuilder<ProjectResource> bar = builder
    .AddProject<Projects.OrderPoint_Bar>("order-point-bar")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithEnvironment("BusinessDay__StartHour", businessDayStartHour)
    .WithEnvironment("Images__StorageUrl", imagesStorageUrl)
    .WithReference(api)
    .WaitFor(api);

IResourceBuilder<ProjectResource> bartender = builder
    .AddProject<Projects.OrderPoint_Bartender>("order-point-bartender")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithEnvironment("BusinessDay__StartHour", businessDayStartHour)
    .WithEnvironment("Images__StorageUrl", imagesStorageUrl)
    .WithReference(api)
    .WaitFor(api);

IResourceBuilder<ProjectResource> customer = builder
    .AddProject<Projects.OrderPoint_Customer>("order-point-customer")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithEnvironment("Images__StorageUrl", imagesStorageUrl)
    .WithReference(api)
    .WaitFor(api);

IResourceBuilder<ProjectResource> register = builder
    .AddProject<Projects.OrderPoint_Register>("order-point-register")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithEnvironment("BusinessDay__StartHour", businessDayStartHour)
    .WithEnvironment("Images__StorageUrl", imagesStorageUrl)
    .WithReference(api)
    .WaitFor(api);

// Public addresses for every app (orderpoint-<port>.euw.devtunnels.ms); started from the dashboard only when needed
builder
    .AddDevTunnel("tunnel", "orderpoint", new DevTunnelOptions { Region = DevTunnelRegion.WestEurope })
    .WithAnonymousAccess()
    .WithReference(admin.GetEndpoint("https"))
    .WithReference(bar.GetEndpoint("https"))
    .WithReference(bartender.GetEndpoint("https"))
    .WithReference(customer.GetEndpoint("https"))
    .WithReference(register.GetEndpoint("https"))
    .WithExplicitStart();

// The Admin's table QR codes point to the customer app
admin.WithReference(customer);

// The API checks the apps' health for the Admin, but does not wait for them
api
    .WithReference(bar)
    .WithReference(bartender)
    .WithReference(customer)
    .WithReference(register);

builder.Build().Run();