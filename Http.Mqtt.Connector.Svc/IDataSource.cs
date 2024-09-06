namespace Http.Mqtt.Connector.Svc;

using System.Text.Json;

public interface IDataSource
{
    string Id { get; init; }

    int PollingInternalInMilliseconds { get; init; }

    Task<JsonDocument> PullDataAsync(CancellationToken stoppingToken);
}
