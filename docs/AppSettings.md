# App Settings File Guide

## Configuration Guide

The appsettings.json example below is configured to poll the following endpoints and send the respective responses to AIO's MQTT broker (MQ) using a pre-configured SAT token (no additional credentials needed).

1. <http://localhost:5126/weatherforecast/africa> with ```5``` seconds of timeout and 2000 milliseconds of polling interval.
2. <http://localhost:5126/weatherforecast/sweden> with ```5``` seconds of timeout and 10000 milliseconds of polling interval.

[Explain additional appsettings.json configuration here.]

## Example appsettings.json

```json
{
  "Http": {
    "Endpoints": [
      {
        "Url": "http://localhost:5126",
        "TimeOutInSeconds": 5,
        "RelativeEndpoints": [
          {
            "Url": "/weatherforecast/africa",
            "PollingInternalInMilliseconds": 2000
          },
          {
            "Url": "/weatherforecast/sweden",
            "PollingInternalInMilliseconds": 10000
          }
        ]
      }
    ]
  },
  "Mqtt": {
    "Host": "aio-mq-dmqtt-frontend",
    "Port": 8883,
    "ClientId": "Http.Mqtt.Connector.Svc",
    "UseTls": true,
    "Username": "",
    "Password": "",
    "SatFilePath": "/var/run/secrets/tokens/mq-sat",
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
