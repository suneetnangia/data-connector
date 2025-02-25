namespace Http.Mqtt.Connector.Svc;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Dictionary<IDataSource, IDataSink> _dataSourceSinkMap;
    private int _initialBackoffDelayInMilliseconds;
    private int _maxBackoffDelayInMilliseconds;

    public Worker(ILogger<Worker> logger, Dictionary<IDataSource, IDataSink> dataSourceSinkMap, int initialBackoffDelayInMilliseconds = 500, int maxBackoffDelayInMilliseconds = 10_000)
    {
        _logger = logger;
        _dataSourceSinkMap = dataSourceSinkMap ?? throw new ArgumentNullException(nameof(dataSourceSinkMap));
        _initialBackoffDelayInMilliseconds = initialBackoffDelayInMilliseconds;
        _maxBackoffDelayInMilliseconds = maxBackoffDelayInMilliseconds;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Initiating connector: {time}", DateTimeOffset.Now);

        // Run loop for each data source in parallel.
        await Parallel.ForEachAsync(
        _dataSourceSinkMap,
        new ParallelOptions
        { MaxDegreeOfParallelism = _dataSourceSinkMap.Count },
        async (dataSourceSink, stoppingToken) =>
            {
                // Exit data sourcing and publishing loop if cancellation is requested.
                int backoff_delay_in_milliseconds = _initialBackoffDelayInMilliseconds;
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var source_data = await dataSourceSink.Key.PullDataAsync(stoppingToken);
                        _logger.LogTrace("Source Data Received: {data}", source_data.RootElement.ToString());

                        await dataSourceSink.Value.PushDataAsync(source_data, stoppingToken);

                        _logger.LogTrace("Data source '{id}', published data to sink, data content: {time}", dataSourceSink.Key.Id, source_data.RootElement.ToString());
                        _logger.LogTrace("Data source '{id}', waiting for next polling cycle (UTC): {time}, current time {time}", dataSourceSink.Key.Id, DateTimeOffset.UtcNow.AddMilliseconds(dataSourceSink.Key.PollingInternalInMilliseconds), DateTimeOffset.UtcNow);

                        // Delay for the configured polling interval.
                        await Task.Delay(dataSourceSink.Key.PollingInternalInMilliseconds, stoppingToken);

                        // Reset backoff delay on successful data processing.
                        backoff_delay_in_milliseconds = _initialBackoffDelayInMilliseconds;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while processing messages; data source '{id}', data sink '{id}', retrying in {milliseconds} milliseconds...", dataSourceSink.Key.Id, dataSourceSink.Value.Id, backoff_delay_in_milliseconds);
                        await Task.Delay(backoff_delay_in_milliseconds, stoppingToken);
                        backoff_delay_in_milliseconds = (int)Math.Pow(backoff_delay_in_milliseconds, 1.02);

                        // Limit backoff delay to _maxBackoffDelayInMilliseconds.
                        backoff_delay_in_milliseconds = backoff_delay_in_milliseconds > _maxBackoffDelayInMilliseconds ? _maxBackoffDelayInMilliseconds : backoff_delay_in_milliseconds;
                    }
                }
            });
    }
}
