using Http.Mqtt.Connector.Svc;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
.AddHostedService<Worker>()
.AddConfig(builder.Configuration)
.AddDependencies();

// Add option to read settings from a different folder than root, for container mounts.
builder.Configuration.AddJsonFile("settings/appsettings.json", optional: true);

var host = builder.Build();
host.Run();
