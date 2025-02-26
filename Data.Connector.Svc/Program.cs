using Data.Connector.Svc;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
.AddHostedService<Worker>()
.AddConfig(builder.Configuration)
.AddDependencies();

// Add option to read settings from a different folder than root, for container mounts.
// It will be merged with the appsettings.json in the root folder of the app.
builder.Configuration.AddJsonFile("settings/appsettings.json", optional: true);

var host = builder.Build();
host.Run();
