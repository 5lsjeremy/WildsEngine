$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# ============================================================
# E7-H PRE-COMMIT AUDIT
#
# Expected script location:
# <repo>\WildsEngine.Discoveries\Audits\
#     Invoke-E7H-PreCommitAudit.ps1
#
# Current working directory does not matter.
#
# READ ONLY:
# - does not modify production source;
# - does not stage files;
# - does not commit;
# - does not modify WildsEngine.Core.
#
# Produces:
#
# Audits\Reports\
#   E7-H-PreCommit-Audit-<timestamp>.txt
#   E7-H-PreCommit-Audit-Latest.txt
# ============================================================

# ============================================================
# PATHS
# ============================================================

$AuditsRoot =
$PSScriptRoot

$ProjectRoot =
Split-Path -Parent $AuditsRoot

$RepoRoot =
Split-Path -Parent $ProjectRoot

$ProjectFile =
Join-Path `
        $ProjectRoot `
        "WildsEngine.Discoveries.csproj"

$AbstractionsDir =
Join-Path `
        $ProjectRoot `
        "Abstractions"

$BaseDir =
Join-Path `
        $ProjectRoot `
        "Base"

$ReportDir =
Join-Path `
        $AuditsRoot `
        "Reports"

$Timestamp =
Get-Date -Format "yyyyMMdd-HHmmss"

$Report =
Join-Path `
        $ReportDir `
        "E7-H-PreCommit-Audit-$Timestamp.txt"

$Latest =
Join-Path `
        $ReportDir `
        "E7-H-PreCommit-Audit-Latest.txt"

# ============================================================
# PATH VALIDATION
# ============================================================

if (-not (Test-Path -LiteralPath $ProjectFile))
{
    throw @"
Could not locate WildsEngine.Discoveries.csproj.

Script:
  $PSCommandPath

Expected:
  $ProjectFile
"@
}

if (-not (Test-Path -LiteralPath $AbstractionsDir))
{
    throw "Missing Abstractions directory: $AbstractionsDir"
}

if (-not (Test-Path -LiteralPath $BaseDir))
{
    throw "Missing Base directory: $BaseDir"
}

New-Item `
    -ItemType Directory `
    -Path $ReportDir `
    -Force |
        Out-Null

# ============================================================
# STATE
# ============================================================

$Script:FailureCount = 0
$Script:WarningCount = 0
$Script:PassCount = 0

# ============================================================
# REPORT HELPERS
# ============================================================

function Write-Line
{
    param(
        [string]$Text = ""
    )

    Write-Host $Text

    $Text |
            Out-File `
        -LiteralPath $Report `
        -Append `
        -Encoding utf8
}

function Write-Section
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Title
    )

    Write-Line ""
    Write-Line "============================================================"
    Write-Line $Title
    Write-Line "============================================================"
}

function Write-Pass
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $Script:PassCount++

    Write-Line "[PASS] $Text"
}

function Write-Warning
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $Script:WarningCount++

    Write-Line "[WARN] $Text"
}

function Write-Failure
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $Script:FailureCount++

    Write-Line "[FAIL] $Text"
}

function Write-CommandOutput
{
    param(
        [AllowNull()]
        [object[]]$Lines
    )

    if (-not $Lines)
    {
        Write-Line "<none>"
        return
    }

    foreach ($Line in $Lines)
    {
        Write-Line ([string]$Line)
    }
}

# ============================================================
# HEADER
# ============================================================

Write-Section "WILDS DISCOVERIES E7-H PRE-COMMIT AUDIT"

Write-Line "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "Repo root: $RepoRoot"
Write-Line "Discoveries root: $ProjectRoot"
Write-Line "Project file: $ProjectFile"
Write-Line "Report: $Report"
Write-Line "Latest: $Latest"

# ============================================================
# 1. CORE MUST BE CLEAN
# ============================================================

Write-Section "1. CORE MUST BE CLEAN"

