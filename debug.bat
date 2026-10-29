@echo off

REM Install dependencies and build client
pushd "%~dp0src\Limbo.Umbraco.Feedback\client"

call npm install
if errorlevel 1 exit /b 1

call npm run build
if errorlevel 1 exit /b 1

popd

REM Build and pack .NET project
dotnet build src/Limbo.Umbraco.Feedback --configuration Debug /t:rebuild /t:pack -p:PackageOutputPath=c:\nuget\Umbraco17