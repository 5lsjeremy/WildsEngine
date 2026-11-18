$ErrorActionPreference = "Stop"

# ============================================================
# E7-H CONTEXT CRAWL
#
# Expected script location:
# <repo>\Audit\Invoke-E7H-Context-Crawl.ps1
#
# This crawl is intentionally explicit. It collects only the
# source needed to evaluate E7-H against:
#
# - Core object and neighbor contracts
# - Neighbor operator contracts
# - Neighbor operator base classes
# - Concrete topology and sampling operators
# - Core neighbor object implementations
# - Discoveries contracts, base classes, and concrete objects
# - Existing E7 fixtures and certification tests
#
# It does not crawl entire projects.
# ============================================================

$AuditRoot = $PSScriptRoot
$RepoRoot = Split-Path -Parent $AuditRoot

$CoreRoot =
Join-Path $RepoRoot "WildsEngine.Core"

$DiscoveriesRoot =
Join-Path $RepoRoot "WildsEngine.Discoveries"

$TestsRoot =
Join-Path $RepoRoot "WildsEngine.Core.Tests"

$Timestamp =
Get-Date -Format "yyyyMMdd-HHmmss"

$ReportDir =
Join-Path $AuditRoot "Reports\E7H"

$Report =
Join-Path `
        $ReportDir `
        "Wilds-E7H-Context-Crawl-$Timestamp.txt"

$Latest =
Join-Path `
        $ReportDir `
        "Wilds-E7H-Context-Crawl-Latest.txt"

New-Item `
    -ItemType Directory `
    -Path $ReportDir `
    -Force |
        Out-Null

# ============================================================
# EXPLICIT SOURCE PATHS
#
# Every entry below exists in the supplied repository tree.
# Directories are crawled recursively.
# Individual files are included exactly.
# ============================================================

$RequestedPaths = @(
# --------------------------------------------------------
# CORE: foundational object and neighbor contracts
# --------------------------------------------------------

    (Join-Path $CoreRoot "Abstractions\Base"),

    # --------------------------------------------------------
    # CORE: neighbor operator contracts
    # --------------------------------------------------------

    (Join-Path $CoreRoot "Abstractions\Neighbors\Operators"),

    # --------------------------------------------------------
    # CORE: concrete neighbor propagation contracts
    # Included because Evidence can carry Neighbor context,
    # but no emission or propagation implementation is crawled.
    # --------------------------------------------------------

    (Join-Path $CoreRoot "Abstractions\Neighbors\Propagation"),

    # --------------------------------------------------------
    # CORE: topology contracts
    # Included to distinguish operators/sampling from traversal,
    # routing, clustering, and propagation.
    # --------------------------------------------------------

    (Join-Path $CoreRoot "Abstractions\Topology"),

    # --------------------------------------------------------
    # CORE: ordinary object implementations
    # BaseObject supplies the runtime host for Wilds projections.
    # NeighborCollection and NeighborRelationship supply topology.
    # --------------------------------------------------------

    (Join-Path $CoreRoot "Objects\BaseObject.cs"),
    (Join-Path $CoreRoot "Objects\NeighborCollection.cs"),
    (Join-Path $CoreRoot "Objects\NeighborRelationship.cs"),

    # --------------------------------------------------------
    # CORE: all concrete neighbor operators and their bases
    # This includes:
    # - NeighborSamplerBase
    # - DeterministicNeighborSample
    # - RandomNeighborSample
    # - NeighborPipeline
    # - filters, predicates, ordering, grouping, and scalars
    # --------------------------------------------------------

    (Join-Path $CoreRoot "Neighbors\Operators"),

    # --------------------------------------------------------
    # DISCOVERIES: contracts and reusable bases
    # --------------------------------------------------------

    (Join-Path $DiscoveriesRoot "Abstractions"),
    (Join-Path $DiscoveriesRoot "Base"),

    # --------------------------------------------------------
    # DISCOVERIES: concrete production surfaces
    # --------------------------------------------------------

    (Join-Path $DiscoveriesRoot "Attributes"),
    (Join-Path $DiscoveriesRoot "Discoveries"),
    (Join-Path $DiscoveriesRoot "Entities"),
    (Join-Path $DiscoveriesRoot "Evidences"),
    (Join-Path $DiscoveriesRoot "Facts"),
    (Join-Path $DiscoveriesRoot "Proofs"),
    (Join-Path $DiscoveriesRoot "Vectors"),

    # --------------------------------------------------------
    # DISCOVERIES: policy family
    # Required because H may need to distinguish existing
    # contribution policy from a new topographical reducer.
    # --------------------------------------------------------

    (Join-Path $DiscoveriesRoot "Policies"),

    # --------------------------------------------------------
    # TESTS: E7 fixtures
    # --------------------------------------------------------

    (Join-Path $TestsRoot "Entities\Fixtures"),

    # --------------------------------------------------------
    # TESTS: existing E7 A-G certification source
    # Required to avoid proposing H behavior already certified.
    # --------------------------------------------------------

    (Join-Path $TestsRoot "Entities\Certifications\E7A"),
    (Join-Path $TestsRoot "Entities\Certifications\E7B"),
    (Join-Path $TestsRoot "Entities\Certifications\E7C"),
    (Join-Path $TestsRoot "Entities\Certifications\E7D"),
    (Join-Path $TestsRoot "Entities\Certifications\E7E"),
    (Join-Path $TestsRoot "Entities\Certifications\E7F"),
    (Join-Path $TestsRoot "Entities\Certifications\E7G"),

    # --------------------------------------------------------
    # TESTS: existing topology certification/tests
    # --------------------------------------------------------

    (Join-Path $TestsRoot "Neighbors\NeighborTopologyTests.cs"),

    # --------------------------------------------------------
    # TESTS: topology fixture and component fixture
    # --------------------------------------------------------

    (Join-Path $TestsRoot "Certification\Fixtures\NeighborTopologyFixture.cs"),
    (Join-Path $TestsRoot "Fixtures\TestComponent.cs"),
    (Join-Path $TestsRoot "Fixtures\TestTopologyObject.cs")
)

# ============================================================
# VALIDATE REQUESTED PATHS
#
# Missing paths are reported explicitly rather than silently
# ignored. The crawl continues with all paths that do exist.
# ============================================================

$ExistingPaths = @()
$MissingPaths = @()

foreach ($RequestedPath in $RequestedPaths)
{
    if (Test-Path -LiteralPath $RequestedPath)
    {
        $ExistingPaths += $RequestedPath
    }
    else
    {
        $MissingPaths += $RequestedPath
    }
}

# ============================================================
# BUILD EXACT FILE SET
# ============================================================

$Files = @()

foreach ($ExistingPath in $ExistingPaths)
{
    $Item =
    Get-Item `
            -LiteralPath $ExistingPath

    if ($Item.PSIsContainer)
    {
        $Files +=
        Get-ChildItem `
            -LiteralPath $Item.FullName `
            -Recurse `
            -File `
            -Filter *.cs
    }
    elseif ($Item.Extension -ieq ".cs")
    {
        $Files += $Item
    }
}