$CoreChanges =
@(
    git `
        -C $RepoRoot `
        status `
        --short `
        -- `
        .\WildsEngine.Core
)

if ($LASTEXITCODE -ne 0)
{
    Write-Failure "git status failed while checking WildsEngine.Core."
}
elseif ($CoreChanges.Count -gt 0)
{
    Write-Failure "WildsEngine.Core contains working-tree changes."

    Write-Line ""

    foreach ($Line in $CoreChanges)
    {
        Write-Line "  $Line"
    }
}
else
{
    Write-Pass "WildsEngine.Core is unchanged."
}

# ============================================================
# 2. DISCOVERIES STATUS
# ============================================================

Write-Section "2. DISCOVERIES WORKING-TREE STATUS"

$DiscoveriesStatus =
@(
    git `
        -C $RepoRoot `
        status `
        --short `
        -- `
        .\WildsEngine.Discoveries
)

if ($LASTEXITCODE -ne 0)
{
    Write-Failure "git status failed while checking WildsEngine.Discoveries."
}
else
{
    Write-CommandOutput `
        -Lines $DiscoveriesStatus

    Write-Line ""

    Write-Line (
    "Status entries: {0}" -f
    $DiscoveriesStatus.Count
    )
}

# ============================================================
# 3. STALE / BACKUP FILES
# ============================================================

Write-Section "3. STALE / BACKUP FILES"

$StaleFiles =
@(
Get-ChildItem `
        -LiteralPath $ProjectRoot `
        -Recurse `
        -File |
        Where-Object {
            $_.Name -eq "IDiscoveryVectorHistory.cs" -or
                    $_.Name -match "\.(old|bak|backup)$" -or
                    $_.Name -match "~$"
        }
)

if ($StaleFiles.Count -gt 0)
{
    Write-Failure "Stale or backup files remain."

    foreach ($File in $StaleFiles)
    {
        Write-Line (
        "  {0}" -f
        $File.FullName
        )
    }
}
else
{
    Write-Pass "No stale or backup source files found."
}

# ============================================================
# 4. DISCOVERIES PRODUCTION FILE SET
# ============================================================

Write-Section "4. PRODUCTION SOURCE INVENTORY"

$ProductionFiles =
@(
Get-ChildItem `
        -LiteralPath $ProjectRoot `
        -Recurse `
        -File `
        -Filter *.cs |
        Where-Object {
            $_.FullName -notmatch "\\bin\\" -and
                    $_.FullName -notmatch "\\obj\\" -and
                    $_.FullName -notmatch "\\Audits\\"
        } |
        Sort-Object FullName
)

Write-Line (
"Production C# files: {0}" -f
$ProductionFiles.Count
)

# ============================================================
# 5. E7-H REQUIRED CONTRACTS
# ============================================================

Write-Section "5. REQUIRED E7-H CONTRACTS"

$RequiredFiles = @(
    (Join-Path $AbstractionsDir "IConsequence.cs"),
    (Join-Path $AbstractionsDir "IConsequenceProjectionPolicy.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryCluster.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryClusterer.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVector.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorComponent.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorContribution.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorInput.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorMetric.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorMotion.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorPoint.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorSeries.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorizationPolicy.cs"),
    (Join-Path $AbstractionsDir "ITruth.cs"),
    (Join-Path $AbstractionsDir "ITruthCongealmentPolicy.cs"),

    (Join-Path $BaseDir "ConsequenceBase.cs"),
    (Join-Path $BaseDir "DiscoveryClusterBase.cs"),
    (Join-Path $BaseDir "DiscoveryDimensions.cs"),
    (Join-Path $BaseDir "DiscoveryNumbers.cs"),
    (Join-Path $BaseDir "DiscoverySnapshots.cs"),
    (Join-Path $BaseDir "DiscoveryVectorBase.cs"),
    (Join-Path $BaseDir "TruthBase.cs")
)

