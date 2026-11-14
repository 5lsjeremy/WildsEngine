$ErrorActionPreference = "Stop"

# ============================================================
# PATHS
#
# Expected script location:
# <repo>\WildsEngine.Core\Abstractions\Invoke-Abstractions-Crawl.ps1
#
# Script location is authoritative. Rider's current working
# directory does not matter.
# ============================================================

$AbstractionsRoot = $PSScriptRoot
$CoreRoot = Split-Path -Parent $AbstractionsRoot
$Root = Split-Path -Parent $CoreRoot

$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

$ReportDir = Join-Path $AbstractionsRoot "Audit\Reports"
$Report = Join-Path $ReportDir "Wilds-Core-Abstractions-Crawl-$Timestamp.txt"
$Latest = Join-Path $ReportDir "Wilds-Core-Abstractions-Crawl-Latest.txt"

New-Item `
    -ItemType Directory `
    -Path $ReportDir `
    -Force |
        Out-Null

# ============================================================
# SOURCE SCOPE
#
# Capture the contracts plus the Core implementation areas that
# exercise topology, sampling, propagation, eligibility,
# emissions, state, runtime objects, and units.
# ============================================================

$SourceRoots = @(
    (Join-Path $CoreRoot "Abstractions"),
    (Join-Path $CoreRoot "Neighbors"),
    (Join-Path $CoreRoot "Topology"),
    (Join-Path $CoreRoot "Emission"),
    (Join-Path $CoreRoot "Objects"),
    (Join-Path $CoreRoot "States"),
    (Join-Path $CoreRoot "Contexts"),
    (Join-Path $CoreRoot "Units")
) |
        Where-Object {
            Test-Path $_
        }

$Files =
$SourceRoots |
        ForEach-Object {
            Get-ChildItem `
        -Path $_ `
        -Recurse `
        -File `
        -Filter *.cs
        } |
        Where-Object {
            $_.FullName -notmatch "\\bin\\" -and
                    $_.FullName -notmatch "\\obj\\" -and
                    $_.FullName -notmatch "\\Audit\\"
        } |
        Sort-Object FullName -Unique

# ============================================================
# HELPERS
# ============================================================

function Get-RelativePath
{
    param(
        [string]$Path
    )

    return $Path.
            Substring($CoreRoot.Length).
            TrimStart("\")
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
        [string]$Title,
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
        $Relative = Get-RelativePath $Hit.Path

        Write-Line (
        "{0}:{1}: {2}" -f
        $Relative,
        $Hit.LineNumber,
        $Hit.Line.Trim()
        )
    }
}

# ============================================================
# HEADER
# ============================================================

Write-Section "WILDS CORE ABSTRACTIONS / TOPOLOGY CRAWL"

Write-Line "Repo root: $Root"
Write-Line "Core root: $CoreRoot"
Write-Line "Abstractions root: $AbstractionsRoot"
Write-Line "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "C# source files: $($Files.Count)"

Write-Line ""
Write-Line "Source roots:"

foreach ($SourceRoot in $SourceRoots)
{
    Write-Line "  $(Get-RelativePath $SourceRoot)"
}

# ============================================================
# SOURCE INVENTORY
# ============================================================

Write-Section "SOURCE INVENTORY"

foreach ($File in $Files)
{
    Write-Line (Get-RelativePath $File.FullName)
}

# ============================================================
# TYPE INVENTORY
# ============================================================

Write-Section "TYPE INVENTORY"

$Types = @()

foreach ($File in $Files)
{
    $Relative = Get-RelativePath $File.FullName

    $Matches =
    Select-String `
        -Path $File.FullName `
        -Pattern "public\s+(?:abstract\s+|sealed\s+|static\s+)?(?:class|interface|struct|enum)\s+([A-Za-z0-9_]+)" `
        -AllMatches

    foreach ($MatchSet in $Matches)
    {
        foreach ($Match in $MatchSet.Matches)
        {
            $TypeName = $Match.Groups[1].Value

            $Types +=
            [pscustomobject]@{
                Type = $TypeName
                File = $Relative
            }

            Write-Line (
            "{0} -> {1}" -f
            $TypeName,
            $Relative
            )
        }
    }
}

# ============================================================
# DUPLICATE PUBLIC TYPES
# ============================================================

Write-Section "DUPLICATE TYPE CHECK"

$Duplicates =
$Types |
        Group-Object Type |
        Where-Object {
            $_.Count -gt 1
        }

if (-not $Duplicates)
{
    Write-Line "[PASS] No duplicate public type names."
}
else
{
    foreach ($Duplicate in $Duplicates)
    {
        Write-Line ""
        Write-Line "[DUPLICATE] $($Duplicate.Name)"

        foreach ($Entry in $Duplicate.Group)
        {
            Write-Line "  $($Entry.File)"
        }
    }
}

# ============================================================
# BASE OBJECT / COMPONENT MODEL
# ============================================================

Write-Section "BASE OBJECT / COMPONENT MODEL"

Write-Matches `
    "Base object contracts and implementations" `
    "\bIBaseObject\b|\bBaseObject\b"

