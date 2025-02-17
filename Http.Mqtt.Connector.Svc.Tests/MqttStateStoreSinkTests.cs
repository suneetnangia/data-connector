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

    [Fact]
    public async Task TestMqttStateStoreSinkPushDataAsync()
    {
        // Arrange
        _mqttSessionClientMock.Setup(m => m.ConnectAsync(It.IsAny<MqttConnectionSettings>(), It.IsAny<CancellationToken>()))
           .Returns(Task.FromResult(new MqttClientConnectResult { ResultCode = MqttClientConnectResultCode.Success }));

        // TODO this still needs work - not able to mock
        var stateStoreSetResponseMock = new Mock<StateStoreSetResponse>();
        stateStoreSetResponseMock.Setup(m => m.Success).Returns(true);

        _stateStoreClientMock.Setup(m => m.SetAsync(
            It.IsAny<StateStoreKey>(),
            It.IsAny<StateStoreValue>(),
            It.IsAny<StateStoreSetRequestOptions>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(stateStoreSetResponseMock.Object);

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
        await _mqttStateStoreSink.PushDataAsync(test_json_document, CancellationToken.None).ConfigureAwait(false);

        // Assert
        _stateStoreClientMock.Verify(m => m.SetAsync(
            It.Is<string>(m => m == MQTT_KEY), 
            It.Is<string>(m => m == formattedContent.ToString()),
            It.IsAny<StateStoreSetRequestOptions>(),
            It.Is<TimeSpan>(m => m == TimeSpan.FromSeconds(30)),
            It.IsAny<CancellationToken>())
        );
    }
}

// Derived class to simulate StateStoreSetResponse
// public class MockStateStoreSetResponse : StateStoreSetResponse
// {
//     public new bool Success { get; set; }
// }

// public class CustomStateStoreClient : IStateStoreClient
// {
//     public event Func<object?, KeyChangeMessageReceivedEventArgs, Task>? KeyChangeMessageReceivedAsync;

//     public Task<StateStoreDeleteResponse> DeleteAsync(StateStoreKey key, StateStoreDeleteRequestOptions? options = null, TimeSpan? requestTimeout = null, CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }

//     public ValueTask DisposeAsync(bool disposing)
//     {
//         throw new NotImplementedException();
//     }

//     public ValueTask DisposeAsync()
//     {
//         throw new NotImplementedException();
//     }

//     public Task<StateStoreGetResponse> GetAsync(StateStoreKey key, TimeSpan? requestTimeout = null, CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }

//     public Task ObserveAsync(StateStoreKey key, StateStoreObserveRequestOptions? options = null, TimeSpan? requestTimeout = null, CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }

//     // public Task<StateStoreSetResponse> SetAsync(
//     //     StateStoreKey key,
//     //     StateStoreValue value,
//     //     StateStoreSetRequestOptions options,
//     //     TimeSpan timeout,
//     //     CancellationToken cancellationToken)
//     // {
//     //     // Create the StateStoreSetResponse instance
//     //     var constructorInfo = typeof(StateStoreSetResponse).GetConstructor(
//     //         BindingFlags.Instance | BindingFlags.NonPublic,
//     //         null,
//     //         new Type[] { typeof(HybridLogicalClock), typeof(bool) },
//     //         null);

//     //     var hybridLogicalClock = new HybridLogicalClock(); // Assuming a parameterless constructor
//     //     var instance = (StateStoreSetResponse)constructorInfo.Invoke(new object[] { hybridLogicalClock, true });

//     //     return Task.FromResult(instance);
//     // }

//     public Task<StateStoreSetResponse> SetAsync(
//         StateStoreKey key,
//         StateStoreValue value,
//         StateStoreSetRequestOptions options,
//         TimeSpan timeout,
//         CancellationToken cancellationToken)
//     {
//         // Directly create the StateStoreSetResponse instance
//         var response = new StateStoreSetResponseWrapper
//         {
//             Success = true // Set the desired properties
//         };

//         return Task.FromResult(response);
//     }

//     public Task<StateStoreSetResponse> SetAsync(StateStoreKey key, StateStoreValue value, StateStoreSetRequestOptions? options = null, TimeSpan? requestTimeout = null, CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }

//     public Task UnobserveAsync(StateStoreKey key, TimeSpan? requestTimeout = null, CancellationToken cancellationToken = default)
//     {
//         throw new NotImplementedException();
//     }
// }

// // Wrapper class to simulate StateStoreSetResponse
// public class MockStateStoreSetResponse : StateStoreSetResponse
// {
//     public MockStateStoreSetResponse() : base(new HybridLogicalClock(), true) { }

//     public new bool Success { get; set; }
// }
