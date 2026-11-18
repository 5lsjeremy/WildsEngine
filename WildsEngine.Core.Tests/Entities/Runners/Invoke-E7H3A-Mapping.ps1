$ErrorActionPreference="Stop"
Set-StrictMode -Version Latest
$RunnersRoot=$PSScriptRoot;$EntitiesRoot=Split-Path -Parent $RunnersRoot;$TestsRoot=Split-Path -Parent $EntitiesRoot;$RepoRoot=Split-Path -Parent $TestsRoot
$Project=Join-Path $TestsRoot "WildsEngine.Core.Tests.csproj";$Reports=Join-Path $EntitiesRoot "Reports";$Stamp=Get-Date -Format "yyyyMMdd-HHmmss";$Report=Join-Path $Reports "E7-H3A-Mapping-Certification-$Stamp.txt";$Latest=Join-Path $Reports "E7-H3A-Mapping-Certification-Latest.txt"
New-Item -ItemType Directory -Path $Reports -Force|Out-Null
function W([string]$t=""){Write-Host $t;$t|Out-File $Report -Append -Encoding utf8}
function Run([string]$n,[scriptblock]$c){W "";W $n;&$c 2>&1|ForEach-Object{W ([string]$_)};if($LASTEXITCODE -ne 0){throw "$n failed."}}
$core=@(git -C $RepoRoot status --short -- .\WildsEngine.Core);if($core.Count -gt 0){throw "Core has changes."};W "WILDS DISCOVERIES E7-H3A CLUSTER MAPPING CERTIFICATION";W "Core guard: PASS"
Run "RELEASE BUILD" {dotnet build $Project -c Release}
Run "ASSERTION TESTS" {dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryClusterMappingCertificationTests"}
Run "READABLE TEST" {dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryClusterMappingReadableTests" --logger "console;verbosity=detailed"}
$core=@(git -C $RepoRoot status --short -- .\WildsEngine.Core);if($core.Count -gt 0){throw "Core changed."};W "RESULT: PASS";W "Core unchanged. Consumer cluster mapping endpoint and registry certified.";Copy-Item $Report $Latest -Force;Write-Host "Latest: $Latest"
