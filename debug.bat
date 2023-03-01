@echo off
dotnet build src/Limbo.Umbraco.Feedback --configuration Debug /t:rebuild /t:pack -p:PackageOutputPath=c:/nuget