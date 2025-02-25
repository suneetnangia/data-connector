# Data Connector

A repo to periodically reads data from the configured data sources and publish it to the their paired data sinks, at the configurable interval.

This repo currently supports the following Data Source-Sink Pairs:

1. RESTful APIs to MQTT Broker
2. SQL Server to AIO Distributed Data Store (DSS)

## Overview

![Design](docs/Design.png)

> **Http-MQTT:** To understand MQTT topic generation and its customization options please refer to the document [here](docs/AppSettingsHttpMqtt.md#Supported-Customizations).

> **SQL-DSS:** To understand SQL and DS customization options please refer to the document [here](docs/AppSettingsSqlDss.md#Supported-Customizations).

## Features

1. Configurable polling on RESTful endpoints using `GET` verb.
2. Configurable Polling on SQL DB endpoints using SQL `DQL` queries.
3. MQTT and DSS connectivity using anonymous, username/password or SAT token in Azure IoT Operations.

## Deploy in K8s

1. Add Helm Repo:

    ```helm repo add aio-extensions https://raw.githubusercontent.com/suneetnangia/http-mqtt-connector/release_management```

2. Copy configuration file from the example:
   1. For REST-MQTT AIO Connector [here](docs/AppSettingsHttpMqtt.md#Example-appsettings.json-for-AIO-MQTT-broker).
   2. For REST-MQTT Non-AIO [here](docs/AppSettingsHttpMqtt.md#Example-appsettings.json-for-non-AIO-MQTT-broker).
   3. For SQL-DSS AIO Connector [here](docs/AppSettingsSqlDss.md).

3. Update configuration file with your specifics, refer to the specific links in step 2 above.

4. Install Helm package (appsettings.json content is stored as a K8s secret):
   1. For AIO:

        ```helm install http-mqtt-connector-01 aio-extensions/aio-http-mqtt-connector --namespace azure-iot-operations --create-namespace --set-file appsettingsContent=$pwd./<path to your>/appsettings.json```

   2. For non-AIO

        ```helm install http-mqtt-connector-01 aio-extensions/http-mqtt-connector --namespace azure-iot-operations --create-namespace --set-file appsettingsContent=$pwd./<path to your>/appsettings.json```

5. Uninstall Helm package (after evaluation):

    ```helm uninstall http-mqtt-connector-01 -n azure-iot-operations```

## Backlog

Please refer to the project board [here](https://github.com/users/suneetnangia/projects/3).

## Development Loop

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/suneetnangia/http-mqtt-connector/)
