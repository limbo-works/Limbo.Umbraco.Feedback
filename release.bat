@echo off
dotnet build src/Limbo.Umbraco.Feedback --configuration Release /t:rebuild /t:pack -p:PackageOutputPath=../../releases/nuget