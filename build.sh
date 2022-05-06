#!/bin/bash

# Noise peer host installation script
set -e

# Latest version build and binary preparation installation in temp directory
dotnet publish ./Noise/Noise.Host/Noise.Host.csproj --runtime linux-x64 --output ./build --self-contained true --configuration Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

mkdir ./bin
cp -r ./build/Noise.Host ./bin/noise

# Removing usused files
rm -rf build
rm -rf Noise
rm -rf resources
rm -rf .git
rm -f .gitignore
rm -f build.cmd
rm -f patch.cmd
rm -f build.sh