Write-Matches `
    "Component contracts and implementations" `
    "\bIComponent\b|\bBaseComponent\b"

Write-Matches `
    "Component binding" `
    "\bComponent\b|_component"

Write-Matches `
    "Runtime identity" `
    "\bGuid\b|\bId\b|\bLayer\b"

# ============================================================
# NEIGHBOR RELATIONSHIP MODEL
# ============================================================

Write-Section "NEIGHBOR RELATIONSHIP MODEL"

Write-Matches `
    "Neighbor contracts" `
    "INeighborCollection|INeighborRelationship|NeighborCollection|NeighborRelationship"

Write-Matches `
    "Membership" `
    "\bMembership\b|\bmembership\b"

Write-Matches `
    "Distance" `
    "NeighborDistance|\bDistance\b|\bdistance\b"

Write-Matches `
    "Relationship mutation" `
    "AddNeighbor|RemoveNeighbor|SetNeighborDistance"

# ============================================================
# TOPOLOGY OPERATORS
# ============================================================

Write-Section "TOPOLOGY OPERATORS"

Write-Matches `
    "Generic neighbor operators" `
    "INeighborOperator|NeighborOperatorBase|NeighborCompositeOperatorBase"

Write-Matches `
    "Predicates" `
    "INeighborPredicate|NeighborPredicateBase|Predicate"

Write-Matches `
    "Filters" `
    "Filter|NeighborFilterOperatorBase"

Write-Matches `
    "Ordering" `
    "Order|Comparer|NeighborOrderOperatorBase"

Write-Matches `
    "Grouping" `
    "Group|INeighborGroupingOperator|NeighborGroupingOperatorBase"

Write-Matches `
    "Scalar reduction" `
    "INeighborScalarOperator|NeighborCount|AverageNeighborDistance|MinimumNeighborDistance|MaximumNeighborDistance"

Write-Matches `
    "Pipelines" `
    "NeighborPipeline|Pipeline"

# ============================================================
# SAMPLING
# ============================================================

Write-Section "NEIGHBOR SAMPLING"

Write-Matches `
    "Sampling contracts" `
    "INeighborSampler|NeighborSamplerBase"

Write-Matches `
    "Deterministic sampling" `
    "DeterministicNeighborSample"

Write-Matches `
    "Random sampling" `
    "RandomNeighborSample"

Write-Matches `
    "Sampling mechanics" `
    "\bSample\b|\bsample\b|Take\(|Random|OrderBy|Count"

# ============================================================
# TOPOLOGY ROUTING / TRAVERSAL
# ============================================================

Write-Section "TOPOLOGY ROUTING / TRAVERSAL"

Write-Matches `
    "Traversal" `
    "INeighborTraversal|NeighborTraversal"

Write-Matches `
    "Routing" `
    "INeighborRoute|INeighborRouter|NeighborRoute|NeighborRouter"

Write-Matches `
    "Clustering" `
    "INeighborCluster|INeighborClusterer|NeighborCluster|NeighborClusterer"

Write-Matches `
    "Propagation topology" `
    "INeighborPropagation|INeighborPropagationRule|NeighborPropagation"

# ============================================================
# PROPAGATION
# ============================================================

Write-Section "PROPAGATION"

Write-Matches `
    "Neighbor propagation contracts" `
    "INeighborPropogation|INeighborPropagation|ExponentialDistancePropagation"

Write-Matches `
    "Exponential attenuation" `
    "Exp\(|exponential|decay"

Write-Matches `
    "Propagation values" `
    "sourceIntensity|received|intensity|Value"

# ============================================================
# ELIGIBILITY
# ============================================================

Write-Section "ELIGIBILITY"

Write-Matches `
    "Eligibility contracts" `
    "INeighborPropagationEligibility|IEmissionEligibilityResult|IEmissionEligibilityVector"

Write-Matches `
    "Minimum intensity eligibility" `
    "MinimumIntensityEligibility"

Write-Matches `
    "Emission eligibility" `
    "EmissionEligibility|EmissionVectorEligibility"

Write-Matches `
    "Eligibility decisions" `
    "Eligible|Eligibility|threshold"

# ============================================================
# EMISSION RESOLUTION
# ============================================================

Write-Section "EMISSION RESOLUTION"

Write-Matches `
    "Emitter contracts" `
    "\bIEmitter\b|\bEmitterBase\b"

Write-Matches `
    "Emission data source" `
    "IEmissionDataSource"

Write-Matches `
    "Resolved emission" `
    "IResolvedEmission|ResolvedEmission"

Write-Matches `
    "Resolved emission vectors" `
    "IResolvedEmissionVector|ResolvedEmissionVector"

Write-Matches `
    "Normalization" `
    "IEmissionNormalizationPolicy|NormalizedEmission|ProportionalEmissionNormalization"

Write-Matches `
    "Propagation carriers" `
    "IPropagatedEmission|IPropagatedEmissionVector|PropagatedEmission|PropogatedEmission"

Write-Matches `
    "Composition" `
    "IEmissionComposition|EmissionCompositionBase"

# ============================================================
# STATE BOUNDARY
# ============================================================

Write-Section "STATE / INSTRUCTION BOUNDARY"