$MissingRequiredFiles =
@(
$RequiredFiles |
        Where-Object {
            -not (Test-Path -LiteralPath $_)
        }
)

if ($MissingRequiredFiles.Count -gt 0)
{
    Write-Failure "Required E7-H files are missing."

    foreach ($File in $MissingRequiredFiles)
    {
        Write-Line "  $File"
    }
}
else
{
    Write-Pass (
    "All {0} required E7-H files exist." -f
    $RequiredFiles.Count
    )
}

# ============================================================
# 6. INTRINSIC H CONTRACTS MUST BE CORE-NEUTRAL
# ============================================================

Write-Section "6. INTRINSIC E7-H CORE DEPENDENCY AUDIT"

$HInterfaces =
@(
Get-ChildItem `
        -LiteralPath $AbstractionsDir `
        -File `
        -Filter *.cs |
        Where-Object {
            $_.Name -match `
            "DiscoveryVector|DiscoveryCluster|Truth|Consequence"
        }
)

$CoreLeakPattern =
"WildsEngine\.Core|" +
        "\bINeighborRelationship\b|" +
        "\bIBaseObject\b|" +
        "\bIBaseUnit\b"

$CoreLeaks =
@(
$HInterfaces |
        Select-String `
        -Pattern $CoreLeakPattern `
        -CaseSensitive:$false
)

if ($CoreLeaks.Count -gt 0)
{
    Write-Failure "Intrinsic E7-H interfaces expose Core types."

    foreach ($Hit in $CoreLeaks)
    {
        Write-Line (
        "{0}:{1}: {2}" -f
        $Hit.Path,
        $Hit.LineNumber,
        $Hit.Line.Trim()
        )
    }
}
else
{
    Write-Pass "Intrinsic E7-H interfaces are Core-neutral."
}

# ============================================================
# 7. OBSOLETE SYMBOLS
# ============================================================

Write-Section "7. OBSOLETE E7-H SYMBOLS"

$ObsoleteHits =
@(
$ProductionFiles |
        Select-String `
        -Pattern "\bIDiscoveryVectorHistory\b|\.Position\b" `
        -CaseSensitive:$false
)

if ($ObsoleteHits.Count -gt 0)
{
    Write-Failure "Obsolete E7-H symbols remain."

    foreach ($Hit in $ObsoleteHits)
    {
        Write-Line (
        "{0}:{1}: {2}" -f
        $Hit.Path,
        $Hit.LineNumber,
        $Hit.Line.Trim()
        )
    }
}
else
{
    Write-Pass "No obsolete E7-H symbols remain."
}

# ============================================================
# 8. PARALLEL IDENTITY AUDIT
#
# Truth and Consequence inherit IIdentifier, so Guid Id arrives
# through the established identity contract.
#
# Vector and cluster interfaces should not declare their own Id.
# ============================================================

Write-Section "8. IDENTITY REUSE AUDIT"

$VectorIdentityHits =
@(
@(
    (Join-Path $AbstractionsDir "IDiscoveryVector.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorPoint.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryCluster.cs")
) |
        Select-String `
        -Pattern "Guid\s+Id\s*\{\s*get;\s*\}" `
        -CaseSensitive:$false
)

if ($VectorIdentityHits.Count -gt 0)
{
    Write-Failure "Vector or cluster contracts introduced independent Guid identity."

    foreach ($Hit in $VectorIdentityHits)
    {
        Write-Line (
        "{0}:{1}: {2}" -f
        $Hit.Path,
        $Hit.LineNumber,
        $Hit.Line.Trim()
        )
    }
}
else
{
    Write-Pass "Vectors and clusters do not introduce parallel identity."
}

$TruthIdentifier =
@(
    Select-String `
        -LiteralPath (Join-Path $AbstractionsDir "ITruth.cs") `
        -Pattern "\bIIdentifier\b"
)

