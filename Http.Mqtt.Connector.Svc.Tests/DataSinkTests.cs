namespace Http.Mqtt.Connector.Svc.Tests;

using Xunit;
using Moq;
using Http.Mqtt.Connector.Svc;
using Microsoft.Extensions.Logging;
using Azure.Iot.Operations.Mqtt.Session;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Iot.Operations.Protocol.Models;
using System.Text;
using Azure.Iot.Operations.Protocol.Connection;
using System.Security.Cryptography;

public class MqttDataSinkTests
{
    const string MQTT_HOST = "testhost";
    const int MQTT_PORT = 1883;
    const string MQTT_CLIENT_ID = "Http.Mqtt.Connector.Svc";
    const bool MQTT_USE_TLS = false;
    const string MQTT_USERNAME = "user01";
    const string MQTT_PASSWORD_FILE = "/path/to/password/file";
    const string MQTT_SAT_AUTH_FILE = "/path/to/sat/file";
    const string MQTT_CA_FILE = "/path/to/ca/file";
    const string MQTT_BASE_TOPIC = "azure-iot-operations/data/";
    const string MQTT_SOURCE_ID = "sourceid";

    private readonly Mock<ILogger<MqttDataSink>> _loggerMock;
    private readonly Mock<MqttSessionClient> _mqttSessionClientMock;
    private readonly MqttDataSink _mqttDataSink;
    private readonly string _hash;

    public MqttDataSinkTests()
    {
        _loggerMock = new Mock<ILogger<MqttDataSink>>();
        _mqttSessionClientMock = new Mock<MqttSessionClient>(null);

        // Pre-computed hash for the given MQTT parameters
        using SHA256 sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(MQTT_SOURCE_ID));
        var hash = new StringBuilder();        
        for (int i = 0; i < bytes.Length; i++)
        {
            hash.Append(bytes[i].ToString("x2"));
        }
        _hash = hash.ToString();

        _mqttDataSink = new MqttDataSink(
            _loggerMock.Object,
            _mqttSessionClientMock.Object,
            MQTT_HOST,
            MQTT_PORT,
            MQTT_CLIENT_ID,
            MQTT_USE_TLS,
            MQTT_USERNAME,
            MQTT_PASSWORD_FILE,
            MQTT_SAT_AUTH_FILE,
            MQTT_CA_FILE,
            MQTT_BASE_TOPIC,
            MQTT_SOURCE_ID,
            []);
    }
    [Fact]
    public void TestMqttDataSinkInitialization()
    {
        // Arrange
        // Act        
        // Assert        
        Assert.NotNull(_mqttDataSink);
        var g = $"{MQTT_CLIENT_ID}-{MQTT_HOST}-{MQTT_PORT}-{MQTT_BASE_TOPIC}/{_hash}/{MQTT_SOURCE_ID}";
        Assert.Equal($"{MQTT_CLIENT_ID}-{MQTT_HOST}-{MQTT_PORT}-{MQTT_BASE_TOPIC}{_hash}/{MQTT_SOURCE_ID}", _mqttDataSink.Id);
    }

    [Fact]
    public async Task TestMqttDataSinkPublishAsync()
    {
        // Arrange
        using var test_json_document = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            key1 = "value1",
            key2 = 2,
            key3 = true
        }));

        var expected_payload_segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(test_json_document.RootElement.GetRawText()));

        // Act
        await _mqttDataSink.PushDataAsync(test_json_document, CancellationToken.None);

        // Assert
        _mqttSessionClientMock.Verify(m => m.PublishAsync(It.Is<MqttApplicationMessage>(m => m.Topic == $"{MQTT_BASE_TOPIC}{_hash}/{MQTT_SOURCE_ID}"), It.IsAny<CancellationToken>()));
        _mqttSessionClientMock.Verify(m => m.PublishAsync(It.Is<MqttApplicationMessage>(m => m.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtLeastOnce), It.IsAny<CancellationToken>()));
        _mqttSessionClientMock.Verify(m => m.PublishAsync(It.Is<MqttApplicationMessage>(m => m.PayloadSegment.SequenceEqual(expected_payload_segment)), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public void TestMqttDataSinkConnect()
    {
        // Arrange
        _mqttSessionClientMock.Setup(m => m.ConnectAsync(It.IsAny<MqttConnectionSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new MqttClientConnectResult { ResultCode = MqttClientConnectResultCode.Success }));

        // Act
        _mqttDataSink.Connect();

        // Assert
        _mqttSessionClientMock.Verify(m => m.ConnectAsync(It.Is<MqttConnectionSettings>(m =>
           m.TcpPort == MQTT_PORT &&
           m.ClientId == MQTT_CLIENT_ID &&
           m.UseTls == MQTT_USE_TLS &&
           m.Username == MQTT_USERNAME &&
           m.PasswordFile == MQTT_PASSWORD_FILE &&
           m.SatAuthFile == MQTT_SAT_AUTH_FILE &&
           m.CaFile == MQTT_CA_FILE), It.IsAny<CancellationToken>()));
    }
}