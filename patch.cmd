@echo off

REM Noise peer host patching script

REM Backup local binary and peer data
MOVE bin bin-pre

REM Fetching the latest version
MKDIR patch
git clone --branch main https://github.com/Krzysztofz01/Noise.git patch

REM Latest version build and binary preparation installation in temp directory 
dotnet publish patch\Noise\Noise.Host\Noise.Host.csproj --runtime win-x64 --output patch/build --self-contained true --configuration Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

MKDIR patch\bin
COPY patch\build\Noise.Host.exe patch\bin\noise.exe

REM Removing usused files
RMDIR /s /q patch\build
RMDIR /s /q patch\Noise
RMDIR /s /q patch\resources
RMDIR /s /q patch\.git
DEL /q patch\.gitignore
DEL /q patch\build.sh
DEL /q patch\patch.sh
DEL /q patch\build.cmd

REM Removing local version files
DEL /q LICENSE
DEL /q NOTICES
DEL /q README.md
DEL /q patch.sh

REM Replacing the latest version files and peer data transfer
MOVE patch\* .
MOVE patch\bin .
MOVE bin-pre\peer.noise bin\peer.noise

REM Installation cleanup
RMDIR /s /q patch
RMDIR /s /q bin-pre