$ConsequenceIdentifier =
@(
    Select-String `
        -LiteralPath (Join-Path $AbstractionsDir "IConsequence.cs") `
        -Pattern "\bIIdentifier\b"
)

if ($TruthIdentifier.Count -gt 0 -and
        $ConsequenceIdentifier.Count -gt 0)
{
    Write-Pass "Truth and Consequence reuse IIdentifier."
}
else
{
    Write-Failure "Truth and/or Consequence do not reuse IIdentifier."
}

# ============================================================
# 9. BASE INHERITANCE
# ============================================================

Write-Section "9. BASE IDENTITY INHERITANCE"

$TruthBaseIdentity =
@(
    Select-String `
        -LiteralPath (Join-Path $BaseDir "TruthBase.cs") `
        -Pattern "\bIdentifierBase\b"
)

$ConsequenceBaseIdentity =
@(
    Select-String `
        -LiteralPath (Join-Path $BaseDir "ConsequenceBase.cs") `
        -Pattern "\bIdentifierBase\b"
)

if ($TruthBaseIdentity.Count -gt 0)
{
    Write-Pass "TruthBase reuses IdentifierBase."
}
else
{
    Write-Failure "TruthBase does not reuse IdentifierBase."
}

if ($ConsequenceBaseIdentity.Count -gt 0)
{
    Write-Pass "ConsequenceBase reuses IdentifierBase."
}
else
{
    Write-Failure "ConsequenceBase does not reuse IdentifierBase."
}

# ============================================================
# 10. VECTOR STRUCTURE
# ============================================================

Write-Section "10. VECTOR STRUCTURE"

$VectorFile =
Join-Path $AbstractionsDir "IDiscoveryVector.cs"

$VectorPatterns = @(
    "IDiscoveryVectorPoint",
    "Contributions",
    "Motion",
    "Metric",
    "Magnitude",
    "EvaluatedAt"
)

foreach ($Pattern in $VectorPatterns)
{
    $Hit =
    @(
        Select-String `
            -LiteralPath $VectorFile `
            -Pattern $Pattern
    )

    if ($Hit.Count -gt 0)
    {
        Write-Pass "IDiscoveryVector exposes $Pattern."
    }
    else
    {
        Write-Failure "IDiscoveryVector is missing $Pattern."
    }
}

# ============================================================
# 11. VECTOR INPUT / PROVENANCE BOUNDARY
# ============================================================

Write-Section "11. VECTOR INPUT / PROVENANCE BOUNDARY"

$VectorInputFile =
Join-Path $AbstractionsDir "IDiscoveryVectorInput.cs"

$InputDiscovery =
@(
    Select-String `
        -LiteralPath $VectorInputFile `
        -Pattern "\bIDiscovery\s+Discovery\b"
)

$InputProvenance =
@(
    Select-String `
        -LiteralPath $VectorInputFile `
        -Pattern "object\?\s+Provenance"
)

if ($InputDiscovery.Count -gt 0)
{
    Write-Pass "Vector input preserves an authoritative Discovery."
}
else
{
    Write-Failure "Vector input does not expose its authoritative Discovery."
}

if ($InputProvenance.Count -gt 0)
{
    Write-Pass "Vector input exposes opaque external provenance."
}
else
{
    Write-Failure "Vector input does not expose external provenance."
}

# ============================================================
# 12. VECTOR SERIES SCOPE
# ============================================================

Write-Section "12. VECTOR SERIES SCOPE"

$SeriesFile =
Join-Path $AbstractionsDir "IDiscoveryVectorSeries.cs"

$SeriesPatterns = @(
    "Scope",
    "Inputs",
    "Vectors",
    "Current",
    "Previous"
)

foreach ($Pattern in $SeriesPatterns)
{
    $Hit =
    @(
        Select-String `
            -LiteralPath $SeriesFile `
            -Pattern $Pattern
    )

    if ($Hit.Count -gt 0)
    {
        Write-Pass "IDiscoveryVectorSeries exposes $Pattern."
    }
    else
    {
        Write-Failure "IDiscoveryVectorSeries is missing $Pattern."
    }
}

