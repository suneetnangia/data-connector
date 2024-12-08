#! /bin/bash

# Default target
all: build test build_container

run build:
	dotnet run --project Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj

# TODO: This does not work currently as docker host IP is not configured in the mounted appsettings.json.
docker_run: build_container
	docker run -v ${pwd}./Http.Mqtt.Connector.Svc:/app/settings http-mqtt-connector:v0.1
	
build:
	dotnet build Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj

build_container:
	docker build . -f Http.Mqtt.Connector.Svc/Dockerfile -t suneetnangia/http-mqtt-connector:v0.1

package:
	helm package Http.Mqtt.Connector.Svc/helm/http-mqtt-connector

test:
	dotnet test Http.Mqtt.Connector.Svc.Tests/Http.Mqtt.Connector.Svc.Tests.csproj

clean:
	dotnet clean Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj
	dotnet clean Http.Mqtt.Connector.Svc.Tests/Http.Mqtt.Connector.Svc.Tests.csproj
	dotnet nuget locals all --clear