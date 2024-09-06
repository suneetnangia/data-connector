namespace Http.Mqtt.Connector.Svc;

using System.Text;
using System.Text.Json;
using Akri.Mqtt.Connection;
using Akri.Mqtt.Models;
using Akri.Mqtt.MqttNetAdapter.Session;
using MQTTnet.Exceptions;

public class MqttDataSink : IDataSink
{
    private readonly ILogger _logger;
    private readonly MqttSessionClient _mqttSessionClient;

    private readonly string _host;

    private readonly int _port;

    private readonly string? _clientId;

    private readonly bool _useTls;

    private readonly string _baseTopic;

    private int _initialBackoffDelayInMilliseconds;

    private int _maxBackoffDelayInMilliseconds;

    public MqttDataSink(ILogger logger, MqttSessionClient mqttSessionClient, string host, int port, string? clientId, bool useTls, string baseTopic, int initialBackoffDelayInMilliseconds = 500, int maxBackoffDelayInMilliseconds = 10_000)
    {
        _logger = logger;
        _mqttSessionClient = mqttSessionClient ?? throw new ArgumentNullException(nameof(mqttSessionClient));
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _port = port;
        _clientId = clientId ?? Guid.NewGuid().ToString();
        _useTls = useTls;
        _baseTopic = baseTopic ?? throw new ArgumentNullException(nameof(baseTopic));
        _initialBackoffDelayInMilliseconds = initialBackoffDelayInMilliseconds;
        _maxBackoffDelayInMilliseconds = maxBackoffDelayInMilliseconds;

        // Set the unique identifier for the data sink for observability.
        Id = _clientId + _host + port + _baseTopic;

        // Connect to the MQTT broker.
        Connect();
    }

    public string Id { get; init; }

    public async Task PushDataAsync(JsonDocument data, CancellationToken stoppingToken)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        // TODO: create dynamic topic structure from configuration template.
        var mqtt_application_message = new MqttApplicationMessage(_baseTopic, MqttQualityOfServiceLevel.AtLeastOnce)
        {
            PayloadSegment = new ArraySegment<byte>(Encoding.Unicode.GetBytes(data.RootElement.GetRawText()))
        };

        // Publish data to the MQTT broker until successful.
        var successfulPublish = false;
        int backoff_delay_in_milliseconds = _initialBackoffDelayInMilliseconds;
        while (!successfulPublish)
        {
            try
            {
                await _mqttSessionClient.PublishAsync(mqtt_application_message, stoppingToken);

                // Reset backoff delay on successful data processing.
                backoff_delay_in_milliseconds = _initialBackoffDelayInMilliseconds;
                successfulPublish = true;
            }
            catch (MqttCommunicationException ex)
            {
                _logger.LogError(ex, "Error publishing data to MQTT broker, topic: {topic}, reconnecting...", _baseTopic);

                await Task.Delay(backoff_delay_in_milliseconds);
                backoff_delay_in_milliseconds = (int)Math.Pow(backoff_delay_in_milliseconds, 1.02);

                // Limit backoff delay to _maxBackoffDelayInMilliseconds.
                backoff_delay_in_milliseconds = backoff_delay_in_milliseconds > _maxBackoffDelayInMilliseconds ? _maxBackoffDelayInMilliseconds : backoff_delay_in_milliseconds;

                await _mqttSessionClient.ReconnectAsync();
            }
        }
    }

    public void Connect()
    {
        // Connect should only happen via single thread at a time in multithreaded calls to PushDataAsync.
        lock (_mqttSessionClient)
        {
            if (!_mqttSessionClient.IsConnected)
            {
                MqttConnectionSettings connectionSettings = new(_host)
                {
                    TcpPort = _port,
                    ClientId = _clientId,
                    UseTls = _useTls,
                };

                var result_code = _mqttSessionClient.ConnectAsync(connectionSettings).GetAwaiter().GetResult().ResultCode;

                if (result_code != MqttClientConnectResultCode.Success)
                {
                    throw new ApplicationException($"Failed to connect to the MQTT broker, code {result_code}");
                }
            }
        }
    }
}
