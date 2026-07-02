using Aspire.Hosting.JavaScript;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpoint("tcp", endpoint => endpoint.Port = 59286);

IResourceBuilder<PostgresDatabaseResource> database = postgres
    .AddDatabase("order-point-db");

IResourceBuilder<ProjectResource> api = builder
    .AddProject<Projects.OrderPoint_Api>("order-point-api")
    .WithUrlForEndpoint("http", url => url.Url += "/scalar")
    .WithUrlForEndpoint("https", url => url.Url += "/scalar")
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WaitFor(database);

IResourceBuilder<ViteAppResource> adminWeb = builder
    .AddViteApp("admin-web", "../../../client/admin-web")
    .WithEndpoint("http", endpoint =>
    {
        endpoint.Port = 5173;
        endpoint.TargetPort = 5173;
        endpoint.IsProxied = false;
    })
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();