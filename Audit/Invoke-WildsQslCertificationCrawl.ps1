[CmdletBinding()]
param(
    [string] $Root = (Get-Location).Path,

    [string] $QslRoot = "C:\Users\jmatt\source\QSL",

    [switch] $RunBuild
)

$ErrorActionPreference = "Stop"

# ============================================================
# Paths
# ============================================================

$Root = (Resolve-Path $Root).Path

$AuditRoot =
Join-Path `
        $Root `
        "Audit"

if (-not (Test-Path $AuditRoot))
{
    New-Item `
        -ItemType Directory `
        -Path $AuditRoot `
        -Force |
            Out-Null
}

$Timestamp =
Get-Date `
        -Format "yyyyMMdd-HHmmss"

$ReportPath =
Join-Path `
        $AuditRoot `
        "Wilds-Qsl-Certification-Context-$Timestamp.txt"

$LatestPath =
Join-Path `
        $AuditRoot `
        "Wilds-Qsl-Certification-Context-Latest.txt"

# ============================================================
# Helpers
# ============================================================

function Write-Line
{
    param(
        [string] $Text = ""
    )

    Add-Content `
        -LiteralPath $ReportPath `
        -Value $Text
}

function Write-Section
{
    param(
        [Parameter(Mandatory = $true)]
        [string] $Title
    )

    Write-Line
    Write-Line ("=" * 80)
    Write-Line $Title
    Write-Line ("=" * 80)
}

function Get-DisplayPath
{
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $fullPath =
    [System.IO.Path]::GetFullPath(
            $Path
    )

    $rootPath =
    $Root.TrimEnd(
            "\",
            "/"
    )

    if ($fullPath.StartsWith(
            $rootPath,
            [System.StringComparison]::OrdinalIgnoreCase))
    {
        return $fullPath.Substring(
                $rootPath.Length
        ).TrimStart(
                "\",
                "/"
        )
    }

    return $fullPath
}

function Write-SourceFile
{
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path))
    {
        return
    }

    $resolvedPath =
    (Resolve-Path -LiteralPath $Path).Path

    Write-Line
    Write-Line (
    "FILE: " +
            (Get-DisplayPath -Path $resolvedPath)
    )

    Write-Line ("-" * 80)

    Get-Content `
        -LiteralPath $resolvedPath `
        -Raw |
            Add-Content `
            -LiteralPath $ReportPath
}

function Get-SourceFiles
{
    param(
        [Parameter(Mandatory = $true)]
        [string[]] $Paths
    )

    $results =
    @()

    foreach ($path in $Paths)
    {
        if (-not (Test-Path -LiteralPath $path))
        {
            continue
        }

        $results +=
        Get-ChildItem `
                -LiteralPath $path `
                -Recurse `
                -File `
                -Filter "*.cs" |
                Where-Object {
                    $_.FullName -notmatch
                            "\\(bin|obj|\.git)\\"
                }
    }

    return $results |
            Sort-Object FullName `
            -Unique
}

# ============================================================
# Initialize report
# ============================================================

Set-Content `
    -LiteralPath $ReportPath `
    -Value "WILDS / QSL CERTIFICATION CONTEXT"

Write-Line (
"Generated: " +
        (Get-Date -Format "O")
)

Write-Line (
"Wilds root: " +
        $Root
)

Write-Line (
"PowerShell: " +
        $PSVersionTable.PSVersion
)

Write-Line (
"CLR: " +
        [Environment]::Version
)

if (Test-Path -LiteralPath $QslRoot)
{
    Write-Line (
    "QSL root: " +
            (Resolve-Path -LiteralPath $QslRoot).Path
    )
}
else
{
    Write-Line (
    "QSL root: NOT FOUND - " +
            $QslRoot
    )
}

# ============================================================
# Project inventory
# ============================================================

Write-Section `
    -Title "PROJECT INVENTORY"

$Projects =
Get-ChildItem `
        -LiteralPath $Root `
        -Recurse `
        -File `
        -Filter "*.csproj" |
        Where-Object {
            $_.FullName -notmatch
                    "\\(bin|obj|\.git)\\"
        } |
        Sort-Object FullName

foreach ($project in $Projects)
{
    Write-Line (
    Get-DisplayPath `
            -Path $project.FullName
    )
}

# ============================================================
# Project files
# ============================================================

