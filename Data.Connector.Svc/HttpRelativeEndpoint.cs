namespace Data.Connector.Svc;

public class HttpRelativeEndpoint
{
    public required string Url { get; set; }

    public int PollingInternalInMilliseconds { get; set; } = 1000;
}
