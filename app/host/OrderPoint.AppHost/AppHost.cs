using Aspire.Hosting.Azure;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

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

IResourceBuilder<ProjectResource> api = builder
    .AddProject<Projects.OrderPoint_Api>("order-point-api")
    .WithUrlForEndpoint("https", _ => new ResourceUrlAnnotation { Url = "/scalar", DisplayText = "API reference" })
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WithReference(images)
    .WaitFor(database)
    .WaitFor(images);

// Browser tools for data; the API checks their health, but does not wait for them
postgres.WithPgWeb(pgWeb => api.WithReference(pgWeb.Resource.PrimaryEndpoint));

storage
    .AddBlobs("storage-blobs")
    .WithAzureStorageExplorer(explorer => api.WithReference(explorer.Resource.PrimaryEndpoint));

IResourceBuilder<ProjectResource> admin = builder
    .AddProject<Projects.OrderPoint_Admin>("order-point-admin")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();