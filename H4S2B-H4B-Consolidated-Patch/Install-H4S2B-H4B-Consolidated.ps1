param([string]$RepoRoot)
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$Package = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($RepoRoot)) { $RepoRoot = Split-Path -Parent $Package }
$RepoRoot = (Resolve-Path $RepoRoot).Path
$Payload = Join-Path $Package "Payload"
if (-not (Test-Path (Join-Path $RepoRoot "WildsEngine.sln"))) { throw "WildsEngine.sln not found." }
if (@(git -C $RepoRoot status --short -- .\WildsEngine.Core).Count -gt 0) { throw "Core has changes." }
foreach ($required in @(
    "WildsEngine.Discoveries\Abstractions\Narratives\IDiscoveryRecognizedGeometryPopulation.cs",
    "WildsEngine.Discoveries\Surface\DiscoveryNarrativeSurface.cs",
    "WildsEngine.Discoveries\Surface\WildsDiscoveries.cs")) {
    if (-not (Test-Path (Join-Path $RepoRoot $required))) { throw "Required H4A foundation missing: $required" }
}
Get-ChildItem $Payload -Recurse -File | ForEach-Object {
    $relative = $_.FullName.Substring($Payload.Length).TrimStart('\','/')
    $destination = Join-Path $RepoRoot $relative
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item $_.FullName $destination -Force
    Write-Host "[INSTALL] $relative" -ForegroundColor Green
}
dotnet build (Join-Path $RepoRoot "WildsEngine.Core.Tests\WildsEngine.Core.Tests.csproj") -c Release
if ($LASTEXITCODE -ne 0) { throw "Release build failed." }
if (@(git -C $RepoRoot status --short -- .\WildsEngine.Core).Count -gt 0) { throw "Core changed." }
Write-Host "Consolidated H4S2B/H4B patch installed." -ForegroundColor Green
