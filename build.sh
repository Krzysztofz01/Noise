#!/bin/bash

set -e

dotnet publish ./Noise/Noise.Host/Noise.Host.csproj --runtime linux-x64 --output ./build --self-contained true --configuration Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

mkdir ./bin

cp -r ./build/Noise.Host ./bin/noise

rm -rf build

rm -rf Noise

rm -f build.cmd

rm -f build.sh

mr -f patch.cmd