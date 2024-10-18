# App Settings File Guide

## Configuration Guide

The appsettings.json example below is configured to poll the following endpoints and send the respective responses to the configured MQTT broker.

## Supported Macros

Relative Urls support the following macros:

1. ```{yyyy-mm-dd}``` will replace it with the current date, example

   ```/api/breed/hound/list/{yyyy-mm-dd}``` -> ```/api/breed/hound/list/2024-10-09```
2. ```{yyyy-mm}``` will replace it with the current date, example

   ```/api/breed/hound/{yyyy-mm}/list``` -> ```/api/breed/hound/2024-10/list```

## Example appsettings.json for AIO MQTT broker

```json
{
 "Http": {
    "Endpoints": [
      {
        "Url": "https://dog.ceo",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/api/breed/hound/list",
            "PollingInternalInMilliseconds": 2000
          },
          {
            "Url": "/api/breed/greyhound/list",
            "PollingInternalInMilliseconds": 2000
          }
        ]
      },
      {
        "Url": "https://dog.ceo",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/api/breed/bulldog/list",
            "PollingInternalInMilliseconds": 2000
          },
          {
            "Url": "/api/breed/retriever/list",
            "PollingInternalInMilliseconds": 2000
          }
        ]
      }
    ]
  },
  "Mqtt": {
    "Host": "aio-broker",
    "Port": 18883,
    "ClientId": "Http.Mqtt.Connector.Svc",
    "UseTls": true,
    "SatFilePath": "/var/run/secrets/tokens/broker-sat",
    "CaFilePath": "/var/run/certs/ca.crt",
    "BaseTopic": "azure-iot-operations/data/"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

## Example appsettings.json for non AIO MQTT broker

```json
{
 "Http": {
    "Endpoints": [
      {
        "Url": "https://dog.ceo",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/api/breed/hound/list",
            "PollingInternalInMilliseconds": 2000
          },
          {
            "Url": "/api/breed/greyhound/list",
            "PollingInternalInMilliseconds": 2000
          }
        ]
      },
      {
        "Url": "https://dog.ceo",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/api/breed/bulldog/list",
            "PollingInternalInMilliseconds": 2000
          },
          {
            "Url": "/api/breed/retriever/list",
            "PollingInternalInMilliseconds": 2000
          }
        ]
      }
    ]
  },
  "Mqtt": {
    "Host": "broker.emqx.io",
    "Port": 1883,
    "ClientId": "Http.Mqtt.Connector.Svc",
    "UseTls": false,
    "Username": "",
    "Password": "",
    "BaseTopic": "azure-iot-operations/data/"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```
