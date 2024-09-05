using Http.Mqtt.Connector.Svc;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
.AddHostedService<Worker>()
.AddConfig(builder.Configuration)
.AddDependencies();

var host = builder.Build();
host.Run();
