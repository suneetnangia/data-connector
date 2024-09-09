#! /bin/bash

# Default target
all: build

build:
	dotnet build Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj

clean:
	dotnet clean Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj