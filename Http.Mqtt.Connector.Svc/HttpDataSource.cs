namespace Http.Mqtt.Connector.Svc;

using System.Text.Json;

public class HttpDataSource : IDataSource
{
    private readonly ILogger _logger;
    private readonly HttpClient _http_client;
    private readonly Uri _relative_url;

    public HttpDataSource(ILogger logger, HttpClient httpClient, Uri relativeUrl, int pollingInternalInMilliseconds)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _http_client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _relative_url = relativeUrl ?? throw new ArgumentNullException(nameof(relativeUrl));
        PollingInternalInMilliseconds = pollingInternalInMilliseconds;

        // Set the unique identifier for the data source for observability.
        Id = new Uri(_http_client.BaseAddress ?? throw new InvalidOperationException("BaseAddress cannot be null"), relativeUrl).ToString();
        _logger.LogTrace("Configured http endpoint, Id: {Id}", Id);
    }

    public string Id { get; init; }

    public int PollingInternalInMilliseconds { get; init; }

    public async Task<JsonDocument> PullDataAsync(CancellationToken stoppingToken)
    {
        _logger.LogTrace("Connecting to Http endpoint, Id: {Id}", Id);

        // Circuit breaker with reties and back-off is configured at the HttpClient level in DI config.
        var response = await _http_client.GetAsync(_relative_url.Normalize(), stoppingToken);
        response.EnsureSuccessStatusCode();

        using var responseStream = await response.Content.ReadAsStreamAsync();
        var jsonDocument = await JsonDocument.ParseAsync(responseStream);

        return jsonDocument;
    }
}
