#!/bin/sh
git reset --hard
git gc
git pull
chmod a+x update.sh
dotnet publish -c Release
cd bin/Release/net6.0
pm2 stop "dotnet qcs-whitelist"
pm2 delete "dotnet qcs-whitelist"
pm2 start "dotnet qcs-whitelist.dll"