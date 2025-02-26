namespace Data.Connector.Svc;

// TODO: Add validations for each field.
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0#options-validation
public class SqlOptions
{
    public const string Sql = "Sql";

    public required SqlServerEndpoint[] SqlServerEndpoints { get; set; }
}