# ============================================================
# 13. TRUTH COMPOSITION
# ============================================================

Write-Section "13. TRUTH COMPOSITION"

$TruthFile =
Join-Path $AbstractionsDir "ITruth.cs"

$TruthPatterns = @(
    "IIdentifier",
    "Series",
    "Clusters",
    "Discoveries",
    "Stability",
    "Confidence",
    "CongealedAt"
)

foreach ($Pattern in $TruthPatterns)
{
    $Hit =
    @(
        Select-String `
            -LiteralPath $TruthFile `
            -Pattern $Pattern
    )

    if ($Hit.Count -gt 0)
    {
        Write-Pass "ITruth exposes $Pattern."
    }
    else
    {
        Write-Failure "ITruth is missing $Pattern."
    }
}

# ============================================================
# 14. CONSEQUENCE BOUNDARY
# ============================================================

Write-Section "14. CONSEQUENCE BOUNDARY"

$ConsequenceFile =
Join-Path $AbstractionsDir "IConsequence.cs"

$ConsequencePatterns = @(
    "IIdentifier",
    "Truths",
    "Magnitude",
    "Confidence",
    "ProjectedAt"
)

foreach ($Pattern in $ConsequencePatterns)
{
    $Hit =
    @(
        Select-String `
            -LiteralPath $ConsequenceFile `
            -Pattern $Pattern
    )

    if ($Hit.Count -gt 0)
    {
        Write-Pass "IConsequence exposes $Pattern."
    }
    else
    {
        Write-Failure "IConsequence is missing $Pattern."
    }
}

# ============================================================
# 15. SHARED INTERNAL MECHANICS
# ============================================================

Write-Section "15. SHARED INTERNAL MECHANICS"

$HelperFiles = @(
    (Join-Path $BaseDir "DiscoverySnapshots.cs"),
    (Join-Path $BaseDir "DiscoveryNumbers.cs"),
    (Join-Path $BaseDir "DiscoveryDimensions.cs")
)

foreach ($HelperFile in $HelperFiles)
{
    $InternalHit =
    @(
        Select-String `
            -LiteralPath $HelperFile `
            -Pattern "internal\s+static\s+class"
    )

    if ($InternalHit.Count -gt 0)
    {
        Write-Pass (
        "{0} remains internal." -f
        (Split-Path -Leaf $HelperFile)
        )
    }
    else
    {
        Write-Failure (
        "{0} is not an internal static helper." -f
        (Split-Path -Leaf $HelperFile)
        )
    }
}

# ============================================================
# 16. ACCIDENTAL NESTED AUDIT TREE
# ============================================================

Write-Section "16. ACCIDENTAL NESTED AUDIT TREE"

$NestedAudit =
Join-Path `
        $AbstractionsDir `
        "Audits"

