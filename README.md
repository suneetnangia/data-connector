# Http Mqtt Connector

A repo to periodically read data from RESTful APIs and publish it on MQTT broker.

## Overview

[Add diagram here]

## Features

1. Poll RESTful endpoints using ```GET``` verb.
2. Configurable polling interval for RESTful endpoints.
3. MQTT connectivity using anonymous, username/password or SAT token in Azure IoT Operations. [WIP]

## Backlog

Please refer to the project board [here](https://github.com/users/suneetnangia/projects/3).

## Development Loop

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/suneetnangia/http-mqtt-connector/)

## Deploy in K8s

1. Add Helm Repo

    ```helm repo add aio-extensions https://raw.githubusercontent.com/suneetnangia/http-mqtt-connector/release_management```

2. Copy configuration file from [here](Http.Mqtt.Connector.Svc/appsettings.json) and update with your specifics.

3. Install Helm Package (appsettings.json content is stored as a K8s secret)

    ```helm install http-mqtt-connector-01 aio-extensions/http-mqtt-connector --namespace aio-extensions --create-namespace --set-file appsettingsContent=$pwd./<path to your>/appsettings.json```
