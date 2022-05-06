@echo off

REM Noise peer host installation script

REM Latest version build and binary preparation installation in temp directory 
dotnet publish .\Noise\Noise.Host\Noise.Host.csproj --runtime win-x64 --output ./build --self-contained true --configuration Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

MKDIR bin
COPY build\Noise.Host.exe bin\noise.exe

REM Removing usused files
RMDIR /s /q build
RMDIR /s /q Noise
RMDIR /s /q resources
RMDIR /s /q .git
RMDIR /q .gitignore
DEL /q build.sh
DEL /q patch.sh
DEL /q build.cmd