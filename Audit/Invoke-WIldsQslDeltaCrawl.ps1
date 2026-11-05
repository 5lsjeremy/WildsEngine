$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# ------------------------------------------------------------
# Resolve repository root from this script's location.
#
# Expected:
#   <WildsEngine>\Audit\Invoke-WildsQslDeltaCrawl.ps1
# ------------------------------------------------------------

$ScriptDirectory =
Split-Path -Parent $MyInvocation.MyCommand.Path

$Root =
Split-Path -Parent $ScriptDirectory

$Solution =
Join-Path $Root "WildsEngine.sln"

if (-not (Test-Path -LiteralPath $Solution))
{
    throw "Wilds repository root could not be resolved: $Root"
}

$Stamp =
Get-Date -Format "yyyyMMdd-HHmmss"

$AuditDirectory =
$ScriptDirectory

$TimestampedReport =
Join-Path $AuditDirectory `
        "Wilds-Qsl-Delta-$Stamp.txt"

$LatestReport =
Join-Path $AuditDirectory `
        "Wilds-Qsl-Delta-Latest.txt"

$builder =
New-Object System.Text.StringBuilder

function Write-ReportLine
{
    param(
        [string]$Text = ""
    )

    [void]$builder.AppendLine($Text)
}

function Write-Section
{
    param(
        [string]$Title
    )

    Write-ReportLine ""
    Write-ReportLine "============================================================"
    Write-ReportLine $Title
    Write-ReportLine "============================================================"
}

function Add-File
{
    param(
        [string]$RelativePath
    )

    $Path =
    Join-Path $Root $RelativePath

    Write-ReportLine ""
    Write-ReportLine "############################################################"
    Write-ReportLine "FILE: $RelativePath"
    Write-ReportLine "############################################################"

    if (-not (Test-Path -LiteralPath $Path))
    {
        Write-ReportLine "<missing>"
        return
    }

    $lineNumber =
    0

    Get-Content -LiteralPath $Path |
            ForEach-Object {
                $lineNumber++

                Write-ReportLine (
                "{0,5}: {1}" -f
                $lineNumber,
                $_
                )
            }
}

Write-Host ""
Write-Host "Wilds root:"
Write-Host $Root
Write-Host ""

Write-Section "WILDS / QSL DELTA CRAWL"

Write-ReportLine "Root: $Root"
Write-ReportLine "Generated: $(Get-Date -Format o)"

# ------------------------------------------------------------
# Git status
# ------------------------------------------------------------

Write-Section "GIT STATUS"

Push-Location $Root

try
{
    $gitStatus =
    git status --short 2>&1

    if ($gitStatus)
    {
        foreach ($line in $gitStatus)
        {
            Write-ReportLine $line
        }
    }
    else
    {
        Write-ReportLine "<clean>"
    }
}
finally
{
    Pop-Location
}

# ------------------------------------------------------------
# Project files
# ------------------------------------------------------------

Write-Section "PROJECT FILES"

Add-File "WildsEngine.Core\WildsEngine.Core.csproj"
Add-File "WildsEngine.Qsl\WildsEngine.Qsl.csproj"
Add-File "WildsEngine.Core.Tests\WildsEngine.Core.Tests.csproj"

# ------------------------------------------------------------
# Wilds execution contract
# ------------------------------------------------------------

Write-Section "WILDS EXECUTION CONTRACT"

Add-File "WildsEngine.Core\Engines\WildsEngine.cs"

Add-File "WildsEngine.Core\Abstractions\Units\IBaseUnit.cs"
Add-File "WildsEngine.Core\Abstractions\Units\IPulse.cs"
Add-File "WildsEngine.Core\Abstractions\Units\IWorkUnit.cs"
Add-File "WildsEngine.Core\Abstractions\Units\IMoment.cs"
Add-File "WildsEngine.Core\Abstractions\Units\ITimeline.cs"
Add-File "WildsEngine.Core\Abstractions\Units\IEpoch.cs"

Add-File "WildsEngine.Core\Units\BaseUnit.cs"
Add-File "WildsEngine.Core\Units\Pulse.cs"
Add-File "WildsEngine.Core\Units\WorkUnit.cs"
Add-File "WildsEngine.Core\Units\Moment.cs"
Add-File "WildsEngine.Core\Units\Timeline.cs"
Add-File "WildsEngine.Core\Units\Epoch.cs"

Add-File "WildsEngine.Core\Processing\Coordinator\PulseCoordinator.cs"

# ------------------------------------------------------------
# Wilds / QSL adapter
# ------------------------------------------------------------

Write-Section "CURRENT WILDS QSL ADAPTER"

