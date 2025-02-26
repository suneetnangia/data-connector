namespace Data.Connector.Svc;

using System.Text.Json;

public interface IDataSink
{
    string Id { get; init; }

    Task PushDataAsync(JsonDocument data, CancellationToken stoppingToken);
}
