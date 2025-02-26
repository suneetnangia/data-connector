namespace Data.Connector.Svc;

public class HttpEndpoint
{
    public required string Url { get; set; }

    public double TimeOutInSeconds { get; set; } = 10;

    public required HttpRelativeEndpoint[] RelativeEndpoints { get; set; }
}
