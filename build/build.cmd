@echo off

echo Building CsvSerializer

dotnet build --configuration Release ..\srcCsvSerializer

nuget pack ..\src\CsvSerializer\CsvSerializer.nuspec -OutputDirectory ..\releases\

echo Building CsvSerializer for .NET Core



