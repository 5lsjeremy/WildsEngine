param(
    [Parameter(Mandatory = $true)]
    [string]$Gate,

    [Parameter(Mandatory = $true)]
    [string]$Title,

    [Parameter(Mandatory = $true)]
    [string]$AssertionFilter,

    [Parameter(Mandatory = $true)]
    [string]$ReadableFilter
)

$ErrorActionPreference = "Stop"

# ------------------------------------------------------------
# Resolve project paths from the runner location.
#
# WildsEngine.Core.Tests
#   \ Entities
#       \ Runners
#           \ Invoke-E7Gate.ps1
# ------------------------------------------------------------

$EntitiesRoot =
Split-Path `
        $PSScriptRoot `
        -Parent

$ProjectRoot =
Split-Path `
        $EntitiesRoot `
        -Parent

$Project =
Join-Path `
        $ProjectRoot `
        "WildsEngine.Core.Tests.csproj"

$ReportRoot =
Join-Path `
        $EntitiesRoot `
        "Reports"

$Timestamp =
Get-Date -Format "yyyyMMdd-HHmmss"

$Report =
Join-Path `
        $ReportRoot `
        "$Gate-Certification-$Timestamp.txt"

$Latest =
Join-Path `
        $ReportRoot `
        "$Gate-Certification-Latest.txt"

New-Item `
    -ItemType Directory `
    -Path $ReportRoot `
    -Force |
        Out-Null

# Explicit UTF-8 without BOM.
$Utf8 =
New-Object `
        System.Text.UTF8Encoding(
$false)

[System.IO.File]::WriteAllText(
        $Report,
        "",
        $Utf8)

function Write-Report
{
    param(
        [string]$Text = ""
    )

    Write-Host $Text

    [System.IO.File]::AppendAllText(
            $Report,
            $Text + [Environment]::NewLine,
            $Utf8)
}

function Write-ReportHeader
{
    param(
        [string]$Text
    )

    Write-Report ""
    Write-Report "============================================================"
    Write-Report $Text
    Write-Report "============================================================"
}

function Invoke-LoggedCommand
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$StepName,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Command
    )

    Write-ReportHeader $StepName

    $Output =
    & $Command 2>&1

    $ExitCode =
    $LASTEXITCODE

    foreach ($Line in $Output)
    {
        Write-Report(
        $Line.ToString())
    }

    Write-Report ""

    if ($ExitCode -ne 0)
    {
        Write-Report "[FAIL] $StepName"
        Write-Report "[FAIL] Exit code = $ExitCode"

        throw "$StepName failed with exit code $ExitCode."
    }

    Write-Report "[PASS] $StepName"
}

$Succeeded =
$false

$FailureMessage =
$null

try
{
    Write-ReportHeader "WILDS ENTITY $Gate CERTIFICATION"

    Write-Report "Gate       : $Gate"
    Write-Report "Title      : $Title"
    Write-Report "Project    : $Project"
    Write-Report "Started    : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    Write-Report ""

    Write-Report "Assertion filter:"
    Write-Report "  $AssertionFilter"
    Write-Report ""

    Write-Report "Readable filter:"
    Write-Report "  $ReadableFilter"

    Invoke-LoggedCommand `
        "$Gate RELEASE BUILD" `
        {
        dotnet build `
                $Project `
                -c Release
    }

    Invoke-LoggedCommand `
        "$Gate ASSERTION CERTIFICATION" `
        {
        dotnet test `
                $Project `
                -c Release `
                --no-build `
                --filter $AssertionFilter `
                --logger "console;verbosity=normal"
    }

    Invoke-LoggedCommand `
        "$Gate READABLE CERTIFICATION" `
        {
        dotnet test `
                $Project `
                -c Release `
                --no-build `
                --filter $ReadableFilter `
                --logger "console;verbosity=detailed"
    }

    $Succeeded =
    $true
}
catch
{
    $FailureMessage =
    $_.Exception.Message
}

Write-ReportHeader "$Gate CERTIFICATION SUMMARY"

Write-Report "Gate  : $Gate"
Write-Report "Title : $Title"
Write-Report ""

if ($Succeeded)
{
    Write-Report "[PASS] Release build"
    Write-Report "[PASS] Assertion certification"
    Write-Report "[PASS] Readable certification"
    Write-Report ""
    Write-Report "$Gate CERTIFIED"
}
else
{
    Write-Report "[FAIL] $Gate certification"
    Write-Report ""
    Write-Report "Failure:"
    Write-Report "  $FailureMessage"
}

Write-Report ""
Write-Report "Completed:"
Write-Report "  $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"

Copy-Item `
    -Path $Report `
    -Destination $Latest `
    -Force

Write-Host ""
Write-Host "============================================================"
Write-Host " $Gate CERTIFICATION ARTIFACT"
Write-Host "============================================================"
Write-Host "Report:"
Write-Host "  $Report"
Write-Host ""
Write-Host "Latest:"
Write-Host "  $Latest"
Write-Host "============================================================"

if (-not $Succeeded)
{
    exit 1
}

exit 0