Write-Section `
    -Title "PROJECT FILE CONTENTS"

foreach ($project in $Projects)
{
    Write-SourceFile `
        -Path $project.FullName
}

# ============================================================
# Source roots
# ============================================================

$WildsSourceRoots =
@(
    (Join-Path $Root "WildsEngine.Core"),
    (Join-Path $Root "WildsEngine.Qsl"),
    (Join-Path $Root "WildsEngine.Core.Tests"),
    (Join-Path $Root "WildsEngine.Console")
)

$SourceFiles =
Get-SourceFiles `
        -Paths $WildsSourceRoots

# ============================================================
# Relevant source inventory
# ============================================================

Write-Section `
    -Title "CURRENT EXECUTION / TEMPORAL / QSL SOURCE"

$RelevantFiles =
$SourceFiles |
        Where-Object {
            $_.Name -match
                    "WildsEngine|Qsl|Pulse|WorkUnit|Moment|Timeline|Epoch|Result|Integration|Coordinator|Temporal|Execute"
        } |
        Sort-Object FullName `
        -Unique

foreach ($file in $RelevantFiles)
{
    Write-SourceFile `
        -Path $file.FullName
}

# ============================================================
# Test files potentially exercising execution route
# ============================================================

Write-Section `
    -Title "EXECUTION AND INTEGRATION TEST SOURCE"

$TestRoot =
Join-Path `
        $Root `
        "WildsEngine.Core.Tests"

if (Test-Path -LiteralPath $TestRoot)
{
    $TestFiles =
    Get-ChildItem `
            -LiteralPath $TestRoot `
            -Recurse `
            -File `
            -Filter "*.cs" |
            Where-Object {
                $_.FullName -notmatch
                        "\\(bin|obj|\.git)\\"
            } |
            Where-Object {
                $content =
                Get-Content `
                    -LiteralPath $_.FullName `
                    -Raw

                $content -match
                        "ExecuteAsync|ExecuteInstructionAsync|Pulse|WorkUnit|Moment|QslEngine|QslProcessingResult"
            } |
            Sort-Object FullName `
            -Unique

    foreach ($file in $TestFiles)
    {
        Write-SourceFile `
            -Path $file.FullName
    }
}

# ============================================================
# Execution API search
# ============================================================

Write-Section `
    -Title "EXECUTION API MATCHES"

$Patterns =
@(
    "ExecuteAsync",
    "Execute(",
    "ExecuteInstructionAsync",
    "QslProcessingResult",
    "QslEngine",
    "Pulse",
    "WorkUnit",
    "Moment",
    "Timeline",
    "Epoch",
    "ComputedResult",
    "QslWilds",
    "WildsQsl",
    "Coordinator",
    "Resolve"
)

foreach ($pattern in $Patterns)
{
    Write-Line
    Write-Line (
    "PATTERN: " +
            $pattern
    )

    Write-Line ("-" * 80)

    $Matches =
    $SourceFiles |
            Select-String `
            -SimpleMatch `
            -Pattern $pattern

    if (-not $Matches)
    {
        Write-Line "<no matches>"
        continue
    }

    foreach ($match in $Matches)
    {
        $displayPath =
        Get-DisplayPath `
                -Path $match.Path

        Write-Line (
        "{0}:{1}: {2}" -f `
                $displayPath,
        $match.LineNumber,
        $match.Line.Trim()
        )
    }
}

# ============================================================
# QSL certification assets
# ============================================================

