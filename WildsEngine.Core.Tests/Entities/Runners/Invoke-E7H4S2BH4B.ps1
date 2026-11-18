$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$Runners = $PSScriptRoot
$Entities = Split-Path -Parent $Runners
$Tests = Split-Path -Parent $Entities
$Repo = Split-Path -Parent $Tests
$Project = Join-Path $Tests "WildsEngine.Core.Tests.csproj"
if (@(git -C $Repo status --short -- .\WildsEngine.Core).Count -gt 0) { throw "Core has changes." }
dotnet build $Project -c Release
if ($LASTEXITCODE -ne 0) { throw "Release build failed." }
dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryNarrativeCompositionCertificationTests" --logger "console;verbosity=detailed"
if ($LASTEXITCODE -ne 0) { throw "H4S2B/H4B tests failed." }
if (@(git -C $Repo status --short -- .\WildsEngine.Core).Count -gt 0) { throw "Core changed." }
Write-Host "H4S2B + H4B PASS" -ForegroundColor Green
