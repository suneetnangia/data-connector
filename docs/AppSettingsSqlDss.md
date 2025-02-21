# Sql Server Data source and AIO DSS reference data sink - App Settings Guide

## Configuration Guide

The appsettings.json example below is configured to poll the following SQL based endpoints based on a SQL query definition and send the resulting query to AIO's Distributed Data Store (DSS) at the configured `key`. Every update to the key value is complete, meaning we replace its entire contents on every run.

> NOTE: The format of the data stored into the DSS is JSON Lines, as required by Data flow's [enrich](https://learn.microsoft.com/azure/iot-operations/connect-to-cloud/concept-dataflow-enrich) with reference data feature.

## Supported Configurations

### SQL Server Endpoint element

You can connect to multiple servers, with a set of one or more queries per server. Following is an example of the settings for each of the items in the array `SqlServerEndpoints`.

```json
{
  "DataSource": "localhost",
  "Port": 1433,
  "Username": "",
  "Password": "",
  "TimeOutInSeconds": 60000,
  "TrustServerCertificate": true,
  "Queries": [ ]
}

```

### Queries collection per endpoint

Each element in the `Queries` array allows for configuring a SQL query to a database within the server. The `key` is the name of the DSS key to upsert.

```json
{
  "Query": "SELECT * FROM Mytable",
  "Key": "myreftable",
  "DatabaseName": "mydb",
  "PollingInternalInMilliseconds": 10000
}
```

## Full example appsettings.json for AIO DSS

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
