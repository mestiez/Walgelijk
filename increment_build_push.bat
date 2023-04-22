@echo off
echo This will increment the versions of the changed projects, build and pack them, and then push them to the package repository. Are you sure you want to continue? (y/n)?
set /p c=
if /i "%c%" EQU "y" goto :yes
if /i "%c%" EQU "n" goto :no

:yes
call dotnet tool install --global dotnet-script

echo ### Incrementing version...
dotnet script increment_version.csx

echo ### Building and packaging...
dotnet script clean_build_and_package.csx

echo ### Pushing...
dotnet script push_packages.csx

:no
@REM niks