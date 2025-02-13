namespace Http.Mqtt.Connector.Svc;

using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.SqlClient;
using Polly;

public class SqlServerDataSource : IDataSource
{
    private readonly ILogger _logger;
    private readonly SqlClientFactory _sqlClientFactory;

    private readonly DbConnectionStringBuilder _dbConnectionStringBuilder;
    private readonly string _query;

    private readonly IAsyncPolicy _retryPolicy;

    public SqlServerDataSource(
        ILogger logger,
        SqlClientFactory sqlClientFactory,
        DbConnectionStringBuilder connectionStringBuilder,
        IAsyncPolicy retryPolicy,
        string query,
        int pollingInternalInMilliseconds)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sqlClientFactory = sqlClientFactory ?? throw new ArgumentNullException(nameof(sqlClientFactory));
        _dbConnectionStringBuilder = connectionStringBuilder ?? throw new ArgumentNullException(nameof(connectionStringBuilder));
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
        _query = query ?? throw new ArgumentNullException(nameof(query));
        PollingInternalInMilliseconds = pollingInternalInMilliseconds;

        // Set the unique identifier for the data source for observability.
        Id = $"{_dbConnectionStringBuilder.ConnectionString.GetHashCode(StringComparison.CurrentCulture)}-'{_query}'";
        _logger.LogTrace("Configured sql query endpoint, Id: {Id}", Id);
    }

    public string Id { get; init; }

    public int PollingInternalInMilliseconds { get; init; }

    public async Task<JsonDocument> PullDataAsync(CancellationToken stoppingToken)
    {
        _logger.LogTrace("Executing query on server, Id: {Id}", Id);

        return await _retryPolicy.ExecuteAsync(
            async (stoppingToken) =>
        {
            using (var connection = _sqlClientFactory.CreateConnection())
            {
                connection.ConnectionString = _dbConnectionStringBuilder.ConnectionString;
                await connection.OpenAsync(stoppingToken);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = _query;
                    command.CommandType = CommandType.Text;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        using (var dataTable = new DataTable())
                        {
                            dataTable.Load(reader);
                            var jsonArray = new JsonArray();

                            foreach (DataRow row in dataTable.Rows)
                            {
                                var jsonObject = new JsonObject();

                                foreach (DataColumn column in dataTable.Columns)
                                {
                                    jsonObject[column.ColumnName] = JsonValue.Create(row[column]);
                                }

                                jsonArray.Add(jsonObject);
                            }

                            return JsonDocument.Parse(jsonArray.ToJsonString());
                        }
                    }
                }
            }
        },
            stoppingToken);
    }
}
