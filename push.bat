@echo off
set KEY=%1

if [%KEY%]==[] goto fail

dotnet clean -c Release
dotnet restore
dotnet build -c Release
dotnet pack -c Release --no-build

dotnet nuget push Walgelijk\bin\Release\*.nupkg --api-key %KEY% --source "https://nuget.pkg.github.com/Walgelijk/index.json" --skip-duplicate
dotnet nuget push Walgelijk.OpenTK\bin\Release\*.nupkg --api-key %KEY% --source "https://nuget.pkg.github.com/Walgelijk.OpenTK/index.json" --skip-duplicate
dotnet nuget push Walgelijk.SimpleDrawing\bin\Release\*.nupkg --api-key %KEY% --source "https://nuget.pkg.github.com/Walgelijk.SimpleDrawing/index.json" --skip-duplicate
dotnet nuget push Walgelijk.Imgui\bin\Release\*.nupkg --api-key %KEY% --source "https://nuget.pkg.github.com/Walgelijk.Imgui/index.json" --skip-duplicate
dotnet nuget push Walgelijk.Physics\bin\Release\*.nupkg --api-key %KEY% --source "https://nuget.pkg.github.com/Walgelijk.Physics/index.json" --skip-duplicate
dotnet nuget push Walgelijk.ParticleSystem\bin\Release\*.nupkg --api-key %KEY% --source "https://nuget.pkg.github.com/Walgelijk.ParticleSystem/index.json" --skip-duplicate

exit /B 0

:fail
@echo No key given