Write-Matches `
    "State contracts" `
    "\bIState\b|\bWildsState\b"

Write-Matches `
    "State instructions" `
    "IStateInstruction|StateInstructionBase|EmissionStateInstruction"

Write-Matches `
    "Context" `
    "\bIContext\b|\bWildsContext\b"

Write-Matches `
    "Metadata" `
    "Metadata|SetComponentMetadata"

# ============================================================
# UNIT / PROVENANCE SURFACES
# ============================================================

Write-Section "UNIT / PROVENANCE SURFACES"

Write-Matches `
    "Base units" `
    "IBaseUnit|BaseUnit"

Write-Matches `
    "Pulse and work units" `
    "\bIPulse\b|\bPulse\b|IWorkUnit|WorkUnit"

Write-Matches `
    "Moment / timeline / epoch" `
    "IMoment|\bMoment\b|ITimeline|\bTimeline\b|IEpoch|\bEpoch\b"

Write-Matches `
    "Source references" `
    "\bSource\b|\bsource\b|Provenance"

# ============================================================
# DISCOVERY-COMPOSITION CLUES
#
# These searches do not assume that Core knows Discoveries.
# They identify generic extension points that Discoveries may
# compose with from its own project.
# ============================================================

Write-Section "DISCOVERY COMPOSITION CLUES"

Write-Matches `
    "Generic selection surfaces" `
    "INeighborSampler|INeighborPredicate|INeighborSequenceOperator|INeighborGroupingOperator"

Write-Matches `
    "Generic scalar surfaces" `
    "INeighborScalarOperator|IEmissionScalar"

Write-Matches `
    "Generic data-source surfaces" `
    "IEmissionDataSource"

Write-Matches `
    "Generic processable-object surfaces" `
    "IBaseObject|IComponent"

Write-Matches `
    "Read-only / snapshot surfaces" `
    "IReadOnlyList|IReadOnlyCollection|ReadOnlyCollection|ToArray|snapshot"

# ============================================================
# NAMESPACE AUDIT
# ============================================================

Write-Section "NAMESPACE AUDIT"

foreach ($File in $Files)
{
    $Relative = Get-RelativePath $File.FullName

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
# EXPECTED CORE TYPE INVENTORY
# ============================================================

Write-Section "EXPECTED CORE TYPE INVENTORY"

$Expected = @(
    "IBaseObject",
    "IComponent",
    "IContext",
    "IState",

    "INeighborCollection",
    "INeighborRelationship",
    "NeighborDistance",

    "INeighborOperator",
    "INeighborPredicate",
    "INeighborSampler",
    "INeighborScalarOperator",
    "INeighborSequenceOperator",
    "INeighborGroupingOperator",

    "NeighborSamplerBase",
    "DeterministicNeighborSample",
    "RandomNeighborSample",
    "NeighborPipeline",

    "INeighborTraversal",
    "INeighborRoute",
    "INeighborRouter",
    "INeighborCluster",
    "INeighborClusterer",

    "IEmissionDataSource",
    "IEmissionComposition",
    "IEmissionScalar",
    "IResolvedEmission",
    "IResolvedEmissionVector",
    "INormalizedEmission",
    "INormalizedEmissionVector",
    "IPropagatedEmission",
    "IPropagatedEmissionVector",
    "IEmissionEligibilityResult",
    "IEmissionEligibilityVector",

    "IBaseUnit",
    "IPulse",
    "IWorkUnit",
    "IMoment",
    "ITimeline",
    "IEpoch"
)

foreach ($ExpectedType in $Expected)
{
    $Found =
    $Types |
            Where-Object {
                $_.Type -eq $ExpectedType
            }

    if ($Found)
    {
        Write-Line "[FOUND]   $ExpectedType"
    }
    else
    {
        Write-Line "[MISSING] $ExpectedType"
    }
}

# ============================================================
# FULL SOURCE
# ============================================================

Write-Section "FULL SOURCE"

foreach ($File in $Files)
{
    $Relative = Get-RelativePath $File.FullName

    Write-Line ""
    Write-Line "------------------------------------------------------------"
    Write-Line "FILE: $Relative"
    Write-Line "------------------------------------------------------------"

    Get-Content `
        -Path $File.FullName |
            ForEach-Object {
                Write-Line $_
            }
}

# ============================================================
# COMPLETE
# ============================================================

Write-Section "CRAWL COMPLETE"

Write-Line "Repo root: $Root"
Write-Line "Core root: $CoreRoot"
Write-Line "Abstractions root: $AbstractionsRoot"
Write-Line "Report: $Report"
Write-Line "Latest: $Latest"

Copy-Item `
    -Path $Report `
    -Destination $Latest `
    -Force

Write-Host ""
Write-Host "============================================================"
Write-Host " WILDS CORE ABSTRACTIONS / TOPOLOGY CRAWL COMPLETE"
Write-Host "============================================================"
Write-Host "Repo   : $Root"
Write-Host "Core   : $CoreRoot"
Write-Host "Files  : $($Files.Count)"
Write-Host "Report : $Report"
Write-Host "Latest : $Latest"
Write-Host "============================================================"