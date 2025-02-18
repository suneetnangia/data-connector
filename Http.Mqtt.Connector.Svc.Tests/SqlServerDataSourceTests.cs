namespace Http.Mqtt.Connector.Svc.Tests;

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Polly;
using System.Data.Common;
using System.Threading.Tasks;
using System.Data;
using System.Text.Json;

public class SqlServerDataSourceTests
{
    private readonly Mock<ILogger<SqlServerDataSource>> _loggerMock;
    private readonly Mock<DbProviderFactory> _dbProviderFactoryMock;
    private readonly Mock<DbConnectionStringBuilder> _dbConnectionStringBuilderMock;

    private readonly Mock<TestDbConnection> _connectionMock;
    private readonly Mock<IAsyncPolicy> _retryPolicyMock;
    private SqlServerDataSource _sqlServerDataSource;
    private readonly string _query = "SELECT * FROM TestTable";
    private readonly int _pollingInternalInMilliseconds = 60000;
    private readonly Mock<TestDbCommand> _commandMock;

    public SqlServerDataSourceTests()
    {
        _loggerMock = new Mock<ILogger<SqlServerDataSource>>();
        _dbProviderFactoryMock = new Mock<DbProviderFactory>();
        _dbConnectionStringBuilderMock = new Mock<DbConnectionStringBuilder>() { CallBase = true }; ;
        _retryPolicyMock = new Mock<IAsyncPolicy>();

        _connectionMock = new Mock<TestDbConnection>();
        _commandMock = new Mock<TestDbCommand>();

        // var mockBuilder = new Mock<DbConnectionStringBuilder>();
        // mockBuilder.SetupSet(b => b["Data Source"] = "TestDataSource");
        // mockBuilder.SetupSet(b => b["Initial Catalog"] = "TestCatalog");
        // mockBuilder.Setup(b => b.ConnectionString).Returns("Data Source=TestDataSource;Initial Catalog=TestCatalog");
        // var mockBuilder = new Mock<CustomDbConnectionStringBuilder> { CallBase = true };
        _dbConnectionStringBuilderMock.SetupSet(b => b["Data Source"] = "TestDataSource");
        _dbConnectionStringBuilderMock.SetupSet(b => b["Initial Catalog"] = "TestCatalog");
        _dbConnectionStringBuilderMock.Object.ConnectionString = "Data Source=TestDataSource;Initial Catalog=TestCatalog";


        // _dbConnectionStringBuilderMock.SetupGet(m => m.ConnectionString).Returns("Server=test;Database=testdb;User Id=testuser;Password=testpassword;");

        _dbProviderFactoryMock
            .Setup(factory => factory.CreateConnection())
            .Returns(_connectionMock.Object);

        // _connectionMock.As<ITestDbConnection>()
        //     .Setup(c => c.CreateCommand())
        //     .Returns(_commandMock.Object);

        // _commandMock.As<ITestDbCommand>()
        //     .Setup(c => c.ExecuteReaderAsync(It.IsAny<CancellationToken>()))
        //     .ReturnsAsync(new Mock<DbDataReader>().Object);

        _sqlServerDataSource = new SqlServerDataSource(
            _loggerMock.Object,
            _dbProviderFactoryMock.Object,
            _dbConnectionStringBuilderMock.Object,
            _retryPolicyMock.Object,
            _query,
            _pollingInternalInMilliseconds);
    }

    [Fact]
    public void TestSqlServerDataSourceInitialization()
    {
        // Arrange
        // Act
        // Assert
        Assert.NotNull(_sqlServerDataSource);
        Assert.Equal($"{_dbConnectionStringBuilderMock.Object.ConnectionString.GetHashCode(StringComparison.CurrentCulture)}-'{_query}'", _sqlServerDataSource.Id);
    }

    [Fact]
    public async Task TestSqlServerDataSourcePullDataAsync()
    {
        // Arrange
        var readerMock = new Mock<DbDataReader>();
        _connectionMock.As<ITestDbConnection>()
            .Setup(c => c.CreateCommand())
           .Returns(_commandMock.Object);

        // _commandMock.As<ITestDbCommand>()
        //     .Setup(c => c.ExecuteReaderAsync(It.IsAny<CancellationToken>()))
        //     .ReturnsAsync(new Mock<DbDataReader>().Object);

        using (var dataTable = new DataTable())
        {
            dataTable.Columns.Add("Column1");
            dataTable.Rows.Add("Value1");

            readerMock.Setup(r => r.Read()).Returns(() => dataTable.Rows.Count > 0);
            readerMock.Setup(r => r.GetName(It.IsAny<int>())).Returns((int i) => dataTable.Columns[i].ColumnName);
            readerMock.Setup(r => r.GetValue(It.IsAny<int>())).Returns((int i) => dataTable.Rows[0][i]);
        }

        _retryPolicyMock
            .Setup(p => p.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<JsonDocument>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<JsonDocument>> func, CancellationToken token) => func(token));

        // Act
        var result = await _sqlServerDataSource.PullDataAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("[{\"Column1\":\"Value1\"}]", result.RootElement.GetRawText());
    }

    public class CustomDbConnectionStringBuilder : DbConnectionStringBuilder
    {
        public new string ConnectionString { get; set; } = string.Empty;
    }

    public class TestDbConnection : DbConnection, ITestDbConnection
    {
        public override string ConnectionString { get; set; }
        public override string Database => throw new NotImplementedException();
        public override string DataSource => throw new NotImplementedException();
        public override string ServerVersion => throw new NotImplementedException();
        public override ConnectionState State => throw new NotImplementedException();

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            return new Mock<DbCommand>().Object;
        }

        DbCommand ITestDbConnection.CreateCommand()
        {
            return CreateDbCommand();
        }
    }

    public interface ITestDbConnection
    {
        DbCommand CreateCommand();
    }

    public class TestDbCommand : DbCommand, ITestDbCommand
    {
        public override string? CommandText { get; set; } = String.Empty;
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }

        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection => throw new NotImplementedException();
        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public Task<DbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new Mock<DbDataReader>().Object);
        }
    }

    public interface ITestDbCommand
    {
        Task<DbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken);
    }

}
