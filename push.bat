@echo off
set KEY=%1

if [%KEY%]==[] goto fail

dotnet clean -c Release
dotnet restore
dotnet build -c Release
dotnet test --no-build --verbosity normal -c Release
dotnet pack -c Release --no-build

dotnet nuget push Walgelijk\bin\Release\*.nupkg --api-key %KEY% --source "https://api.nuget.org/v3/index.json" --skip-duplicate
dotnet nuget push Walgelijk.OpenTK\bin\Release\*.nupkg --api-key %KEY% --source "https://api.nuget.org/v3/index.json" --skip-duplicate
dotnet nuget push Walgelijk.SimpleDrawing\bin\Release\*.nupkg --api-key %KEY% --source "https://api.nuget.org/v3/index.json" --skip-duplicate
dotnet nuget push Walgelijk.Imgui\bin\Release\*.nupkg --api-key %KEY% --source "https://api.nuget.org/v3/index.json" --skip-duplicate
dotnet nuget push Walgelijk.Physics\bin\Release\*.nupkg --api-key %KEY% --source "https://api.nuget.org/v3/index.json" --skip-duplicate
dotnet nuget push Walgelijk.ParticleSystem\bin\Release\*.nupkg --api-key %KEY% --source "https://api.nuget.org/v3/index.json" --skip-duplicate

exit /B 0

:fail
@echo No key given
exit /B 1