$Files =
$Files |
        Where-Object {
            $_.FullName -notmatch "\\bin\\" -and
                    $_.FullName -notmatch "\\obj\\" -and
                    $_.FullName -notmatch "\\Audits?\\Reports\\" -and
                    $_.FullName -notmatch "\\Audit\\Backups\\"
        } |
        Sort-Object FullName -Unique

# ============================================================
# HELPERS
# ============================================================

function Get-RelativePath
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ($Path.StartsWith(
            $RepoRoot,
            [System.StringComparison]::OrdinalIgnoreCase))
    {
        return $Path.
                Substring($RepoRoot.Length).
                TrimStart("\")
    }

    return $Path
}

function Write-Line
{
    param(
        [string]$Text = ""
    )

    $Text |
            Tee-Object `
        -FilePath $Report `
        -Append
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

function Write-Matches
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Title,

        [Parameter(Mandatory = $true)]
        [string]$Pattern
    )

    Write-Line ""
    Write-Line "------------------------------------------------------------"
    Write-Line $Title
    Write-Line "------------------------------------------------------------"

    $Hits =
    $Files |
            Select-String `
        -Pattern $Pattern `
        -CaseSensitive:$false

    if (-not $Hits)
    {
        Write-Line "<none>"
        return
    }

    foreach ($Hit in $Hits)
    {
        $Relative =
        Get-RelativePath $Hit.Path

        Write-Line (
        "{0}:{1}: {2}" -f
        $Relative,
        $Hit.LineNumber,
        $Hit.Line.Trim()
        )
    }
}

