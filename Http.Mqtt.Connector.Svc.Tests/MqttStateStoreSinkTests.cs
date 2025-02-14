namespace Http.Mqtt.Connector.Svc.Tests;

using Xunit;
using Moq;
using Http.Mqtt.Connector.Svc;
using Microsoft.Extensions.Logging;
using Azure.Iot.Operations.Mqtt.Session;
using Azure.Iot.Operations.Protocol.Models;
using Azure.Iot.Operations.Services.StateStore;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Iot.Operations.Protocol.Connection;
using System.Text;
using System.Text.Json.Nodes;

public class MqttStateStoreSinkTests
{
    const string MQTT_HOST = "testhost";
    const int MQTT_PORT = 1883;
    const string MQTT_CLIENT_ID = "Http.MqttStateStore.Connector.Svc";
    const bool MQTT_USE_TLS = false;
    const string MQTT_USERNAME = "user01";
    const string MQTT_PASSWORD_FILE = "/path/to/password/file";
    const string MQTT_SAT_AUTH_FILE = "/path/to/sat/file";
    const string MQTT_CA_FILE = "/path/to/ca/file";
    const string MQTT_KEY = "testkey";

    private readonly Mock<ILogger<MqttStateStoreSink>> _loggerMock;
    private readonly Mock<MqttSessionClient> _mqttSessionClientMock;
    private readonly Mock<StateStoreClient> _stateStoreClientMock;
    private readonly MqttStateStoreSink _mqttStateStoreSink;

    public MqttStateStoreSinkTests()
    {
        _loggerMock = new Mock<ILogger<MqttStateStoreSink>>();
        _mqttSessionClientMock = new Mock<MqttSessionClient>(null);
        _stateStoreClientMock = new Mock<StateStoreClient>(_mqttSessionClientMock.Object);

        _mqttStateStoreSink = new MqttStateStoreSink(
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
            MQTT_KEY);
    }

    [Fact]
    public void TestMqttStateStoreSinkInitialization()
    {
        // Arrange
        // Act        
        // Assert        
        Assert.NotNull(_mqttStateStoreSink);        
        Assert.Equal($"{MQTT_CLIENT_ID}-{MQTT_HOST}-{MQTT_PORT}-{MQTT_KEY}", _mqttStateStoreSink.Id);
    }

    [Fact]
    public async Task TestMqttStateStoreSinkPushDataAsync()
    {
        // Arrange
        var jsonArray = new JsonArray
        {
            new JsonObject
            {
                ["key1"] = "value1",
                ["key2"] = 2,
                ["key3"] = true
            },
            new JsonObject
            {
                ["key1"] = "value2",
                ["key2"] = 3,
                ["key3"] = false
            }
        };

        using var test_json_document = JsonDocument.Parse(jsonArray.ToJsonString());

        var formattedContent = new StringBuilder();
        foreach (var element in test_json_document.RootElement.EnumerateArray())
        {
            formattedContent.AppendLine(element.ToString());
        }

        var content = formattedContent.ToString();

        // Act
        await _mqttStateStoreSink.PushDataAsync(test_json_document, CancellationToken.None);

        // Assert
        _stateStoreClientMock.Verify(m => m.SetAsync(
            It.Is<string>(m => m == MQTT_KEY), 
            It.Is<string>(m => m == content),
            It.IsAny<StateStoreSetRequestOptions>(),
            It.Is<TimeSpan>(m => m == TimeSpan.FromSeconds(30)),
            It.IsAny<CancellationToken>())
        );
    }

    [Fact]
    public void TestMqttStateStoreSinkConnect()
    {
        // Arrange
        _mqttSessionClientMock.Setup(m => m.ConnectAsync(It.IsAny<MqttConnectionSettings>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new MqttClientConnectResult { ResultCode = MqttClientConnectResultCode.Success }));

        // Act
        _mqttStateStoreSink.Connect();

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
