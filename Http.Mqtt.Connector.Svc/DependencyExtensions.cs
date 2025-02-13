namespace Http.Mqtt.Connector.Svc;

using System.Net;
using Azure.Iot.Operations.Mqtt.Session;
using Microsoft.Data.SqlClient;
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

        services.Configure<MqttStateStoreOptions>(
                   config.GetSection(MqttStateStoreOptions.MqttStateStore));

        services.Configure<SqlOptions>(
             config.GetSection(SqlOptions.Sql));

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
        services.AddSingleton<SqlClientFactory>(SqlClientFactory.Instance);

        // Singleton data source objects per endpoint.
        services.AddSingleton<Dictionary<IDataSource, IDataSink>>(provider =>
        {
            var dataSourceSinkMap = new Dictionary<IDataSource, IDataSink>();

            var sql_options = provider.GetRequiredService<IOptions<SqlOptions>>();

            if (sql_options.Value.SqlServerEndpoints is not null)
            {
                var sql_client_factory = provider.GetRequiredService<SqlClientFactory>();

                var mqtt_state_session_client = new MqttSessionClient();

                var sqlRetryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    6,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (
                    exception,
                    timeSpan,
                    retryCount,
                    context) =>
                    {
                        // Log the retry attempt
                        var logger = provider.GetRequiredService<ILogger<SqlServerDataSource>>();
                        logger.LogWarning($"Retry {retryCount} encountered an error: {exception.Message}. Waiting {timeSpan} before next retry.");
                    });

                foreach (var server in sql_options.Value.SqlServerEndpoints)
                {
                    Console.WriteLine($"Server: {server.DataSource}, {server.InitialCatalog}, {server.Username}, ***, {server.TrustServerCertificate}");

                    var mqtt_state_store_options = provider.GetRequiredService<IOptions<MqttStateStoreOptions>>();

                    foreach (var query in server.Queries)
                    {
                        var db_connection_builder = sql_client_factory.CreateConnectionStringBuilder();
                        db_connection_builder["Data Source"] = server.DataSource;
                        db_connection_builder["User Id"] = server.Username;
                        db_connection_builder["Password"] = server.Password;
                        db_connection_builder["TrustServerCertificate"] = server.TrustServerCertificate;
                        db_connection_builder["Initial Catalog"] = query.DatabaseName;

                        var data_source = new SqlServerDataSource(
                            provider.GetRequiredService<ILogger<SqlServerDataSource>>(),
                            SqlClientFactory.Instance,
                            db_connection_builder,
                            sqlRetryPolicy,
                            query.Query,
                            query.PollingInternalInMilliseconds);

                        var data_sink = new MqttStateStoreSink(
                            provider.GetRequiredService<ILogger<MqttStateStoreSink>>(),
                            mqtt_state_session_client,
                            mqtt_state_store_options.Value.Host,
                            mqtt_state_store_options.Value.Port,
                            mqtt_state_store_options.Value.ClientId,
                            mqtt_state_store_options.Value.UseTls,
                            mqtt_state_store_options.Value.Username,
                            mqtt_state_store_options.Value.PasswordFilePath,
                            mqtt_state_store_options.Value.SatFilePath,
                            mqtt_state_store_options.Value.CaFilePath,
                            query.Key,
                            data_source.Id,
                            initialBackoffDelayInMilliseconds: 500,
                            maxBackoffDelayInMilliseconds: 10_000);

                        // Connect to the data sink.
                        data_sink.Connect();

                        // Data Source and Sink mapping.
                        dataSourceSinkMap.Add(data_source, data_sink);
                    }
                }
            }

            // Get the http options from the DI container and add the http client.
            var http_options = provider.GetRequiredService<IOptions<HttpOptions>>();

            if (http_options.Value.Endpoints is not null)
            {
                // Create a data source and sink for each endpoint in http_options.
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

                        var data_sink = new MqttDataSink(
                            provider.GetRequiredService<ILogger<MqttDataSink>>(),
                            mqtt_session_client,
                            mqtt_options.Value.Host,
                            mqtt_options.Value.Port,
                            mqtt_options.Value.ClientId,
                            mqtt_options.Value.UseTls,
                            mqtt_options.Value.Username,
                            mqtt_options.Value.PasswordFilePath,
                            mqtt_options.Value.SatFilePath,
                            mqtt_options.Value.CaFilePath,
                            mqtt_options.Value.BaseTopic,
                            data_source.Id,
                            topicStringReplacements: mqtt_options.Value.TopicStringReplacements);

                        // Connect to the data sink.
                        data_sink.Connect();

                        // Data Source and Sink mapping.
                        dataSourceSinkMap.Add(data_source, data_sink);

                        endpointCount++;
                    }
                }
            }

            return dataSourceSinkMap;
        });

        return services;
    }
}
