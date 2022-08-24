#!/bin/sh
mkdir -p build
# Linux
dotnet publish -c Release -r linux-x64 --sc
cp bin/Release/net6.0/linux-x64/publish/backupex build/backupex-linux-x64
# Windows
dotnet publish -c Release -r win-x64 --sc false -p:EnableCompressionInSingleFile=false
cp bin/Release/net6.0/win-x64/publish/backupex.exe build/backupex-win-x64.exe