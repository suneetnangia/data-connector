namespace Http.Mqtt.Connector.Svc;

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
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        return services;
    }

    public static IServiceCollection AddDependencies(
        this IServiceCollection services)
    {
        // Singleton data source objects per endpoint.
        services.AddSingleton<IDataSource[]>(provider =>
        {
            var http_options = provider.GetRequiredService<IOptions<HttpOptions>>();

            // Create a data source for each endpoint.
            var dataSources = new HttpDataSource[http_options.Value.Endpoints
                .Sum(endpoint => endpoint.RelativeEndpoints.Count())];

            int endpointCount = 0;
            foreach (var endpoint in http_options.Value.Endpoints)
            {
                // Create http client per http endpoint with the configured base address.
                var http_client = provider.GetRequiredService<HttpClient>();

                // Set the base url and timeout for the http client, default to 5 seconds.
                http_client.Timeout = TimeSpan.FromSeconds(endpoint.TimeOutInSeconds);
                http_client.BaseAddress = new Uri(endpoint.Url);

                foreach (var relativeEndpoint in endpoint.RelativeEndpoints)
                {
                    dataSources[endpointCount] = new HttpDataSource(
                        provider.GetRequiredService<ILogger<HttpDataSource>>(),
                        http_client,
                        new Uri(endpoint.Url),
                        new Uri(relativeEndpoint.Url, UriKind.Relative),
                        relativeEndpoint.PollingInternalInMilliseconds);

                    endpointCount++;
                }
            }

            return dataSources;
        });

        services.AddSingleton<IDataSink>(provider =>
        {
            var mqtt_options = provider.GetRequiredService<IOptions<MqttOptions>>();

            var mqtt_session_client = new MqttSessionClient();

            return new MqttDataSink(
                provider.GetRequiredService<ILogger<MqttDataSink>>(),
                mqtt_session_client,
                mqtt_options.Value.Host,
                mqtt_options.Value.Port,
                mqtt_options.Value.ClientId,
                mqtt_options.Value.UseTls,
                mqtt_options.Value.BaseTopic);
        });

        return services;
    }
}
