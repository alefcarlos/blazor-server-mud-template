var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyAppWithMud_Web>("web")
    .WithHttpHealthCheck("/health")
    ;

builder.Build().Run();
