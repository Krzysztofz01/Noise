@echo off

dotnet publish .\Noise\Noise.Host\Noise.Host.csproj --runtime win-x64 --output ./build --self-contained true --configuration Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

MKDIR bin

COPY build\Noise.Host.exe bin\noise.exe

RMDIR /s /q build

RMDIR /s /q Noise

DEL /q build.cmd

DEL /q build.sh

DEL /q patch.sh