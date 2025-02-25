namespace Http.Mqtt.Connector.Svc;

using System.Text;
using System.Text.Json;
using Azure.Iot.Operations.Mqtt.Session;
using Azure.Iot.Operations.Protocol.Connection;
using Azure.Iot.Operations.Protocol.Models;
using Azure.Iot.Operations.Services.StateStore;
using MQTTnet.Exceptions;

public class MqttStateStoreSink : IDataSink
{
    private readonly ILogger _logger;
    private readonly MqttSessionClient _mqttSessionClient;
    private readonly IStateStoreClient _stateStoreClient;

    private readonly string _host;

    private readonly int _port;

    private readonly string? _clientId;

    private readonly bool _useTls;

    private readonly string _username;

    private readonly string _passwordFilePath;

    private readonly string _satFilePath;

    private readonly string _caFilePath;

    private readonly string _key;

    private int _initialBackoffDelayInMilliseconds;

    private int _maxBackoffDelayInMilliseconds;

    public MqttStateStoreSink(
        ILogger logger,
        MqttSessionClient mqttSessionClient,
        IStateStoreClient stateStoreClient,
        string host,
        int port,
        string? clientId,
        bool useTls,
        string username,
        string passwordFilePath,
        string satFilePath,
        string caFilePath,
        string key,
        int initialBackoffDelayInMilliseconds = 500,
        int maxBackoffDelayInMilliseconds = 10_000)
    {
        _logger = logger;
        _mqttSessionClient = mqttSessionClient ?? throw new ArgumentNullException(nameof(mqttSessionClient));
        _stateStoreClient = stateStoreClient ?? throw new ArgumentNullException(nameof(stateStoreClient));
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _port = port;
        _clientId = clientId ?? Guid.NewGuid().ToString();
        _useTls = useTls;
        _username = username;
        _passwordFilePath = passwordFilePath;
        _satFilePath = satFilePath;
        _caFilePath = caFilePath;
        _key = key ?? throw new ArgumentNullException(nameof(key));
        _initialBackoffDelayInMilliseconds = initialBackoffDelayInMilliseconds;
        _maxBackoffDelayInMilliseconds = maxBackoffDelayInMilliseconds;

        // Set the unique identifier for the data sink for observability.
        Id = $"{_clientId}-{_host}-{port}-{_key}";
    }

    public string Id { get; init; }

    public async Task PushDataAsync(JsonDocument data, CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(data);
        _logger.LogTrace("Pushing data to StateStore key: '{key}'", _key);

        // NOTE - this is intentional
        // Format the JSON document to a multi-root JSON format with new lines - as currently required to support Data Flows reference datasets
        // See format referenced https://github.com/Azure-Samples/explore-iot-operations/tree/main/samples/dss_set
        var formattedContent = new StringBuilder();
        foreach (var element in data.RootElement.EnumerateArray())
        {
            formattedContent.AppendLine(element.ToString());
        }

        var content = formattedContent.ToString();

        // Set the key to the data passed in method argument, retry until successful or circuit breaker is triggered.
        var successfulSet = false;
        int backoff_delay_in_milliseconds = _initialBackoffDelayInMilliseconds;
        while (!successfulSet)
        {
            try
            {
                var response = await _stateStoreClient.SetAsync(_key, content, cancellationToken: stoppingToken);

                if (response.Success)
                {
                    _logger.LogTrace("StateStore key {key} set.", _key);

                    // Reset backoff delay on successful save to DSS.
                    backoff_delay_in_milliseconds = _initialBackoffDelayInMilliseconds;
                    successfulSet = true;
                }
                else
                {
                    throw new Exception($"Failed to set StateStore key: '{_key}', response: {response}. Retrying...");
                }
            }
            catch (Exception ex) when (ex is MqttCommunicationException || ex is Exception)
            {
                if (ex is MqttCommunicationException)
                {
                    _logger.LogError(ex, "Error communicating to MQTT broker for setting StateStore key: '{key}', reconnecting...", _key);
                    await _mqttSessionClient.ReconnectAsync();
                }
                else
                {
                    _logger.LogError(ex, "Error setting StateStore key: '{key}' with error '{error}', retrying...", _key, ex);
                }

                await Task.Delay(backoff_delay_in_milliseconds, stoppingToken);
                backoff_delay_in_milliseconds = (int)Math.Pow(backoff_delay_in_milliseconds, 1.02);

                // Limit backoff delay to _maxBackoffDelayInMilliseconds.
                backoff_delay_in_milliseconds = backoff_delay_in_milliseconds > _maxBackoffDelayInMilliseconds ? _maxBackoffDelayInMilliseconds : backoff_delay_in_milliseconds;
            }
        }
    }

    public void Connect()
    {
        if (!_mqttSessionClient.IsConnected)
        {
            _logger.LogInformation("MQTT SAT token file location: '{file}'.", _satFilePath);
            _logger.LogInformation("CA cert file location: '{file}'.", _caFilePath);
            _logger.LogInformation("Password file location: '{file}'.", _passwordFilePath);

            MqttConnectionSettings connectionSettings = new(_host)
            {
                TcpPort = _port,
                ClientId = _clientId,
                UseTls = _useTls,
                Username = _username,
                PasswordFile = _passwordFilePath,
                SatAuthFile = _satFilePath,
                CaFile = _caFilePath
            };

            var result_code = _mqttSessionClient.ConnectAsync(connectionSettings)
                                .ConfigureAwait(false)
                                .GetAwaiter()
                                .GetResult();

            if (result_code.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new ApplicationException($"Failed to connect to the MQTT broker, code {result_code}");
            }
        }
    }
}