function Get-ProjectName
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ($Path.StartsWith(
            $CoreRoot,
            [System.StringComparison]::OrdinalIgnoreCase))
    {
        return "WildsEngine.Core"
    }

    if ($Path.StartsWith(
            $DiscoveriesRoot,
            [System.StringComparison]::OrdinalIgnoreCase))
    {
        return "WildsEngine.Discoveries"
    }

    if ($Path.StartsWith(
            $TestsRoot,
            [System.StringComparison]::OrdinalIgnoreCase))
    {
        return "WildsEngine.Core.Tests"
    }

    return "<unknown>"
}

# ============================================================
# HEADER
# ============================================================

Write-Section "WILDS E7-H CROSS-PROJECT CONTEXT CRAWL"

Write-Line "Repo root: $RepoRoot"
Write-Line "Core root: $CoreRoot"
Write-Line "Discoveries root: $DiscoveriesRoot"
Write-Line "Tests root: $TestsRoot"
Write-Line "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "Selected C# source files: $($Files.Count)"

# ============================================================
# REQUESTED PATH VALIDATION
# ============================================================

Write-Section "REQUESTED PATH VALIDATION"

Write-Line "Existing requested paths: $($ExistingPaths.Count)"
Write-Line "Missing requested paths: $($MissingPaths.Count)"

Write-Line ""
Write-Line "Existing paths:"

foreach ($ExistingPath in $ExistingPaths)
{
    Write-Line "[FOUND] $(Get-RelativePath $ExistingPath)"
}

Write-Line ""
Write-Line "Missing paths:"

if ($MissingPaths.Count -eq 0)
{
    Write-Line "<none>"
}
else
{
    foreach ($MissingPath in $MissingPaths)
    {
        Write-Line "[MISSING] $(Get-RelativePath $MissingPath)"
    }
}

# ============================================================
# PROJECT COUNTS
# ============================================================

Write-Section "PROJECT FILE COUNTS"

$ProjectGroups =
$Files |
        Group-Object {
            Get-ProjectName $_.FullName
        } |
        Sort-Object Name

foreach ($ProjectGroup in $ProjectGroups)
{
    Write-Line (
    "{0}: {1}" -f
    $ProjectGroup.Name,
    $ProjectGroup.Count
    )
}

# ============================================================
# SOURCE INVENTORY
# ============================================================

Write-Section "SOURCE INVENTORY"

foreach ($File in $Files)
{
    Write-Line (
    "[{0}] {1}" -f
    (Get-ProjectName $File.FullName),
    (Get-RelativePath $File.FullName)
    )
}

# ============================================================
# TYPE INVENTORY
#
# This is intentionally broad enough to catch public classes,
# interfaces, structs, enums, and records.
# ============================================================

Write-Section "TYPE INVENTORY"

$Types = @()

$TypePattern =
"public\s+" +
        "(?:(?:abstract|sealed|static|readonly|partial)\s+)*" +
        "(?:class|interface|struct|enum|record)\s+" +
        "([A-Za-z_][A-Za-z0-9_]*)"

