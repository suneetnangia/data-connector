namespace Http.Mqtt.Connector.Svc.Tests;

using Xunit;
using Moq;
using Http.Mqtt.Connector.Svc;
using Microsoft.Extensions.Logging;
using System.Net;
using Moq.Protected;

public class DataSourceTests
{
    const int HTTP_RELATIVE_URL_POLLING_INTERVAL_IN_SECONDS = 10;
    private readonly Mock<ILogger<MqttDataSink>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly HttpDataSource _httpDataSource;
    private readonly Uri _http_base_url;
    private readonly Uri _http_relative_url;

    public DataSourceTests()
    {
        _http_base_url = new Uri("http://testhost");
        _http_relative_url = new Uri("/api/breed/greyhound/list", UriKind.Relative);

        _loggerMock = new Mock<ILogger<MqttDataSink>>();
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object) { BaseAddress = _http_base_url };

        _httpDataSource = new HttpDataSource(
                       _loggerMock.Object,
                       _httpClient,
                       _http_relative_url,
                    HTTP_RELATIVE_URL_POLLING_INTERVAL_IN_SECONDS);
    }
    [Fact]
    public void TestHttpDataSourceInitialization()
    {
        // Arrange
        // Act
        // Assert
        Assert.NotNull(_httpDataSource);
        Assert.Equal(new Uri(_http_base_url, _http_relative_url).ToString(), _httpDataSource.Id);
    }

    [Fact]
    public async Task TestMqttDataSourcePullDataAsync()
    {
        // Arrange
        _httpHandlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(() =>
           {
               var response = new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("{\"breeds\":[\"greyhound\"]}")
               };
               return response;
           });


        // Act
        var response = await _httpDataSource.PullDataAsync(CancellationToken.None);

        // Assert
        Assert.Equal("{\"breeds\":[\"greyhound\"]}", response.RootElement.GetRawText());
    }
}