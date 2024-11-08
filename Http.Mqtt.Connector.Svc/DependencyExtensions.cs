namespace Http.Mqtt.Connector.Svc;

using System.Net;
using System.Security.Cryptography;
using System.Text;
using Akri.Mqtt.MqttNetAdapter.Session;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

public static class DependencyExtensions
{
    public static IServiceCollection AddConfig(
         this IServiceCollection services, IConfiguration config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        services.Configure<MqttOptions>(
             config.GetSection(MqttOptions.Mqtt));

        services.Configure<HttpOptions>(
                   config.GetSection(HttpOptions.Http));

        services.AddHttpClient<IDataSource, HttpDataSource>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
        .AddPolicyHandler(
        HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        return services;
    }

    public static IServiceCollection AddDependencies(
        this IServiceCollection services)
    {
        // Singleton data source objects per endpoint.
        services.AddSingleton<Dictionary<IDataSource, IDataSink>>(provider =>
        {
            var http_options = provider.GetRequiredService<IOptions<HttpOptions>>();

            // Create a data source and sink for each endpoint.
            var dataSourceSinkMap = new Dictionary<IDataSource, IDataSink>();

            var mqtt_session_client = new MqttSessionClient();
            int endpointCount = 0;
            foreach (var endpoint in http_options.Value.Endpoints)
            {
                // Create http client per http endpoint with the configured base address.
                var http_client = provider.GetRequiredService<HttpClient>();

                // Set the base url and timeout for the http client, default to 5 seconds.
                http_client.Timeout = TimeSpan.FromSeconds(endpoint.TimeOutInSeconds);
                http_client.BaseAddress = new Uri(endpoint.Url);

                var mqtt_options = provider.GetRequiredService<IOptions<MqttOptions>>();

                foreach (var relativeEndpoint in endpoint.RelativeEndpoints)
                {
                    var data_source = new HttpDataSource(
                        provider.GetRequiredService<ILogger<HttpDataSource>>(),
                        http_client,
                        new Uri(relativeEndpoint.Url, UriKind.Relative),
                        relativeEndpoint.PollingInternalInMilliseconds);

                    // Topic name is created here.
                    var topic = SanitizeTopicName(endpoint.Url, relativeEndpoint.Url, mqtt_options.Value.BaseTopic);

                    var data_sink = new MqttDataSink(
                        provider.GetRequiredService<ILogger<MqttDataSink>>(),
                        mqtt_session_client,
                        mqtt_options.Value.Host,
                        mqtt_options.Value.Port,
                        mqtt_options.Value.ClientId,
                        mqtt_options.Value.UseTls,
                        mqtt_options.Value.Username,
                        mqtt_options.Value.Password,
                        mqtt_options.Value.SatFilePath,
                        mqtt_options.Value.CaFilePath,
                        topic);

                    // Connect to the data sink.
                    data_sink.Connect();

                    // Data Source and Sink mapping.
                    dataSourceSinkMap.Add(data_source, data_sink);

                    endpointCount++;
                }
            }

            return dataSourceSinkMap;
        });

        return services;
    }

    private static string SanitizeTopicName(string baseUrl, string relativeUrl, string baseTopic)
    {
        var originalUrl = new Uri(new Uri(baseUrl), relativeUrl).ToString();
        var hash = ComputeSha256Hash(originalUrl);
        var sanitizedUrl = originalUrl.Replace(".", "_", StringComparison.InvariantCultureIgnoreCase);
        return $"{baseTopic}{hash}/{sanitizedUrl}";
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
