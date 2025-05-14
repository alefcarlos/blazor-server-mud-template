var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyAppWithMud_Web>("web");

builder.Build().Run();
