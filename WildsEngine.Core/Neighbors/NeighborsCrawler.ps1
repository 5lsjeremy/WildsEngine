$ErrorActionPreference = "Stop"

try {
    # ------------------------------------------------------------
    # Resolve locations from this script's actual location
    # ------------------------------------------------------------

    $NeighborsRoot = $PSScriptRoot
    $CoreRoot = Split-Path $NeighborsRoot -Parent
    $RepoRoot = Split-Path $CoreRoot -Parent

    if ((Split-Path $NeighborsRoot -Leaf) -ne "Neighbors") {
        throw "NeighborsCrawler.ps1 must live in WildsEngine.Core\Neighbors."
    }

    $AuditDirectory =
    Join-Path $RepoRoot "Audit"

    $ReportsDirectory =
    Join-Path $AuditDirectory "Reports"

    $NeighborsReportDirectory =
    Join-Path $ReportsDirectory "Neighbors"

    if (-not (Test-Path $NeighborsReportDirectory)) {
        New-Item `
        -ItemType Directory `
        -Path $NeighborsReportDirectory `
        -Force |
                Out-Null
    }

    $Timestamp =
    Get-Date -Format "yyyyMMdd-HHmmss"

    $OutputFile =
    Join-Path `
        $NeighborsReportDirectory `
        "Wilds-Neighbors-Crawl-$Timestamp.txt"

    $LatestFile =
    Join-Path `
        $NeighborsReportDirectory `
        "Wilds-Neighbors-Crawl-Latest.txt"

    Write-Host ""
    Write-Host "============================================"
    Write-Host " WildsEngine Neighbors Crawler"
    Write-Host "============================================"
    Write-Host ""
    Write-Host "Neighbors root : $NeighborsRoot"
    Write-Host "Core root      : $CoreRoot"
    Write-Host "Repo root      : $RepoRoot"
    Write-Host "Output         : $OutputFile"
    Write-Host ""

    # ------------------------------------------------------------
    # Helpers
    # ------------------------------------------------------------

    $Builder =
    New-Object System.Text.StringBuilder

    function Add-Line {
        param(
            [string] $Value = ""
        )

        [void] $Builder.AppendLine(
                $Value
        )
    }

    function Add-Section {
        param(
            [string] $Title
        )

        Add-Line ""
        Add-Line "============================================================"
        Add-Line $Title
        Add-Line "============================================================"
    }

    function Get-RelativePath {
        param(
            [string] $FullPath
        )

        if ($FullPath.StartsWith(
                $RepoRoot,
                [System.StringComparison]::OrdinalIgnoreCase)) {

            $Relative =
            $FullPath.Substring(
                    $RepoRoot.Length
            )

            return $Relative -replace '^[\\/]+', ''
        }

        return $FullPath
    }

    function Add-SourceFile {
        param(
            [System.IO.FileInfo] $File
        )

        $RelativePath =
        Get-RelativePath `
                $File.FullName

        Add-Line ""
        Add-Line "############################################################"
        Add-Line "FILE: $RelativePath"
        Add-Line "############################################################"

        $LineNumber = 0

        foreach ($Line in Get-Content -LiteralPath $File.FullName) {
            $LineNumber++

            Add-Line (
            "{0,5}: {1}" -f
            $LineNumber,
            $Line
            )
        }
    }

    # ------------------------------------------------------------
    # Locate Neighbor implementation files
    # ------------------------------------------------------------

    $NeighborFiles =
    @(
    Get-ChildItem `
                -Path $NeighborsRoot `
                -Recurse `
                -File `
                -Filter "*.cs" |
            Sort-Object FullName
    )

    Write-Host (
    "Neighbor files : " +
            $NeighborFiles.Count
    )

    # ------------------------------------------------------------
    # Locate relevant abstractions
    # ------------------------------------------------------------

    $AbstractionRoot =
    Join-Path `
            $CoreRoot `
            "Abstractions"

    if (-not (Test-Path $AbstractionRoot)) {
        throw "Abstractions directory not found: $AbstractionRoot"
    }

    $AbstractionFiles =
    @(
    Get-ChildItem `
                -Path $AbstractionRoot `
                -Recurse `
                -File `
                -Filter "*.cs" |
            Where-Object {
                $_.FullName -match "\\Neighbors\\" -or
                        $_.FullName -match "\\Topology\\" -or
                        $_.Name -eq "IBaseObject.cs" -or
                        $_.Name -eq "INeighborCollection.cs" -or
                        $_.Name -eq "INeighborRelationship.cs" -or
                        $_.Name -eq "NeighborDistance.cs"
            } |
            Sort-Object FullName
    )

    Write-Host (
    "Abstractions   : " +
            $AbstractionFiles.Count
    )

    $AllFiles =
    @(
        $NeighborFiles +
                $AbstractionFiles
    )

    # ------------------------------------------------------------
    # Header
    # ------------------------------------------------------------

    Add-Line "WildsEngine Neighbors Crawl"
    Add-Line (
    "Generated: " +
            (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    )
    Add-Line "Repository: $RepoRoot"
    Add-Line "Core: $CoreRoot"
    Add-Line "Neighbors: $NeighborsRoot"

    # ------------------------------------------------------------
    # Tree
    # ------------------------------------------------------------

    Add-Section "NEIGHBORS TREE"

    foreach ($File in $NeighborFiles) {
        Add-Line (
        Get-RelativePath `
                $File.FullName
        )
    }

    Add-Line ""
    Add-Line (
    "Neighbor source files: " +
            $NeighborFiles.Count
    )

    # ------------------------------------------------------------
    # Abstraction inventory
    # ------------------------------------------------------------

    Add-Section "RELEVANT ABSTRACTIONS"

    foreach ($File in $AbstractionFiles) {
        Add-Line (
        Get-RelativePath `
                $File.FullName
        )
    }

    Add-Line ""
    Add-Line (
    "Relevant abstraction files: " +
            $AbstractionFiles.Count
    )

    # ------------------------------------------------------------
    # Declaration / keyword index
    # ------------------------------------------------------------

    Add-Section "DECLARATION AND CAPABILITY INDEX"

    $Patterns = @(
        "class ",
        "interface ",
        "abstract class ",
        "sealed class ",
        "INeighbor",
        "NeighborOperator",
        "NeighborPipeline",
        "Membership",
        "Distance",
        "Sample",
        "Group",
        "Cluster",
        "Scalar",
        "Propagation",
        "Eligibility",
        "Influence"
    )

    foreach ($Pattern in $Patterns) {
        Add-Line ""
        Add-Line "------------------------------------------------------------"
        Add-Line "PATTERN: $Pattern"
        Add-Line "------------------------------------------------------------"

        $Matches =
        @(
        $AllFiles |
                Select-String `
                    -Pattern $Pattern `
                    -SimpleMatch
        )

        if ($Matches.Count -eq 0) {
            Add-Line "<no matches>"
            continue
        }

        foreach ($Match in $Matches) {
            $Relative =
            Get-RelativePath `
                    $Match.Path

            Add-Line (
            "{0}:{1}: {2}" -f
            $Relative,
            $Match.LineNumber,
            $Match.Line.Trim()
            )
        }
    }

    # ------------------------------------------------------------
    # Focused Gate 2 inventory
    # ------------------------------------------------------------

    Add-Section "GATE 2 FOCUSED INDEX"

    $Gate2Patterns = @(
        "MembershipNeighborFilter",
        "MembershipNeighborPredicate",
        "MembershipNeighborOrder",
        "NeighborMembershipGroups",
        "NeighborCount",
        "AverageNeighborDistance",
        "MinimumNeighborDistance",
        "MaximumNeighborDistance",
        "DeterministicNeighborSample",
        "RandomNeighborSample",
        "NeighborPipeline",
        "INeighborScalarOperator",
        "INeighborGroupingOperator",
        "INeighborSequenceOperator",
        "INeighborPredicate",
        "INeighborSampler",
        "ExponentialDistancePropagation",
        "MinimumIntensityEligibility"
    )

    foreach ($Pattern in $Gate2Patterns) {
        Add-Line ""
        Add-Line "------------------------------------------------------------"
        Add-Line "TYPE: $Pattern"
        Add-Line "------------------------------------------------------------"

        $Matches =
        @(
        $AllFiles |
                Select-String `
                    -Pattern $Pattern `
                    -SimpleMatch
        )

        if ($Matches.Count -eq 0) {
            Add-Line "<no matches>"
            continue
        }

        foreach ($Match in $Matches) {
            $Relative =
            Get-RelativePath `
                    $Match.Path

            Add-Line (
            "{0}:{1}: {2}" -f
            $Relative,
            $Match.LineNumber,
            $Match.Line.Trim()
            )
        }
    }

    # ------------------------------------------------------------
    # Full implementation capture
    # ------------------------------------------------------------

    Add-Section "NEIGHBOR SOURCE CAPTURE"

    foreach ($File in $NeighborFiles) {
        Add-SourceFile `
            $File
    }

    # ------------------------------------------------------------
    # Full abstraction capture
    # ------------------------------------------------------------

    Add-Section "NEIGHBOR AND TOPOLOGY ABSTRACTION SOURCE CAPTURE"

    foreach ($File in $AbstractionFiles) {
        Add-SourceFile `
            $File
    }

    # ------------------------------------------------------------
    # Write files
    # ------------------------------------------------------------

    $Content =
    $Builder.ToString()

    [System.IO.File]::WriteAllText(
            $OutputFile,
            $Content,
            [System.Text.Encoding]::UTF8
    )

    Copy-Item `
        -LiteralPath $OutputFile `
        -Destination $LatestFile `
        -Force

    if (-not (Test-Path $OutputFile)) {
        throw "Timestamped crawl was not written."
    }

    if (-not (Test-Path $LatestFile)) {
        throw "Latest crawl was not written."
    }

    $WrittenFile =
    Get-Item `
            -LiteralPath $OutputFile

    Write-Host ""
    Write-Host "============================================"
    Write-Host " Crawl complete"
    Write-Host "============================================"
    Write-Host ""
    Write-Host "Timestamped:"
    Write-Host "  $OutputFile"
    Write-Host ""
    Write-Host "Latest:"
    Write-Host "  $LatestFile"
    Write-Host ""
    Write-Host (
    "Bytes written   : " +
            $WrittenFile.Length
    )
    Write-Host (
    "Neighbor files  : " +
            $NeighborFiles.Count
    )
    Write-Host (
    "Abstractions    : " +
            $AbstractionFiles.Count
    )
    Write-Host ""
    Write-Host "Ready for Gate 2."
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "============================================" `
        -ForegroundColor Red
    Write-Host " CRAWL FAILED" `
        -ForegroundColor Red
    Write-Host "============================================" `
        -ForegroundColor Red
    Write-Host ""

    Write-Host $_.Exception.Message `
        -ForegroundColor Red

    Write-Host ""

    if ($_.InvocationInfo) {
        Write-Host (
        "Line: " +
                $_.InvocationInfo.ScriptLineNumber
        ) -ForegroundColor Yellow

        Write-Host (
        "Command: " +
                $_.InvocationInfo.Line
        ) -ForegroundColor Yellow
    }

    Write-Host ""

    exit 1
}