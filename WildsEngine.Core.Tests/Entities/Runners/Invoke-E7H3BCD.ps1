$ErrorActionPreference="Stop"
Set-StrictMode -Version Latest
$Runners=$PSScriptRoot;$Entities=Split-Path -Parent $Runners;$Tests=Split-Path -Parent $Entities;$Repo=Split-Path -Parent $Tests;$Project=Join-Path $Tests "WildsEngine.Core.Tests.csproj";$Reports=Join-Path $Entities "Reports";$Stamp=Get-Date -Format "yyyyMMdd-HHmmss";$Report=Join-Path $Reports "E7-H3BCD-Certification-$Stamp.txt";$Latest=Join-Path $Reports "E7-H3BCD-Certification-Latest.txt"
New-Item -ItemType Directory -Path $Reports -Force|Out-Null
function W([string]$x=""){Write-Host $x;$x|Out-File $Report -Append -Encoding utf8}
function Run([string]$n,[scriptblock]$c){W "";W $n;&$c 2>&1|ForEach-Object{W ([string]$_)};if($LASTEXITCODE -ne 0){throw "$n failed."}}
if(@(git -C $Repo status --short -- .\WildsEngine.Core).Count -gt 0){throw "Core has changes."};W "WILDS DISCOVERIES E7-H3B/C/D CERTIFICATION";W "Core guard: PASS"
Run "RELEASE BUILD" {dotnet build $Project -c Release}
Run "H3B SPATIAL TESTS" {dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryClusterRelationshipCertificationTests"}
Run "H3C/D TRAJECTORY AND TEMPORAL TESTS" {dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryClusterTrajectoryCertificationTests"}
Run "READABLE TEST" {dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~DiscoveryClusterRelationshipReadableTests" --logger "console;verbosity=detailed"}
if(@(git -C $Repo status --short -- .\WildsEngine.Core).Count -gt 0){throw "Core changed."};W "RESULT: PASS";W "Core unchanged. H3B spatial relationship, H3C trajectory intersection, and H3D projected occupancy certified.";Copy-Item $Report $Latest -Force;Write-Host "Latest: $Latest"
