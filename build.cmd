@echo off

dotnet publish .\Noise\Noise.Host\Noise.Host.csproj --runtime win-x64 --output ./build --self-contained true --configuration Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

MKDIR bin

COPY build\Noise.Host.exe bin\noise.exe

RMDIR /s /q build

RMDIR /s /q Noise

RMDIR /s /q .git

DEL /q .gitignore

DEL /q build.cmd