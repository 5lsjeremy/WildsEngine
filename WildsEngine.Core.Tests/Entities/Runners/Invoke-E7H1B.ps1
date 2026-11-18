$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RunnersRoot = $PSScriptRoot
$EntitiesRoot = Split-Path -Parent $RunnersRoot
$TestsRoot = Split-Path -Parent $EntitiesRoot
$RepoRoot = Split-Path -Parent $TestsRoot
$Project = Join-Path $TestsRoot "WildsEngine.Core.Tests.csproj"
$ReportDir = Join-Path $EntitiesRoot "Reports"
$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$Report = Join-Path $ReportDir "E7-H1B-Vectorization-Certification-$Timestamp.txt"
$Latest = Join-Path $ReportDir "E7-H1B-Vectorization-Certification-Latest.txt"

New-Item -ItemType Directory -Path $ReportDir -Force | Out-Null

function Write-Line([string]$Text = "")
{
    Write-Host $Text
    $Text | Out-File -LiteralPath $Report -Append -Encoding utf8
}

function Invoke-Logged([string]$Title, [scriptblock]$Command)
{
    Write-Line ""
    Write-Line "============================================================"
    Write-Line $Title
    Write-Line "============================================================"

    & $Command 2>&1 |
        ForEach-Object { Write-Line ([string]$_) }

    if ($LASTEXITCODE -ne 0)
    {
        throw "$Title failed with exit code $LASTEXITCODE."
    }
}

Write-Line "============================================================"
Write-Line "WILDS DISCOVERIES E7-H1B VECTORIZATION CERTIFICATION"
Write-Line "============================================================"
Write-Line "Started : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"

$CoreChanges = @(git -C $RepoRoot status --short -- .\WildsEngine.Core)

if ($CoreChanges.Count -gt 0)
{
    $CoreChanges | ForEach-Object { Write-Line $_ }
    throw "Core has changes. H1B will not proceed."
}

Write-Line "Core guard: PASS"

Invoke-Logged "E7-H1B RELEASE BUILD" {
    dotnet build $Project -c Release
}

Invoke-Logged "E7-H1B ASSERTION TESTS" {
    dotnet test $Project `
        -c Release `
        --no-build `
        --filter "FullyQualifiedName~DiscoveryVectorizationServiceCertificationTests"
}

Invoke-Logged "E7-H1B READABLE TESTS" {
    dotnet test $Project `
        -c Release `
        --no-build `
        --filter "FullyQualifiedName~DiscoveryVectorizationServiceReadableTests" `
        --logger "console;verbosity=detailed"
}

$CoreChangesAfter = @(git -C $RepoRoot status --short -- .\WildsEngine.Core)

if ($CoreChangesAfter.Count -gt 0)
{
    $CoreChangesAfter | ForEach-Object { Write-Line $_ }
    throw "Core changed during H1B."
}

Write-Line ""
Write-Line "RESULT: PASS"
Write-Line "Core unchanged. Async vectorization certified."

Copy-Item -LiteralPath $Report -Destination $Latest -Force

Write-Host "Report : $Report"
Write-Host "Latest : $Latest"
