namespace Data.Connector.Svc.Tests;

using Xunit;
using Moq;
using Data.Connector.Svc;
using Microsoft.Extensions.Logging;
using Azure.Iot.Operations.Mqtt.Session;
using Azure.Iot.Operations.Protocol.Models;
using Azure.Iot.Operations.Services.StateStore;
using System.Threading.Tasks;
using Azure.Iot.Operations.Protocol.Connection;

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
    private readonly Mock<IStateStoreClient> _stateStoreClientMock;
    private readonly MqttStateStoreSink _mqttStateStoreSink;

    public MqttStateStoreSinkTests()
    {
        _loggerMock = new Mock<ILogger<MqttStateStoreSink>>();
        _mqttSessionClientMock = new Mock<MqttSessionClient>(null);
        _stateStoreClientMock = new Mock<IStateStoreClient>();

        _mqttStateStoreSink = new MqttStateStoreSink(
            _loggerMock.Object,
            _mqttSessionClientMock.Object,
            _stateStoreClientMock.Object,
            MQTT_HOST,
            MQTT_PORT,
            MQTT_CLIENT_ID,
            MQTT_USE_TLS,
            MQTT_USERNAME,
            MQTT_PASSWORD_FILE,
            MQTT_SAT_AUTH_FILE,
            MQTT_CA_FILE,
            MQTT_KEY);

        // Setup mock behavior
        // _mqttSessionClientMock.Setup(m => m.ConnectAsync(It.IsAny<MqttConnectionSettings>(), It.IsAny<CancellationToken>()));

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
    public void TestStateStoreConnect()
    {
        // Arrange
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
