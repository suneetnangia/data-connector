#! /bin/bash

# Default target
all: setup build

setup:
	dotnet add Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj package Akri.Mqtt.MqttNetAdapter --version 0.4.222-alpha
	dotnet add Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj package Akri.Mq --version 0.4.222-alpha

build:
	dotnet build Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj

clean:
	dotnet clean Http.Mqtt.Connector.Svc/Http.Mqtt.Connector.Svc.csproj