Write-Section `
    -Title "QSL CERTIFICATION ASSETS"

if (Test-Path -LiteralPath $QslRoot)
{
    $CertificationNames =
    @(
        "CertificationRunner.cs",
        "CertificationModels.cs",
        "qsl_statement_testing.json"
    )

    $CertificationAssets =
    Get-ChildItem `
            -LiteralPath $QslRoot `
            -Recurse `
            -File |
            Where-Object {
                $_.FullName -notmatch
                        "\\(bin|obj|\.git)\\"
            } |
            Where-Object {
                $CertificationNames -contains
                        $_.Name
            } |
            Sort-Object FullName

    if (-not $CertificationAssets)
    {
        Write-Line "<no certification assets found>"
    }
    else
    {
        foreach ($asset in $CertificationAssets)
        {
            Write-SourceFile `
                -Path $asset.FullName
        }
    }
}
else
{
    Write-Line (
    "<QSL root not found: " +
            $QslRoot +
            ">"
    )
}

# ============================================================
# JSON suite summary
# ============================================================

Write-Section `
    -Title "QSL SUITE SUMMARY"

if (Test-Path -LiteralPath $QslRoot)
{
    $Suite =
    Get-ChildItem `
            -LiteralPath $QslRoot `
            -Recurse `
            -File `
            -Filter "qsl_statement_testing.json" |
            Where-Object {
                $_.FullName -notmatch
                        "\\(bin|obj|\.git)\\"
            } |
            Select-Object `
            -First 1

    if ($Suite)
    {
        Write-Line (
        "Suite: " +
                $Suite.FullName
        )

        try
        {
            $SuiteJson =
            Get-Content `
                    -LiteralPath $Suite.FullName `
                    -Raw |
                    ConvertFrom-Json

            if ($null -ne $SuiteJson.Tests)
            {
                Write-Line (
                "Tests: " +
                        $SuiteJson.Tests.Count
                )

                $Categories =
                $SuiteJson.Tests |
                        Group-Object Category |
                        Sort-Object Name

                Write-Line
                e-Line "Categories:"

                foreach ($category in $Categories)
                {
                    Write-Line (
                    "  {0}: {1}" -f `
                            $category.Name,
                    $category.Count
                    )
                }
            }
            else
            {
                Write-Line (
                "Tests property was not found."
                )
            }
        }
        catch
        {
            Write-Line (
            "Suite parse error: " +
                    $_.Exception.Message
            )
        }
    }
    else
    {
        Write-Line "<suite not found>"
    }
}

# ============================================================
# Git status
# ============================================================

Write-Section `
    -Title "GIT STATUS"

Push-Location $Root

try
{
    $GitStatus =
    git status --short 2>&1

    if ($GitStatus)
    {
        foreach ($line in $GitStatus)
        {
            Write-Line (
            [string] $line
            )
        }
    }
    else
    {
        Write-Line "<clean>"
    }
}
catch
{
    Write-Line (
    "Git status failed: " +
            $_.Exception.Message
    )
}
finally
{
    Pop-Location
}

# ============================================================
# Optional build
# ============================================================

Write-Section `
    -Title "BUILD"

if (-not $RunBuild)
{
    Write-Line (
    "Build not requested. " +
            "Run crawler with -RunBuild to include build output."
    )
}
else
{
    $Solution =
    Get-ChildItem `
            -LiteralPath $Root `
            -File `
            -Filter "*.sln" |
            Select-Object `
            -First 1

    if (-not $Solution)
    {
        Write-Line "<solution not found>"
    }
    else
    {
        Write-Line (
        "Solution: " +
                $Solution.FullName
        )

        Write-Line

        Push-Location $Root

        try
        {
            $BuildOutput =
            & dotnet build `
                    $Solution.FullName `
                    -c Debug `
                    --nologo 2>&1

            foreach ($line in $BuildOutput)
            {
                Write-Line (
                [string] $line
                )
            }

            Write-Line
            Write-Line (
            "dotnet exit code: " +
                    $LASTEXITCODE
            )
        }
        catch
        {
            Write-Line (
            "Build invocation failed: " +
                    $_.Exception.Message
            )
        }
        finally
        {
            Pop-Location
        }
    }
}

# ============================================================
# Summary
# ============================================================

Write-Section `
    -Title "CRAWL SUMMARY"

Write-Line (
"Projects: " +
        $Projects.Count
)

Write-Line (
"Wilds source files scanned: " +
        $SourceFiles.Count
)

Write-Line (
"Relevant source files captured: " +
        $RelevantFiles.Count
)

Write-Line (
"Report: " +
        $ReportPath
)

# Keep stable latest copy.
Copy-Item `
    -LiteralPath $ReportPath `
    -Destination $LatestPath `
    -Force

Write-Host ""
Write-Host "========================================"
Write-Host "WILDS / QSL CERTIFICATION CRAWL COMPLETE"
Write-Host "========================================"
Write-Host ""
Write-Host "Report:"
Write-Host $ReportPath
Write-Host ""
Write-Host "Latest:"
Write-Host $LatestPath
Write-Host ""
Write-Host "Projects:" $Projects.Count
Write-Host "Sources:" $SourceFiles.Count
Write-Host "Relevant:" $RelevantFiles.Count
Write-Host ""