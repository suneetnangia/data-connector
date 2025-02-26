namespace Data.Connector.Svc;

// TODO: Add validations for each field.
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0#options-validation
public class HttpOptions
{
    public const string Http = "Http";

    public required HttpEndpoint[] Endpoints { get; set; }
}