foreach ($File in $Files)
{
    $Relative =
    Get-RelativePath $File.FullName

    $Matches =
    Select-String `
        -Path $File.FullName `
        -Pattern $TypePattern `
        -AllMatches

    foreach ($MatchSet in $Matches)
    {
        foreach ($Match in $MatchSet.Matches)
        {
            $TypeName =
            $Match.Groups[1].Value

            $Types +=
            [pscustomobject]@{
                Type = $TypeName
                File = $Relative
                Project = Get-ProjectName $File.FullName
            }

            Write-Line (
            "[{0}] {1} -> {2}" -f
            (Get-ProjectName $File.FullName),
            $TypeName,
            $Relative
            )
        }
    }
}

# ============================================================
# DUPLICATE TYPE CHECK
#
# Cross-project duplicate names are reported for inspection.
# They are not automatically treated as defects.
# ============================================================

Write-Section "DUPLICATE PUBLIC TYPE NAMES"

$DuplicateTypes =
$Types |
        Group-Object Type |
        Where-Object {
            $_.Count -gt 1
        }

if (-not $DuplicateTypes)
{
    Write-Line "[PASS] No duplicate public type names in selected source."
}
else
{
    foreach ($DuplicateType in $DuplicateTypes)
    {
        Write-Line ""
        Write-Line "[DUPLICATE NAME] $($DuplicateType.Name)"

        foreach ($Entry in $DuplicateType.Group)
        {
            Write-Line (
            "  [{0}] {1}" -f
            $Entry.Project,
            $Entry.File
            )
        }
    }
}

# ============================================================
# INHERITANCE / IMPLEMENTATION MAP
# ============================================================

Write-Section "INHERITANCE AND IMPLEMENTATION DECLARATIONS"

Write-Matches `
    "Type declarations with base/interface lists" `
    "public\s+(?:(?:abstract|sealed|static|readonly|partial)\s+)*(?:class|interface|struct|record)\s+[A-Za-z_][A-Za-z0-9_]*(?:<[^>{}]+>)?\s*:"

# ============================================================
# CORE OBJECT FOUNDATION
# ============================================================

Write-Section "CORE OBJECT FOUNDATION"

Write-Matches `
    "IBaseObject and BaseObject" `
    "\bIBaseObject\b|\bBaseObject\b"

Write-Matches `
    "IComponent and component ownership" `
    "\bIComponent\b|\bComponent\b|_component"

Write-Matches `
    "Neighbor collections and relationships" `
    "INeighborCollection|NeighborCollection|INeighborRelationship|NeighborRelationship"

Write-Matches `
    "Neighbor membership" `
    "\bMembership\b|\bmembership\b"

Write-Matches `
    "Neighbor distance" `
    "NeighborDistance|\bDistance\b|\bdistance\b"

# ============================================================
# NEIGHBOR OPERATOR CONTRACTS
# ============================================================

Write-Section "NEIGHBOR OPERATOR CONTRACTS"

Write-Matches `
    "Generic operator contract" `
    "\bINeighborOperator\b"

Write-Matches `
    "Sequence operator contract" `
    "\bINeighborSequenceOperator\b"

Write-Matches `
    "Predicate contract" `
    "\bINeighborPredicate\b"

Write-Matches `
    "Sampler contract" `
    "\bINeighborSampler\b"

Write-Matches `
    "Grouping contract" `
    "\bINeighborGroupingOperator\b"

Write-Matches `
    "Scalar contract" `
    "\bINeighborScalarOperator\b"

Write-Matches `
    "Comparer contract" `
    "\bINeighborComparer\b"

# ============================================================
# NEIGHBOR OPERATOR BASE CLASSES
# ============================================================

Write-Section "NEIGHBOR OPERATOR BASE CLASSES"

Write-Matches `
    "NeighborOperatorBase" `
    "\bNeighborOperatorBase\b"

Write-Matches `
    "NeighborCompositeOperatorBase" `
    "\bNeighborCompositeOperatorBase\b"

Write-Matches `
    "NeighborFilterOperatorBase" `
    "\bNeighborFilterOperatorBase\b"

Write-Matches `
    "NeighborGroupingOperatorBase" `
    "\bNeighborGroupingOperatorBase\b"

Write-Matches `
    "NeighborOrderOperatorBase" `
    "\bNeighborOrderOperatorBase\b"

Write-Matches `
    "NeighborPredicateBase" `
    "\bNeighborPredicateBase\b"

Write-Matches `
    "NeighborSamplerBase" `
    "\bNeighborSamplerBase\b"

Write-Matches `
    "NeighborSequenceOperatorBase" `
    "\bNeighborSequenceOperatorBase\b"

# ============================================================
# CONCRETE SAMPLING
# ============================================================

Write-Section "CONCRETE NEIGHBOR SAMPLING"

Write-Matches `
    "DeterministicNeighborSample" `
    "\bDeterministicNeighborSample\b"

Write-Matches `
    "RandomNeighborSample" `
    "\bRandomNeighborSample\b"

Write-Matches `
    "Sampling method declarations and calls" `
    "\bSample\s*\(|\bSelect\s*\(|\bApply\s*\("

Write-Matches `
    "Sampling count and limits" `
    "\bCount\b|\bcount\b|\btake\b|\bTake\b"

Write-Matches `
    "Randomness or seeds" `
    "\bRandom\b|\brandom\b|\bseed\b|\bSeed\b"

# ============================================================
# SELECTION / REDUCTION OPERATORS
# ============================================================

Write-Section "SELECTION AND REDUCTION OPERATORS"

Write-Matches `
    "Filters" `
    "AdjacentNeighborFilter|MembershipNeighborFilter|NeighborDistanceRangeFilter|NeighborWithinDistanceFilter"

Write-Matches `
    "Predicates" `
    "AdjacentNeighborPredicate|DistanceRangeNeighborPredicate|MaximumDistanceNeighborPredicate|MembershipNeighborPredicate|MinimumDistanceNeighborPredicate"

Write-Matches `
    "Ordering" `
    "FarthestNeighborOrder|MembershipNeighborOrder|NearestNeighborOrder"

Write-Matches `
    "Grouping" `
    "NeighborDistanceBandGroups|NeighborMembershipGroups"

Write-Matches `
    "Scalar reduction" `
    "AverageNeighborDistance|MaximumNeighborDistance|MinimumNeighborDistance|NeighborCount"

Write-Matches `
    "Single/multiple neighbor selection" `
    "FarthestNeighbor|FarthestNeighbors|NearestNeighbor|NearestNeighbors"

Write-Matches `
    "Neighbor pipeline" `
    "\bNeighborPipeline\b"

# ============================================================
# COLLECTION / SNAPSHOT BEHAVIOR
# ============================================================

Write-Section "COLLECTION AND SNAPSHOT BEHAVIOR"

Write-Matches `
    "Read-only collection contracts" `
    "IReadOnlyList|IReadOnlyCollection|IReadOnlyDictionary"

Write-Matches `
    "Collection copies" `
    "new\s+List<|new\s+Dictionary<|new\s+ReadOnlyCollection<"

Write-Matches `
    "Array or list snapshots" `
    "ToArray\s*\(|ToList\s*\(|Snapshot|snapshot"

Write-Matches `
    "Potential source mutation" `
    "\.Add\s*\(|\.Remove\s*\(|\.Clear\s*\(|\.Sort\s*\("

# ============================================================
# DISCOVERIES FOUNDATION
# ============================================================

Write-Section "DISCOVERIES FOUNDATION"

Write-Matches `
    "Entity contracts and bases" `
    "\bIEntity\b|\bEntityBase\b"

Write-Matches `
    "Evidence contracts and bases" `
    "\bIDiscoveryEvidence\b|\bEvidenceBase\b"

Write-Matches `
    "Fact contracts and bases" `
    "\bIFact\b|\bFactBase\b"

Write-Matches `
    "Discovery contracts and bases" `
    "\bIDiscovery\b|\bDiscoveryBase\b"

Write-Matches `
    "Resolution thresholds" `
    "IResolutionThreshold|ResolutionThresholdBase"

Write-Matches `
    "Proof contracts and bases" `
    "\bIProof\b|\bIProofResult\b|\bProofBase\b|\bProofResult\b"

# ============================================================
# DISCOVERY PROJECTIONS INTO WILDS
# ============================================================

Write-Section "DISCOVERY PROJECTIONS INTO WILDS"

Write-Matches `
    "Generic projection" `
    "IWildsProjection|WildsProjectionBase"

Write-Matches `
    "Entity projection" `
    "IWildsEntity|WildsEntityBase|\bWildsEntity\b"

Write-Matches `
    "Discovery projection" `
    "IWildsDiscovery|WildsDiscoveryBase|\bWildsDiscovery\b"

Write-Matches `
    "Fact projection" `
    "IWildsFact|WildsFactBase|\bWildsFact\b"

Write-Matches `
    "Evidence projection" `
    "IWildsEvidence|WildsEvidenceBase|\bWildsEvidence\b"

Write-Matches `
    "Exact source retention" `
    "\bSource\b|ReferenceEquals"

# ============================================================
# TOPOGRAPHICAL DISCOVERY CLUES
#
# These searches locate existing composition points. They do
# not declare that any such model already exists.
# ============================================================

Write-Section "TOPOGRAPHICAL DISCOVERY CLUES"

Write-Matches `
    "Discovery facts and contributors" `
    "\bFacts\b|\bContributors\b|\bCurrentFactCount\b|\bRequiredFactCount\b"

Write-Matches `
    "Entity facts and discoveries" `
    "IReadOnlyList<IFact>|IReadOnlyList<IDiscovery>|AddFact|AddDiscovery"

Write-Matches `
    "Evidence neighbor context" `
    "\bNeighbors\b|MinimumNeighborCountPolicy"

Write-Matches `
    "Wilds projections as neighbor-capable objects" `
    "IWildsProjection|BaseObject|IBaseObject"

Write-Matches `
    "Commonality or aggregation vocabulary" `
    "Common|Intersect|Intersection|Union|Distinct|GroupBy|Aggregate|Consensus|Shared"

Write-Matches `
    "Consumer keys and values" `
    "\bName\b|\bValue\b|\bExternalId\b|\bConditions\b|\bOutcome\b"

Write-Matches `
    "Variable or range vocabulary" `
    "Minimum|Maximum|Range|Any|All|Not"

# ============================================================
# DISCOVERIES / EMISSION BRIDGE
# ============================================================

Write-Section "DISCOVERIES / EMISSION BRIDGE"

Write-Matches `
    "EntityEmissionDataSource" `
    "\bEntityEmissionDataSource\b"

Write-Matches `
    "IEmissionDataSource usage" `
    "\bIEmissionDataSource\b"

Write-Matches `
    "Fact exposure" `
    "ExposeProperty|ExposeGroupProperty|TryGetProperty|TryGetGroup"

# ============================================================
# EXISTING E7 COVERAGE
# ============================================================

Write-Section "EXISTING E7 A-G COVERAGE"

Write-Matches `
    "E7 certification test classes" `
    "Entity[A-Za-z0-9_]+CertificationTests"

Write-Matches `
    "E7 readable test classes" `
    "Entity[A-Za-z0-9_]+ReadableTests"

Write-Matches `
    "Native surface coverage" `
    "NativeSurface"

Write-Matches `
    "Evidence integrity coverage" `
    "EvidenceIntegrity|EntityEvidence"

Write-Matches `
    "Proof coverage" `
    "EntityProof"

Write-Matches `
    "Policy coverage" `
    "EntityFactPolicy|EntityProductionPolicy"

Write-Matches `
    "Fact resolution coverage" `
    "EntityFactResolution"

Write-Matches `
    "Divergent belief coverage" `
    "EntityDivergentBelief"

Write-Matches `
    "Discovery resolution coverage" `
    "EntityDiscoveryResolution"

# ============================================================
# EXISTING TOPOLOGY TEST COVERAGE
# ============================================================

Write-Section "EXISTING TOPOLOGY TEST COVERAGE"

Write-Matches `
    "Topology test classes" `
    "NeighborTopologyTests|NeighborTopologyFixture"

Write-Matches `
    "Sampling tests" `
    "DeterministicNeighborSample|RandomNeighborSample|INeighborSampler"

Write-Matches `
    "Pipeline tests" `
    "NeighborPipeline"

# ============================================================
# NAMESPACE AUDIT
# ============================================================

Write-Section "NAMESPACE AUDIT"

foreach ($File in $Files)
{
    $Relative =
    Get-RelativePath $File.FullName

    $Namespace =
    Select-String `
        -Path $File.FullName `
        -Pattern "^\s*namespace\s+([A-Za-z0-9_.]+)" |
            Select-Object `
        -First 1

    if ($Namespace)
    {
        Write-Line (
        "{0} -> {1}" -f
        $Relative,
        $Namespace.Matches[0].Groups[1].Value
        )
    }
    else
    {
        Write-Line (
        "[NO NAMESPACE] {0}" -f
        $Relative
        )
    }
}

