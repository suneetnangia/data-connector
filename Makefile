#! /bin/bash

# Default target
all: build container

build:
	dotnet build Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj

container:
	docker build . -f Http.Mqtt.Connector.Svc/Dockerfile -t http-mqtt-connector:v0.1

clean:
	dotnet clean Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj