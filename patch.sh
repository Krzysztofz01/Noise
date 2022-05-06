#!/bin/bash

# Noise peer host patching script
set -e

# Backup local binary and peer data
mv ./bin ./bin-pre

# Fetching the latest version
mkdir ./patch
git clone https://github.com/Krzysztofz01/Noise.git ./patch

# Latest version build and binary preparation installation in temp directory
dotnet publish ./patch/Noise/Noise.Host/Noise.Host.csproj --runtime linux-x64 --output ./patch/build --self-contained true --configuration Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

mkdir ./patch/bin
cp -r ./patch/build/Noise.Host ./patch/bin/noise

# Removing usused files
rm -rf ./patch/build
rm -rf ./patch/Noise
rm -rf ./patch/resources
rm -rf ./patch/.git
rm -f ./patch/.gitignore
rm -f ./patch/build.cmd
rm -f ./patch/patch.cmd
rm -f ./patch/build.sh

# Removing local version files
rm -f LICENSE
rm -f NOTICES
rm -f README.md
rm -f patch.sh

# Replacing the latest version files and peer data transfer
mv ./patch/* ./
mv ./bin-pre/peer.noise ./bin/peer.noise

# Installation cleanup
rm ./bin-pre