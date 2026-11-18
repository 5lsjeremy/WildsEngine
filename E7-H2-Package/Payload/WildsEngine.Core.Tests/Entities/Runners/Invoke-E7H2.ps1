$ErrorActionPreference="Stop"
Set-StrictMode -Version Latest
$RunnersRoot=$PSScriptRoot;$EntitiesRoot=Split-Path -Parent $RunnersRoot;$TestsRoot=Split-Path -Parent $EntitiesRoot;$RepoRoot=Split-Path -Parent $TestsRoot
$Project=Join-Path $TestsRoot "WildsEngine.Core.Tests.csproj";$ReportDir=Join-Path $EntitiesRoot "Reports";$Stamp=Get-Date -Format "yyyyMMdd-HHmmss";$Report=Join-Path $ReportDir "E7-H2-Certification-$Stamp.txt";$Latest=Join-Path $ReportDir "E7-H2-Certification-Latest.txt"
New-Item -ItemType Directory -Path $ReportDir -Force|Out-Null
function W([string]$t=""){Write-Host $t;$t|Out-File $Report -Append -Encoding utf8}
function Run([string]$n,[scriptblock]$c){W "";W "============================================================";W $n;W "============================================================";&$c 2>&1|ForEach-Object{W ([string]$_)};if($LASTEXITCODE -ne 0){throw "$n failed."}}
W "WILDS DISCOVERIES E7-H2 A/B/C CERTIFICATION";$core=@(git -C $RepoRoot status --short -- .\WildsEngine.Core);if($core.Count -gt 0){throw "Core has changes."};W "Core guard: PASS"
Run "E7-H2 RELEASE BUILD" {dotnet build $Project -c Release}
Run "E7-H2A SERIES TESTS" {dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryVectorSeriesCertificationTests"}
Run "E7-H2B MOTION TESTS" {dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryVectorMotionServiceCertificationTests"}
Run "E7-H2C CURVE TESTS" {dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryVectorMomentumCurveCertificationTests"}
Run "E7-H2 READABLE TEST" {dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryVectorMotionReadableTests" --logger "console;verbosity=detailed"}
$core=@(git -C $RepoRoot status --short -- .\WildsEngine.Core);if($core.Count -gt 0){throw "Core changed."};W "";W "RESULT: PASS";W "Core unchanged. H2A series, H2B raw motion, and H2C curve-adjusted momentum certified.";Copy-Item $Report $Latest -Force;Write-Host "Latest: $Latest"
