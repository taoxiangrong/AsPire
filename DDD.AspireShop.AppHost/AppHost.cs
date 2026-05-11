var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.DDD_AspireShop_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.DDD_AspireShop_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
