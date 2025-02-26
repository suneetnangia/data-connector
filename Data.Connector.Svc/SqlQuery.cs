namespace Data.Connector.Svc;

public class SqlQuery
{
    public required string Query { get; set; }

    public required string Name { get; set; }

    public required string Key { get; set; }

    public required string DatabaseName { get; set; }

    public int PollingInternalInMilliseconds { get; set; } = 60000;
}
