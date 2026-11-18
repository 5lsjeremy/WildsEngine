param([string]$RepoRoot)
$ErrorActionPreference="Stop"
Set-StrictMode -Version Latest
$PackageRoot=$PSScriptRoot
if([string]::IsNullOrWhiteSpace($RepoRoot)){$RepoRoot=Split-Path -Parent $PackageRoot}
$RepoRoot=(Resolve-Path $RepoRoot).Path
$Payload=Join-Path $PackageRoot "Payload"
if(-not(Test-Path (Join-Path $RepoRoot "WildsEngine.sln"))){throw "RepoRoot does not contain WildsEngine.sln: $RepoRoot"}
$coreBefore=@(git -C $RepoRoot status --short -- .\WildsEngine.Core)
if($coreBefore.Count -gt 0){$coreBefore|ForEach-Object{Write-Host $_};throw "WildsEngine.Core has changes. Install stopped."}
$required=@(
 "WildsEngine.Discoveries\Abstractions\Services\IWildsService.cs",
 "WildsEngine.Discoveries\Abstractions\Vectorization\IDiscoveryVectorizer.cs",
 "WildsEngine.Discoveries\Services\WildsServiceBase.cs",
 "WildsEngine.Discoveries\Vectorization\DiscoveryVectorizer.cs"
)
foreach($r in $required){if(-not(Test-Path (Join-Path $RepoRoot $r))){throw "Required H1 foundation missing: $r"}}
Get-ChildItem $Payload -Recurse -File|ForEach-Object{
 $relative=$_.FullName.Substring($Payload.Length).TrimStart('\','/');$target=Join-Path $RepoRoot $relative;$dir=Split-Path -Parent $target;New-Item -ItemType Directory -Path $dir -Force|Out-Null;Copy-Item $_.FullName $target -Force;Write-Host "[INSTALL] $relative" -ForegroundColor Green
}
Write-Host "Building Release..." -ForegroundColor Cyan
dotnet build (Join-Path $RepoRoot "WildsEngine.Core.Tests\WildsEngine.Core.Tests.csproj") -c Release
if($LASTEXITCODE -ne 0){throw "Release build failed."}
$coreAfter=@(git -C $RepoRoot status --short -- .\WildsEngine.Core)
if($coreAfter.Count -gt 0){throw "Core changed during install."}
Write-Host "H2 installed. Run .\WildsEngine.Core.Tests\Entities\Runners\Invoke-E7H2.ps1" -ForegroundColor Green
