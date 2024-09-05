namespace Http.Mqtt.Connector.Svc;

public class HttpEndpoint
{
    public required string Url { get; set; }

    public required int Port { get; set; } = 80;

    public double TimeOutInSeconds { get; set; } = 10;

    public required HttpRelativeEndpoint[] RelativeEndpoints { get; set; }
}
