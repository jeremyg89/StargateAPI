Remove-Item -Recurse -Force .\StargateAPI.Tests\TestResults -ErrorAction SilentlyContinue
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
reportgenerator -reports:"StargateAPI.Tests/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html