# ============================================================
# FULL SOURCE
# ============================================================

Write-Section "FULL SOURCE"

foreach ($File in $Files)
{
    $Relative =
    Get-RelativePath $File.FullName

    Write-Line ""
    Write-Line "------------------------------------------------------------"
    Write-Line "PROJECT: $(Get-ProjectName $File.FullName)"
    Write-Line "FILE: $Relative"
    Write-Line "------------------------------------------------------------"

    Get-Content `
        -LiteralPath $File.FullName |
            ForEach-Object {
                Write-Line $_
            }
}

# ============================================================
# COMPLETE
# ============================================================

Write-Section "CRAWL COMPLETE"

Write-Line "Repo root: $RepoRoot"
Write-Line "Selected files: $($Files.Count)"
Write-Line "Missing requested paths: $($MissingPaths.Count)"
Write-Line "Report: $Report"
Write-Line "Latest: $Latest"

Copy-Item `
    -LiteralPath $Report `
    -Destination $Latest `
    -Force

Write-Host ""
Write-Host "============================================================"
Write-Host " WILDS E7-H CONTEXT CRAWL COMPLETE"
Write-Host "============================================================"
Write-Host "Repo    : $RepoRoot"
Write-Host "Files   : $($Files.Count)"
Write-Host "Missing : $($MissingPaths.Count)"
Write-Host "Report  : $Report"
Write-Host "Latest  : $Latest"
Write-Host "============================================================"

if ($MissingPaths.Count -gt 0)
{
    Write-Host ""
    Write-Host "Review the REQUESTED PATH VALIDATION section."
    Write-Host "The crawl completed with the paths that exist."
}