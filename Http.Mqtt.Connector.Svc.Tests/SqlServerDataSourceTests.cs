namespace Http.Mqtt.Connector.Svc.Tests;

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Polly;
using System.Data.Common;

public class SqlServerDataSourceTests
{
    private readonly Mock<ILogger<SqlServerDataSource>> _loggerMock;
    private readonly Mock<DbProviderFactory> _dbProviderFactoryMock;
    private readonly Mock<DbConnectionStringBuilder> _dbConnectionStringBuilderMock;

    private readonly Mock<DbConnection> _connectionMock;
    private readonly Mock<IAsyncPolicy> _retryPolicyMock;
    private SqlServerDataSource _sqlServerDataSource;
    private readonly string _query = "SELECT * FROM TestTable";
    private readonly int _pollingInternalInMilliseconds = 60000;

    public SqlServerDataSourceTests()
    {
        _loggerMock = new Mock<ILogger<SqlServerDataSource>>();
        _dbProviderFactoryMock = new Mock<DbProviderFactory>();
        _connectionMock = new Mock<DbConnection>();
        _dbConnectionStringBuilderMock = new Mock<DbConnectionStringBuilder>() { CallBase = true }; ;
        _retryPolicyMock = new Mock<IAsyncPolicy>();

        _dbConnectionStringBuilderMock.SetupSet(b => b["Data Source"] = "TestDataSource");
        _dbConnectionStringBuilderMock.SetupSet(b => b["Initial Catalog"] = "TestCatalog");
        _dbConnectionStringBuilderMock.Object.ConnectionString = "Data Source=TestDataSource;Initial Catalog=TestCatalog";

        _dbProviderFactoryMock
            .Setup(factory => factory.CreateConnection())
            .Returns(_connectionMock.Object);

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

}
