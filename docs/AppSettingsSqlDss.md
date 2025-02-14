# Sql Server Data source and AIO DSS reference data sink

## Example appsettings.json for AIO MQTT broker

```json
{
   "Sql" : {
    "SqlServerEndpoints": [
      {
        "DataSource": "localhost",
        "Port": 1433,
        "Username": "",
        "Password": "",
        "TimeOutInSeconds": 60000,
        "TrustServerCertificate": true,
        "Queries": [
            {
              "Query": "SELECT * FROM Mytable",
              "Key": "myreftable",
              "DatabaseName": "mydb",
              "PollingInternalInMilliseconds": 10000
            },
            {
              "Query": "SELECT TOP 3 name, id FROM sys.sysobjects",
              "Key": "test2",
              "DatabaseName": "master",
              "PollingInternalInMilliseconds": 15000
            }
        ]
      }
    ]
  },
  "MqttStateStore": {
    "Host": "aio-broker",
    "Port": 18883,
    "ClientId": "Http.MqttStateStore.Connector.Svc",
    "UseTls": true,
    "SatFilePath": "/var/run/secrets/tokens/broker-sat",
    "CaFilePath": "/var/run/certs/ca.crt"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```
