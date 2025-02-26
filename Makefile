#! /bin/bash

# Default target
all: build test build_container

run build:
	dotnet run --project Data.Connector.Svc/Data.Connector.Svc.csproj

# TODO: This does not work currently as docker host IP is not configured in the mounted appsettings.json.
docker_run: build_container
	docker run -v ${pwd}./Data.Connector.Svc:/app/settings data-connector:v0.1
	
build:
	dotnet build Data.Connector.Svc/Data.Connector.Svc.csproj

build_container:
	docker build . -f Data.Connector.Svc/Dockerfile -t suneetnangia/data-connector:v0.1

package:
	helm package Data.Connector.Svc/helm/data-connector

test:
	dotnet test Data.Connector.Svc.Tests/Data.Connector.Svc.Tests.csproj

clean:
	dotnet clean Data.Connector.Svc/Data.Connector.Svc.csproj
	dotnet clean Data.Connector.Svc.Tests/Data.Connector.Svc.Tests.csproj
	dotnet nuget locals all --clear