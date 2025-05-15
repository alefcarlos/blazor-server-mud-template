var builder = DistributedApplication.CreateBuilder(args);

var adminUsername = builder.AddParameter("username", "admin");
var adminPassword = builder.AddParameter("password", "admin");

var keycloak = builder.AddKeycloak("keycloak", 8080, adminUsername, adminPassword)
    .WithRealmImport("./realms")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    ;

builder.AddProject<Projects.MyAppWithMud_Web>("web")
    .WithHttpHealthCheck("/health")
    .WaitFor(keycloak)
    ;

await builder.Build().RunAsync();