Add-File "WildsEngine.Qsl\WildsQslExecutor.cs"
Add-File "WildsEngine.Qsl\WildsQslExecutionException.cs"
Add-File "WildsEngine.Qsl\WildsQslRequest.cs"

# ------------------------------------------------------------
# Certification
# ------------------------------------------------------------

Write-Section "CURRENT CERTIFICATION ENTRY POINTS"

Add-File "WildsEngine.Core.Tests\CertificationTests.cs"
Add-File "WildsEngine.Core.Tests\WildsEngineExecuteIntegrationTests.cs"

Add-File "WildsEngine.Core.Tests\Certification\WildsQslCertification.cs"
Add-File "WildsEngine.Core.Tests\Certification\WildsCertificationStatus.cs"

# ------------------------------------------------------------
# Test support
# ------------------------------------------------------------

Write-Section "TEST SUPPORT"

Add-File "WildsEngine.Core.Tests\TestComponent.cs"
Add-File "WildsEngine.Core.Tests\TestContext.cs"
Add-File "WildsEngine.Core.Tests\TestState.cs"
Add-File "WildsEngine.Core.Tests\TestSignal.cs"
Add-File "WildsEngine.Core.Tests\TestLog.cs"

# ------------------------------------------------------------
# Symbol search
# ------------------------------------------------------------

Write-Section "QSL / WILDS SYMBOL SEARCH"

$patterns = @(
    "WildsQslExecutor",
    "WildsQslRequest",
    "QslEngine",
    "QslProcessingResult",
    "QslConsumerBinding",
    "QslStatePackage",
    "BindConsumer",
    "ReceiveState",
    "ExecuteAsync",
    "Pulse",
    "WorkUnit",
    "Moment"
)

$sourceRoots = @(
    (Join-Path $Root "WildsEngine.Core"),
    (Join-Path $Root "WildsEngine.Qsl"),
    (Join-Path $Root "WildsEngine.Core.Tests")
)

foreach ($pattern in $patterns)
{
    Write-ReportLine ""
    Write-ReportLine "------------------------------------------------------------"
    Write-ReportLine "PATTERN: $pattern"
    Write-ReportLine "------------------------------------------------------------"

    $matches =
    @()

    foreach ($sourceRoot in $sourceRoots)
    {
        if (-not (Test-Path -LiteralPath $sourceRoot))
        {
            continue
        }

        $matches +=
        Get-ChildItem `
                -LiteralPath $sourceRoot `
                -Recurse `
                -File `
                -Filter "*.cs" |
                Where-Object {
                    $_.FullName -notmatch "\\bin\\" -and
                            $_.FullName -notmatch "\\obj\\"
                } |
                Select-String `
                -SimpleMatch `
                -Pattern $pattern
    }

    if ($matches.Count -eq 0)
    {
        Write-ReportLine "<no matches>"
        continue
    }

    foreach ($match in $matches)
    {
        $relative =
        $match.Path.Substring(
                $Root.Length
        ).TrimStart(
                "\"
        )

        Write-ReportLine (
        "{0}:{1}: {2}" -f
        $relative,
        $match.LineNumber,
        $match.Line.Trim()
        )
    }
}

# ------------------------------------------------------------
# Focused source inventory
# ------------------------------------------------------------

Write-Section "SOURCE INVENTORY"

foreach ($folder in @(
    "WildsEngine.Qsl",
    "WildsEngine.Core.Tests\Certification"
))
{
    Write-ReportLine ""
    Write-ReportLine $folder
    Write-ReportLine "------------------------------------------------------------"

    $path =
    Join-Path $Root $folder

    if (-not (Test-Path -LiteralPath $path))
    {
        Write-ReportLine "<missing>"
        continue
    }

    Get-ChildItem `
        -LiteralPath $path `
        -Recurse `
        -File |
            Where-Object {
                $_.FullName -notmatch "\\bin\\" -and
                        $_.FullName -notmatch "\\obj\\"
            } |
            Sort-Object FullName |
            ForEach-Object {
                Write-ReportLine (
                $_.FullName.Substring(
                        $Root.Length
                ).TrimStart(
                        "\"
                )
                )
            }
}

# ------------------------------------------------------------
# Write reports
# ------------------------------------------------------------

$builder.ToString() |
        Set-Content `
        -LiteralPath $TimestampedReport `
        -Encoding UTF8

Copy-Item `
    -LiteralPath $TimestampedReport `
    -Destination $LatestReport `
    -Force

Write-Host ""
Write-Host "Wilds / QSL delta crawl complete."
Write-Host ""
Write-Host "Timestamped:"
Write-Host $TimestampedReport
Write-Host ""
Write-Host "Latest:"
Write-Host $LatestReport
Write-Host ""