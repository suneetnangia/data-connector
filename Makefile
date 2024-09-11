#! /bin/bash

# Default target
all: build build_container

run build:
	dotnet run --project Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj

# TODO: This does not work currently as docker host IP is not configured in the mounted appsettings.json.
docker_run: build_container
	docker run -v ${pwd}./Http.Mqtt.Connector.Svc:/app/settings http-mqtt-connector:v0.1	
	
build:
	dotnet build Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj

build_container:
	docker build . -f Http.Mqtt.Connector.Svc/Dockerfile -t http-mqtt-connector:v0.1

package:
	helm package Http.Mqtt.Connector.Svc/helm/http-mqtt-connector

clean:
	dotnet clean Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj
	dotnet nuget locals all --clear