namespace Http.Mqtt.Connector.Svc;

using System.Text.Json;

public class HttpDataSource : IDataSource
{
    private readonly ILogger _logger;
    private readonly HttpClient _http_client;
    private readonly Uri _url;

    public HttpDataSource(ILogger logger, HttpClient httpClient, Uri baseUrl, Uri relativeUrl, int pollingInternalInMilliseconds)
    {
        _logger = logger;
        _http_client = httpClient;

        _url = new Uri(baseUrl, relativeUrl);

        PollingInternalInMilliseconds = pollingInternalInMilliseconds;

        // Set the unique identifier for the data source for observability.
        Id = _url.ToString();
    }

    public string Id { get; init; }

    public int PollingInternalInMilliseconds { get; init; }

    public async Task<JsonDocument> PullDataAsync(CancellationToken stoppingToken)
    {
        _logger.LogTrace("Connecting to Http end point: {url}", _url.ToString());

        // Circuit breaker with reties and back-off is configured at the HttpClient level in DI config.
        var response = await _http_client.GetAsync(_url, stoppingToken);
        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();
        var jsonDocument = await JsonDocument.ParseAsync(responseStream);

        return jsonDocument;
    }
}
