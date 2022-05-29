echo off
set version=%1
set p=bin/Release/Walgelijk.%1%.nupkg

echo Uploading %version% to %p%

dotnet nuget push -s http://78.141.216.223:5000/v3/index.json -k "y63K&Zh4e" %p%