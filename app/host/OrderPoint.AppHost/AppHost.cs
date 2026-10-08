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
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WithReference(images)
    .WaitFor(database)
    .WaitFor(images);

IResourceBuilder<ProjectResource> admin = builder
    .AddProject<Projects.OrderPoint_Admin>("order-point-admin")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();