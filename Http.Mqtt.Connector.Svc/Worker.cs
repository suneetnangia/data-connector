namespace Http.Mqtt.Connector.Svc;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IDataSource[] _dataSources;
    private readonly IDataSink _dataSink;

    private int _initialBackoffDelayInMilliseconds;
    private int _maxBackoffDelayInMilliseconds;

    public Worker(ILogger<Worker> logger, IDataSource[] dataSources, IDataSink dataSink, int initialBackoffDelayInMilliseconds = 500, int maxBackoffDelayInMilliseconds = 10_000)
    {
        _logger = logger;
        _dataSources = dataSources ?? throw new ArgumentNullException(nameof(dataSources));
        _dataSink = dataSink ?? throw new ArgumentNullException(nameof(dataSink));
        _initialBackoffDelayInMilliseconds = initialBackoffDelayInMilliseconds;
        _maxBackoffDelayInMilliseconds = maxBackoffDelayInMilliseconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Initiating connector: {time}", DateTimeOffset.Now);

        // Run loop for each data source in parallel.
        await Parallel.ForEachAsync(_dataSources, async (dataSource, stoppingToken) =>
        {
            // Exit data sourcing and publishing loop if cancellation is requested.
            int backoff_delay_in_milliseconds = _initialBackoffDelayInMilliseconds;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var source_data = await dataSource.PullDataAsync();
                    _logger.LogTrace("Source Data Received: {data}", source_data.RootElement.ToString());

                    await _dataSink.PushDataAsync(source_data);

                    _logger.LogTrace("Data source {id}, published data to MQTT, data content: {time}", dataSource.Id, source_data.RootElement.ToString());
                    _logger.LogTrace("Data source {id}, waiting for next polling cycle (UTC): {time}, current time {time}", dataSource.Id, DateTimeOffset.UtcNow.AddMilliseconds(dataSource.PollingInternalInMilliseconds), DateTimeOffset.UtcNow);

                    // Delay for the configured polling interval.
                    await Task.Delay(dataSource.PollingInternalInMilliseconds, stoppingToken);

                    // Reset backoff delay on successful data processing.
                    backoff_delay_in_milliseconds = _initialBackoffDelayInMilliseconds;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing messages; data source {id}, data sink {id}, retrying in {milliseconds} milliseconds...", dataSource.Id, _dataSink.Id, backoff_delay_in_milliseconds);
                    await Task.Delay(backoff_delay_in_milliseconds, stoppingToken);
                    backoff_delay_in_milliseconds = (int)Math.Pow(backoff_delay_in_milliseconds, 1.02);

                    // Limit backoff delay to _maxBackoffDelayInMilliseconds.
                    backoff_delay_in_milliseconds = backoff_delay_in_milliseconds > _maxBackoffDelayInMilliseconds ? _maxBackoffDelayInMilliseconds : backoff_delay_in_milliseconds;
                }
            }
        });
    }
}
