param([string]$RepoRoot)
$ErrorActionPreference="Stop"
Set-StrictMode -Version Latest
$PackageRoot=$PSScriptRoot
if([string]::IsNullOrWhiteSpace($RepoRoot)){$RepoRoot=Split-Path -Parent $PackageRoot}
$RepoRoot=(Resolve-Path $RepoRoot).Path;$Payload=Join-Path $PackageRoot "Payload"
if(-not(Test-Path (Join-Path $RepoRoot "WildsEngine.sln"))){throw "WildsEngine.sln not found."}
$core=@(git -C $RepoRoot status --short -- .\WildsEngine.Core);if($core.Count -gt 0){throw "Core has changes."}
$required=@("WildsEngine.Discoveries\Services\WildsServiceBase.cs","WildsEngine.Discoveries\Motion\FiniteDifferenceDiscoveryVectorMotionService.cs")
foreach($r in $required){if(-not(Test-Path (Join-Path $RepoRoot $r))){throw "Required foundation missing: $r"}}
Get-ChildItem $Payload -Recurse -File|ForEach-Object{$rel=$_.FullName.Substring($Payload.Length).TrimStart('\','/');$dest=Join-Path $RepoRoot $rel;New-Item -ItemType Directory -Path (Split-Path -Parent $dest) -Force|Out-Null;Copy-Item $_.FullName $dest -Force;Write-Host "[INSTALL] $rel" -ForegroundColor Green}
dotnet build (Join-Path $RepoRoot "WildsEngine.Core.Tests\WildsEngine.Core.Tests.csproj") -c Release
if($LASTEXITCODE -ne 0){throw "Release build failed."}
$core=@(git -C $RepoRoot status --short -- .\WildsEngine.Core);if($core.Count -gt 0){throw "Core changed."}
Write-Host "H3A mapping endpoint installed. Run .\WildsEngine.Core.Tests\Entities\Runners\Invoke-E7H3A-Mapping.ps1" -ForegroundColor Green
