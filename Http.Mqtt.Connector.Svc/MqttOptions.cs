namespace Http.Mqtt.Connector.Svc;

// TODO: Add validations for each field.
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0#options-validation
public class MqttOptions
{
    public const string Mqtt = "Mqtt";

    public required string Host { get; set; }

    public int Port { get; set; } = 1883;

    public string ClientId { get; set; } = Guid.NewGuid().ToString();

    public bool UseTls { get; set; } = false;

    public string BaseTopic { get; set; } = "azure-iot-operations/data/";

    // Add more options as needed
}
