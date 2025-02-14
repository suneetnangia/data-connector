namespace Http.Mqtt.Connector.Svc;

public class SqlServerEndpoint
{
    public required string DataSource { get; set; }

    public string Port { get; set; } = "1433";

    // Currently assuming Username and Password based auth - in future add support for other auth types
    public required string Username { get; set; }

    public required string Password { get; set; }

    public bool TrustServerCertificate { get; set; } = false;

    public string? CertAuthorityServerPath { get; set; }

    public double TimeOutInSeconds { get; set; } = 10;

    public required SqlQuery[] Queries { get; set; }
}
