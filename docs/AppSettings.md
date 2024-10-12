# App Settings File Guide

## Configuration Guide

The appsettings.json example below is configured to poll the following endpoints and send the respective responses to AIO's MQTT broker (MQ) using a pre-configured SAT token (no additional credentials needed).

1. <http://localhost:5126/weatherforecast/africa> with ```5``` seconds of timeout and 2000 milliseconds of polling interval.
2. <http://localhost:5126/weatherforecast/sweden> with ```5``` seconds of timeout and 10000 milliseconds of polling interval.

[Explain additional appsettings.json configuration here.]

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
        "Url": "http://10.128.46.1:80",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/NeoRestApi/v1/neo",
            "PollingInternalInMilliseconds": 10000
          }
        ]
      },
      {
        "Url": "http://10.128.46.2:80",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/NeoRestApi/v1/neo",
            "PollingInternalInMilliseconds": 10000
          }
        ]
      },
      {
        "Url": "http://10.128.46.3:80",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/NeoRestApi/v1/neo",
            "PollingInternalInMilliseconds": 10000
          }
        ]
      },
      {
        "Url": "http://10.128.46.4:80",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/NeoRestApi/v1/neo",
            "PollingInternalInMilliseconds": 10000
          }
        ]
      },
      {
        "Url": "http://10.128.46.5:80",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/NeoRestApi/v1/neo",
            "PollingInternalInMilliseconds": 10000
          }
        ]
      },
      {
        "Url": "http://10.128.46.6:80",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/NeoRestApi/v1/neo",
            "PollingInternalInMilliseconds": 10000
          }
        ]
      },
      {
        "Url": "http://10.128.46.7:80",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/NeoRestApi/v1/neo",
            "PollingInternalInMilliseconds": 10000
          }
        ]
      },
      {
        "Url": "http://10.128.46.8:80",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/NeoRestApi/v1/neo",
            "PollingInternalInMilliseconds": 10000
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