if (Test-Path -LiteralPath $NestedAudit)
{
    $NestedFiles =
    @(
        Get-ChildItem `
            -LiteralPath $NestedAudit `
            -Recurse `
            -File
    )

    if ($NestedFiles.Count -eq 0)
    {
        Write-Warning (
        "Empty nested audit directory exists: {0}" -f
        $NestedAudit
        )
    }
    else
    {
        Write-Failure (
        "Nested Abstractions audit directory contains files: {0}" -f
        $NestedAudit
        )

        foreach ($File in $NestedFiles)
        {
            Write-Line "  $($File.FullName)"
        }
    }
}
else
{
    Write-Pass "No nested Abstractions audit tree exists."
}

# ============================================================
# 17. RELEASE BUILD
# ============================================================

Write-Section "17. RELEASE BUILD"

$BuildOutput =
@(
    & dotnet `
        build `
        $ProjectFile `
        -c Release `
        --no-restore 2>&1
)

$BuildExitCode =
$LASTEXITCODE

Write-CommandOutput `
    -Lines $BuildOutput

Write-Line ""

if ($BuildExitCode -eq 0)
{
    Write-Pass "WildsEngine.Discoveries Release build passed."
}
else
{
    Write-Failure (
    "WildsEngine.Discoveries Release build failed with exit code {0}." -f
    $BuildExitCode
    )
}

# ============================================================
# 18. CORE RECHECK AFTER BUILD
# ============================================================

Write-Section "18. CORE RECHECK AFTER BUILD"

$CoreChangesAfter =
@(
    git `
        -C $RepoRoot `
        status `
        --short `
        -- `
        .\WildsEngine.Core
)

if ($LASTEXITCODE -ne 0)
{
    Write-Failure "git status failed during final Core check."
}
elseif ($CoreChangesAfter.Count -gt 0)
{
    Write-Failure "WildsEngine.Core changed during audit/build."

    foreach ($Line in $CoreChangesAfter)
    {
        Write-Line "  $Line"
    }
}
else
{
    Write-Pass "WildsEngine.Core remains unchanged."
}

# ============================================================
# 19. UNSTAGED DIFF STAT
# ============================================================

Write-Section "19. UNSTAGED DISCOVERIES DIFF STAT"

$UnstagedStat =
@(
    git `
        -C $RepoRoot `
        diff `
        --stat `
        -- `
        .\WildsEngine.Discoveries
)

Write-CommandOutput `
    -Lines $UnstagedStat

# ============================================================
# 20. STAGED DIFF STAT
# ============================================================

Write-Section "20. STAGED DISCOVERIES DIFF STAT"

$StagedStat =
@(
    git `
        -C $RepoRoot `
        diff `
        --cached `
        --stat `
        -- `
        .\WildsEngine.Discoveries
)

Write-CommandOutput `
    -Lines $StagedStat

# ============================================================
# 21. FINAL STATUS
# ============================================================

Write-Section "21. FINAL DISCOVERIES STATUS"

$FinalStatus =
@(
    git `
        -C $RepoRoot `
        status `
        --short `
        -- `
        .\WildsEngine.Discoveries
)

Write-CommandOutput `
    -Lines $FinalStatus

# ============================================================
# SUMMARY
# ============================================================

Write-Section "AUDIT SUMMARY"

Write-Line "Passes: $Script:PassCount"
Write-Line "Warnings: $Script:WarningCount"
Write-Line "Failures: $Script:FailureCount"

Write-Line ""

if ($Script:FailureCount -eq 0)
{
    if ($Script:WarningCount -eq 0)
    {
        Write-Line "RESULT: PASS"
        Write-Line "E7-H structure is ready for final diff review before commit."
    }
    else
    {
        Write-Line "RESULT: PASS WITH WARNINGS"
        Write-Line "Review warnings before commit."
    }
}
else
{
    Write-Line "RESULT: FAIL"
    Write-Line "Do not commit until failures are resolved."
}

Write-Line ""
Write-Line "Timestamped report:"
Write-Line $Report

Write-Line ""
Write-Line "Latest report:"
Write-Line $Latest

# ============================================================
# PUBLISH LATEST REPORT
# ============================================================

Copy-Item `
    -LiteralPath $Report `
    -Destination $Latest `
    -Force

Write-Host ""
Write-Host "============================================================"

if ($Script:FailureCount -eq 0)
{
    Write-Host " E7-H PRE-COMMIT AUDIT COMPLETE: PASS" -ForegroundColor Green
}
else
{
    Write-Host " E7-H PRE-COMMIT AUDIT COMPLETE: FAIL" -ForegroundColor Red
}

Write-Host "============================================================"
Write-Host "Report : $Report"
Write-Host "Latest : $Latest"
Write-Host "============================================================"

if ($Script:FailureCount -gt 0)
{
    exit 1
}

exit 0