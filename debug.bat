@echo off
dotnet build src/Skybrud.Umbraco.Feedback --configuration Debug /t:rebuild /t:pack -p:BuildTools=1 -p:PackageOutputPath=c:/nuget