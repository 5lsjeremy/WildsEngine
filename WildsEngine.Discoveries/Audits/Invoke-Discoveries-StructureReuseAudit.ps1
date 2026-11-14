$ErrorActionPreference = "Stop"

# ============================================================
# DISCOVERIES STRUCTURE-REUSE AUDIT
#
# Expected script location:
# <repo>\WildsEngine.Discoveries\Audits\
#     Invoke-Discoveries-StructureReuseAudit.ps1
#
# Expected invocation location:
# <repo>\WildsEngine.Discoveries
#
# Purpose:
# - Inventory existing Discoveries interfaces and base classes.
# - Inventory the new vector, cluster, truth, and consequence types.
# - Compare them with selected Core abstractions and base objects.
# - Flag likely reinvention, unnecessary identity surfaces,
#   parallel vector models, and types that may belong elsewhere.
# - Produce review candidates, not automatic architectural verdicts.
# ============================================================

$ScriptRoot = $PSScriptRoot
$DiscoveriesRoot = Split-Path -Parent $ScriptRoot
$RepoRoot = Split-Path -Parent $DiscoveriesRoot

$CoreRoot =
Join-Path $RepoRoot "WildsEngine.Core"

$Timestamp =
Get-Date -Format "yyyyMMdd-HHmmss"

$ReportDir =
Join-Path $DiscoveriesRoot "Audits\Reports"

$Report =
Join-Path `
        $ReportDir `
        "Discoveries-Structure-Reuse-Audit-$Timestamp.txt"

$Latest =
Join-Path `
        $ReportDir `
        "Discoveries-Structure-Reuse-Audit-Latest.txt"

New-Item `
    -ItemType Directory `
    -Path $ReportDir `
    -Force |
        Out-Null

# ============================================================
# PATH VALIDATION
# ============================================================

$RequiredPaths = @(
    $DiscoveriesRoot,
    (Join-Path $DiscoveriesRoot "Abstractions"),
    (Join-Path $DiscoveriesRoot "Base"),
    $CoreRoot,
    (Join-Path $CoreRoot "Abstractions"),
    (Join-Path $CoreRoot "Objects")
)

$MissingRequiredPaths =
$RequiredPaths |
        Where-Object {
            -not (Test-Path -LiteralPath $_)
        }

if ($MissingRequiredPaths)
{
    Write-Host ""
    Write-Host "Required paths are missing:" -ForegroundColor Red

    foreach ($MissingPath in $MissingRequiredPaths)
    {
        Write-Host "  $MissingPath" -ForegroundColor Red
    }

    throw "Structure-reuse audit cannot continue."
}

# ============================================================
# EXPLICIT SOURCE SCOPE
#
# Discoveries:
# - Include all production C# source.
#
# Core:
# - Include only contracts and implementations that may reveal
#   existing reusable identity, vector, emission, topology,
#   relationship, component, and base-unit structures.
#
# Do not crawl tests, QSL, processing, registries, pools,
# engine hosts, Console, builders, or generated output.
# ============================================================

$RequestedPaths = @(
# --------------------------------------------------------
# DISCOVERIES: complete production project
# --------------------------------------------------------

    (Join-Path $DiscoveriesRoot "Abstractions"),
    (Join-Path $DiscoveriesRoot "Attributes"),
    (Join-Path $DiscoveriesRoot "Base"),
    (Join-Path $DiscoveriesRoot "Discoveries"),
    (Join-Path $DiscoveriesRoot "Entities"),
    (Join-Path $DiscoveriesRoot "Evidences"),
    (Join-Path $DiscoveriesRoot "Facts"),
    (Join-Path $DiscoveriesRoot "Policies"),
    (Join-Path $DiscoveriesRoot "Proofs"),
    (Join-Path $DiscoveriesRoot "Registries"),

    # --------------------------------------------------------
    # CORE: object and component foundations
    # --------------------------------------------------------

    (Join-Path $CoreRoot "Abstractions\Base"),
    (Join-Path $CoreRoot "Objects\BaseObject.cs"),
    (Join-Path $CoreRoot "Objects\NeighborCollection.cs"),
    (Join-Path $CoreRoot "Objects\NeighborRelationship.cs"),
    (Join-Path $CoreRoot "Components\BaseComponent.cs"),

    # --------------------------------------------------------
    # CORE: vector and emission contracts
    # --------------------------------------------------------

    (Join-Path $CoreRoot "Abstractions\Emissions"),
    (Join-Path $CoreRoot "Emission\EmissionScalar.cs"),
    (Join-Path $CoreRoot "Emission\NormalizedEmissionVector.cs"),
    (Join-Path $CoreRoot "Emission\PropagatedEmissionVector.cs"),
    (Join-Path $CoreRoot "Emission\ResolvedEmissionVector.cs"),
    (Join-Path $CoreRoot "Emission\EmissionEligibilityVector.cs"),
    (Join-Path $CoreRoot "Emission\EmissionVectorPropagation.cs"),
    (Join-Path $CoreRoot "Emission\EmissionVectorEligibility.cs"),

    # --------------------------------------------------------
    # CORE: topology and sampling contracts
    # --------------------------------------------------------

    (Join-Path $CoreRoot "Abstractions\Neighbors\Operators"),
    (Join-Path $CoreRoot "Abstractions\Topology"),
    (Join-Path $CoreRoot "Neighbors\Operators\Base"),
    (Join-Path $CoreRoot "Neighbors\Operators\DeterministicNeighborSample.cs"),
    (Join-Path $CoreRoot "Neighbors\Operators\RandomNeighborSample.cs"),
    (Join-Path $CoreRoot "Neighbors\Operators\NeighborPipeline.cs"),
    (Join-Path $CoreRoot "Topology\NeighborCluster.cs"),
    (Join-Path $CoreRoot "Topology\NeighborClusterer.cs"),

    # --------------------------------------------------------
    # CORE: unit identity/lifecycle foundation
    # --------------------------------------------------------

    (Join-Path $CoreRoot "Abstractions\Units\IBaseUnit.cs"),
    (Join-Path $CoreRoot "Units\BaseUnit.cs")
)

$ExistingPaths = @()
$MissingOptionalPaths = @()

foreach ($RequestedPath in $RequestedPaths)
{
    if (Test-Path -LiteralPath $RequestedPath)
    {
        $ExistingPaths += $RequestedPath
    }
    else
    {
        $MissingOptionalPaths += $RequestedPath
    }
}

# ============================================================
# BUILD EXACT SOURCE SET
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
                    $_.FullName -notmatch "\\Audits?\\Reports\\"
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

function Get-ProjectName
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ($Path.StartsWith(
            $DiscoveriesRoot,
            [System.StringComparison]::OrdinalIgnoreCase))
    {
        return "WildsEngine.Discoveries"
    }

    if ($Path.StartsWith(
            $CoreRoot,
            [System.StringComparison]::OrdinalIgnoreCase))
    {
        return "WildsEngine.Core"
    }

    return "<unknown>"
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
        [string]$Pattern,

        [System.IO.FileInfo[]]$InputFiles = $Files
    )

    Write-Line ""
    Write-Line "------------------------------------------------------------"
    Write-Line $Title
    Write-Line "------------------------------------------------------------"

    $Hits =
    $InputFiles |
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
        Write-Line (
        "[{0}] {1}:{2}: {3}" -f
        (Get-ProjectName $Hit.Path),
        (Get-RelativePath $Hit.Path),
        $Hit.LineNumber,
        $Hit.Line.Trim()
        )
    }
}

function Get-PublicTypes
{
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo[]]$InputFiles
    )

    $Results = @()

    $Pattern =
    "public\s+" +
            "(?:(?:abstract|sealed|static|readonly|partial)\s+)*" +
            "(class|interface|struct|enum|record)\s+" +
            "([A-Za-z_][A-Za-z0-9_]*)"

    foreach ($File in $InputFiles)
    {
        $Matches =
        Select-String `
            -Path $File.FullName `
            -Pattern $Pattern `
            -AllMatches

        foreach ($MatchSet in $Matches)
        {
            foreach ($Match in $MatchSet.Matches)
            {
                $Results +=
                [pscustomobject]@{
                    Kind = $Match.Groups[1].Value
                    Type = $Match.Groups[2].Value
                    File = Get-RelativePath $File.FullName
                    FullPath = $File.FullName
                    Project = Get-ProjectName $File.FullName
                }
            }
        }
    }

    return $Results
}

function Write-Candidate
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Severity,

        [Parameter(Mandatory = $true)]
        [string]$Candidate,

        [Parameter(Mandatory = $true)]
        [string]$Reason,

        [Parameter(Mandatory = $true)]
        [string]$Review
    )

    Write-Line ""
    Write-Line "[$Severity] $Candidate"
    Write-Line "  Reason: $Reason"
    Write-Line "  Review: $Review"
}

# ============================================================
# FILE GROUPS
# ============================================================

$DiscoveriesFiles =
$Files |
        Where-Object {
            $_.FullName.StartsWith(
                    $DiscoveriesRoot,
                    [System.StringComparison]::OrdinalIgnoreCase)
        }

$CoreFiles =
$Files |
        Where-Object {
            $_.FullName.StartsWith(
                    $CoreRoot,
                    [System.StringComparison]::OrdinalIgnoreCase)
        }

$NewHFiles =
$DiscoveriesFiles |
        Where-Object {
            $_.Name -match `
        "DiscoveryVector|DiscoveryCluster|Truth|Consequence"
        }

$ExistingDiscoveriesFiles =
$DiscoveriesFiles |
        Where-Object {
            $_.Name -notmatch `
        "DiscoveryVector|DiscoveryCluster|Truth|Consequence"
        }

# ============================================================
# HEADER
# ============================================================

Write-Section "WILDS DISCOVERIES STRUCTURE-REUSE AUDIT"

Write-Line "Repo root: $RepoRoot"
Write-Line "Discoveries root: $DiscoveriesRoot"
Write-Line "Core root: $CoreRoot"
Write-Line "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "Selected files: $($Files.Count)"
Write-Line "Discoveries files: $($DiscoveriesFiles.Count)"
Write-Line "Core comparison files: $($CoreFiles.Count)"
Write-Line "New H candidate files: $($NewHFiles.Count)"

# ============================================================
# OPTIONAL PATH VALIDATION
# ============================================================

Write-Section "REQUESTED PATH VALIDATION"

Write-Line "Existing requested paths: $($ExistingPaths.Count)"
Write-Line "Missing optional paths: $($MissingOptionalPaths.Count)"

if ($MissingOptionalPaths.Count -eq 0)
{
    Write-Line "[PASS] Every requested comparison path exists."
}
else
{
    foreach ($MissingPath in $MissingOptionalPaths)
    {
        Write-Line (
        "[MISSING OPTIONAL] {0}" -f
        (Get-RelativePath $MissingPath)
        )
    }
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
# PUBLIC TYPE INVENTORY
# ============================================================

$Types =
Get-PublicTypes `
        -InputFiles $Files

$NewHTypes =
Get-PublicTypes `
        -InputFiles $NewHFiles

$ExistingDiscoveriesTypes =
Get-PublicTypes `
        -InputFiles $ExistingDiscoveriesFiles

$CoreTypes =
Get-PublicTypes `
        -InputFiles $CoreFiles

Write-Section "PUBLIC TYPE INVENTORY"

foreach ($Type in $Types)
{
    Write-Line (
    "[{0}] {1} {2} -> {3}" -f
    $Type.Project,
    $Type.Kind,
    $Type.Type,
    $Type.File
    )
}

# ============================================================
# EXACT DUPLICATE TYPE NAMES
# ============================================================

Write-Section "EXACT DUPLICATE PUBLIC TYPE NAMES"

$DuplicateNames =
$Types |
        Group-Object Type |
        Where-Object {
            $_.Count -gt 1
        }

if (-not $DuplicateNames)
{
    Write-Line "[PASS] No exact duplicate public type names."
}
else
{
    foreach ($DuplicateName in $DuplicateNames)
    {
        Write-Line ""
        Write-Line "[DUPLICATE NAME] $($DuplicateName.Name)"

        foreach ($Entry in $DuplicateName.Group)
        {
            Write-Line (
            "  [{0}] {1} {2}" -f
            $Entry.Project,
            $Entry.Kind,
            $Entry.File
            )
        }
    }
}

# ============================================================
# NEW H TYPE INVENTORY
# ============================================================

Write-Section "NEW H TYPE INVENTORY"

if (-not $NewHTypes)
{
    Write-Line "<none>"
}
else
{
    foreach ($Type in $NewHTypes)
    {
        Write-Line (
        "{0} {1} -> {2}" -f
        $Type.Kind,
        $Type.Type,
        $Type.File
        )
    }
}

# ============================================================
# INHERITANCE / IMPLEMENTATION DECLARATIONS
# ============================================================

Write-Section "NEW H INHERITANCE AND IMPLEMENTATION MAP"

Write-Matches `
    -Title "New H declarations with base/interface lists" `
    -Pattern (
"public\s+" +
        "(?:(?:abstract|sealed|static|readonly|partial)\s+)*" +
        "(?:class|interface|struct|record)\s+" +
        "[A-Za-z_][A-Za-z0-9_]*" +
        "(?:<[^>{}]+>)?\s*:"
) `
    -InputFiles $NewHFiles

# ============================================================
# EXISTING DISCOVERIES FOUNDATION
# ============================================================

Write-Section "EXISTING DISCOVERIES FOUNDATION"

Write-Matches `
    -Title "Identifier model" `
    -Pattern "IIdentifier|IdentifierBase|ExternalId|Slug" `
    -InputFiles $ExistingDiscoveriesFiles

Write-Matches `
    -Title "Resolution-threshold model" `
    -Pattern "IResolutionThreshold|ResolutionThresholdBase|RequiredCount|CurrentCount|Percentage|IsResolved" `
    -InputFiles $ExistingDiscoveriesFiles

Write-Matches `
    -Title "Projection model" `
    -Pattern "IWildsProjection|WildsProjectionBase|IWildsEntity|IWildsDiscovery|IWildsFact|IWildsEvidence" `
    -InputFiles $ExistingDiscoveriesFiles

Write-Matches `
    -Title "Authoritative object names and values" `
    -Pattern "\bName\b|\bValue\b|\bExternalId\b" `
    -InputFiles $ExistingDiscoveriesFiles

Write-Matches `
    -Title "Contributor and provenance collections" `
    -Pattern "Contributors|Evidence|Facts|Discoveries|Source|ProofResult|Neighbors" `
    -InputFiles $ExistingDiscoveriesFiles

Write-Matches `
    -Title "Snapshot helpers and immutable collections" `
    -Pattern "Snapshot|ToArray|IReadOnlyList|IReadOnlyDictionary|ReadOnlyCollection" `
    -InputFiles $ExistingDiscoveriesFiles

# ============================================================
# CORE FOUNDATIONS THAT MAY ALREADY SOLVE NEW H NEEDS
# ============================================================

Write-Section "CORE REUSE SURFACES"

Write-Matches `
    -Title "Core runtime identity" `
    -Pattern "\bIBaseObject\b|\bBaseObject\b|\bId\b|\bLayer\b|\bComponent\b" `
    -InputFiles $CoreFiles

Write-Matches `
    -Title "Core base-unit identity and lifetime" `
    -Pattern "\bIBaseUnit\b|\bBaseUnit\b|\bCreatedAt\b|\bCompletedAt\b" `
    -InputFiles $CoreFiles

Write-Matches `
    -Title "Core scalar model" `
    -Pattern "\bIEmissionScalar\b|\bEmissionScalar\b|\bMagnitude\b|\bValue\b" `
    -InputFiles $CoreFiles

Write-Matches `
    -Title "Core vector contracts" `
    -Pattern "EmissionVector|IResolvedEmissionVector|INormalizedEmissionVector|IPropagatedEmissionVector|IEmissionEligibilityVector" `
    -InputFiles $CoreFiles

Write-Matches `
    -Title "Core vector components and dimensions" `
    -Pattern "Components|Dimensions|Dimension|Scalars|Keys|Order" `
    -InputFiles $CoreFiles

Write-Matches `
    -Title "Core vector normalization" `
    -Pattern "Normalize|Normalized|Normalization|Magnitude" `
    -InputFiles $CoreFiles

Write-Matches `
    -Title "Core clustering contracts and implementations" `
    -Pattern "INeighborCluster|INeighborClusterer|NeighborCluster|NeighborClusterer" `
    -InputFiles $CoreFiles

Write-Matches `
    -Title "Core sampling contracts and implementations" `
    -Pattern "INeighborSampler|NeighborSamplerBase|DeterministicNeighborSample|RandomNeighborSample" `
    -InputFiles $CoreFiles

Write-Matches `
    -Title "Core pipeline composition" `
    -Pattern "NeighborPipeline|INeighborSequenceOperator|NeighborCompositeOperatorBase" `
    -InputFiles $CoreFiles

# ============================================================
# NEW H STRUCTURE DETAILS
# ============================================================

Write-Section "NEW H STRUCTURE DETAILS"

Write-Matches `
    -Title "New H identity properties" `
    -Pattern "Guid\s+Id|DateTimeOffset|EvaluatedAt|CongealedAt|ProjectedAt" `
    -InputFiles $NewHFiles

Write-Matches `
    -Title "New H names and generic values" `
    -Pattern "string\s+Name|object\?\s+Value|string\s+Dimension" `
    -InputFiles $NewHFiles

Write-Matches `
    -Title "New H vector quantities" `
    -Pattern "Position|Velocity|Acceleration|Magnitude|Weight" `
    -InputFiles $NewHFiles

Write-Matches `
    -Title "New H geometric quantities" `
    -Pattern "Centroid|Density|Dispersion|Coherence|Stability|Confidence" `
    -InputFiles $NewHFiles

Write-Matches `
    -Title "New H contributor/source collections" `
    -Pattern "Contributions|Members|Clusters|Discoveries|Truths|Fact|Relationship" `
    -InputFiles $NewHFiles

Write-Matches `
    -Title "New H validation helpers" `
    -Pattern "ValidateUnit|ValidateFinite|IsNaN|IsInfinity|ArgumentOutOfRangeException" `
    -InputFiles $NewHFiles

Write-Matches `
    -Title "New H snapshot helpers" `
    -Pattern "Snapshot|new\s+[A-Za-z0-9_<>?]+\[source\.Count\]|IReadOnlyList" `
    -InputFiles $NewHFiles

# ============================================================
# OVERLAP HEURISTICS
#
# These sections intentionally search for structurally similar
# concepts. Similarity is a review signal, not proof of error.
# ============================================================

Write-Section "STRUCTURAL OVERLAP HEURISTICS"

Write-Matches `
    -Title "Parallel identity models" `
    -Pattern "Guid\s+Id|IIdentifier|IdentifierBase|IBaseObject|BaseObject|IBaseUnit|BaseUnit" `
    -InputFiles $Files

Write-Matches `
    -Title "Parallel named-value models" `
    -Pattern "string\s+Name|object\?\s+Value|IEmissionScalar|EmissionScalar|IFact" `
    -InputFiles $Files

Write-Matches `
    -Title "Parallel vector models" `
    -Pattern "IDiscoveryVector|DiscoveryVectorBase|EmissionVector|VectorComponent|Components|Magnitude" `
    -InputFiles $Files

Write-Matches `
    -Title "Parallel clustering models" `
    -Pattern "IDiscoveryCluster|DiscoveryClusterBase|INeighborCluster|NeighborCluster|Clusterer|Centroid" `
    -InputFiles $Files

Write-Matches `
    -Title "Parallel resolution models" `
    -Pattern "ITruth|TruthBase|IResolutionThreshold|ResolutionThresholdBase|IsResolved|Congeal|Stability" `
    -InputFiles $Files

Write-Matches `
    -Title "Parallel policy models" `
    -Pattern "Policy|Contributes|Vectorize|CanCongeal|Congeal|Project" `
    -InputFiles $Files

Write-Matches `
    -Title "Parallel provenance collections" `
    -Pattern "Contributors|Contributions|Evidence|Facts|Discoveries|Clusters|Truths|Source|Relationship" `
    -InputFiles $Files

# ============================================================
# AUTOMATIC REVIEW CANDIDATES
#
# These are intentionally conservative. They surface likely
# redesign areas without modifying source.
# ============================================================

Write-Section "REMAKE / REUSE REVIEW CANDIDATES"

$DiscoveryVectorExists =
$NewHTypes.Type -contains "IDiscoveryVector"

$DiscoveryClusterExists =
$NewHTypes.Type -contains "IDiscoveryCluster"

$TruthExists =
$NewHTypes.Type -contains "ITruth"

$ConsequenceExists =
$NewHTypes.Type -contains "IConsequence"

$CoreVectorTypes =
$CoreTypes |
        Where-Object {
            $_.Type -match "EmissionVector|EmissionScalar"
        }

$CoreClusterTypes =
$CoreTypes |
        Where-Object {
            $_.Type -match "NeighborCluster"
        }

$ExistingIdentityTypes =
$Types |
        Where-Object {
            $_.Type -in @(
                "IIdentifier",
                "IdentifierBase",
                "IBaseObject",
                "BaseObject",
                "IBaseUnit",
                "BaseUnit"
            )
        }

if ($DiscoveryVectorExists -and $CoreVectorTypes)
{
    Write-Candidate `
        -Severity "HIGH REVIEW" `
        -Candidate "IDiscoveryVector / DiscoveryVectorBase" `
        -Reason "Core already contains scalar and vector contracts with magnitude, dimensions/components, immutable carriers, and normalization-related behavior." `
        -Review "Decide whether Discovery vectors should compose existing Core scalar/vector pieces, wrap an existing Core vector carrier, or remain separate because they add temporal and knowledge provenance semantics."
}
else
{
    Write-Candidate `
        -Severity "INFO" `
        -Candidate "IDiscoveryVector / DiscoveryVectorBase" `
        -Reason "No selected exact Core vector family was found for direct comparison." `
        -Review "Inspect the full vector sections before retaining a separate Discoveries vector abstraction."
}

if ($DiscoveryClusterExists -and $CoreClusterTypes)
{
    Write-Candidate `
        -Severity "HIGH REVIEW" `
        -Candidate "IDiscoveryCluster / IDiscoveryClusterer / DiscoveryClusterBase" `
        -Reason "Core already contains Neighbor cluster and clusterer concepts." `
        -Review "Determine whether Core clustering is purely topological membership clustering. If so, keep Discovery geometry separate but avoid copying generic cluster lifecycle or collection mechanics."
}

if ($ExistingIdentityTypes)
{
    Write-Candidate `
        -Severity "HIGH REVIEW" `
        -Candidate "Guid Id properties on vector, cluster, truth, and consequence" `
        -Reason "The repository already has multiple deliberate identity foundations: IdentifierBase for consumer/domain identity, BaseObject for Wilds runtime objects, and BaseUnit for temporal processing units." `
        -Review "Choose an existing identity model or explicitly document why these derived immutable results require standalone Guid identity."
}

Write-Candidate `
    -Severity "MEDIUM REVIEW" `
    -Candidate "IDiscoveryVectorComponent" `
    -Reason "Position, Velocity, and Acceleration describe temporal kinematics, while existing Core emission vectors may already provide reusable dimensions and scalar values." `
    -Review "Consider separating a reusable vector component from temporal derivative snapshots, rather than placing all temporal state directly on one component."

Write-Candidate `
    -Severity "MEDIUM REVIEW" `
    -Candidate "IDiscoveryVectorHistory" `
    -Reason "The current interface exposes ordered vectors but does not identify the subject, timeline, dimension schema, or sampling/topology scope whose history it represents." `
    -Review "Determine whether existing Timeline/Epoch infrastructure should own evaluation history, with Discoveries consuming historical vector snapshots instead of introducing an unrelated history container."

if ($TruthExists)
{
    Write-Candidate `
        -Severity "MEDIUM REVIEW" `
        -Candidate "ITruth / TruthBase" `
        -Reason "The composition boundary appears useful, but it repeats Id, Name, Value, timestamp, and snapshot logic found in existing identity and knowledge foundations." `
        -Review "Preserve Truth as a distinct semantic layer, but consider inheriting or composing existing IdentifierBase and shared immutable-source-list mechanics."
}

if ($ConsequenceExists)
{
    Write-Candidate `
        -Severity "MEDIUM REVIEW" `
        -Candidate "IConsequence / ConsequenceBase" `
        -Reason "The semantic boundary is distinct, but the base repeats Id, Name, Value, confidence validation, timestamp, and immutable-list mechanics." `
        -Review "Retain Consequence as knowledge, but reuse existing identifier and value conventions before implementation."
}

Write-Candidate `
    -Severity "HIGH REVIEW" `
    -Candidate "Repeated Snapshot<T> implementations" `
    -Reason "DiscoveryVectorBase, DiscoveryClusterBase, TruthBase, and ConsequenceBase each implement local immutable-list copying and null validation." `
    -Review "Check for an existing collection snapshot helper. If none exists, consider one shared internal Discoveries helper instead of four parallel copies."

Write-Candidate `
    -Severity "HIGH REVIEW" `
    -Candidate "Repeated finite/unit-range validation" `
    -Reason "The new bases repeat NaN, infinity, non-negative, and [0,1] validation already used throughout Evidence, Proof, Fact, emission, distance, and normalization structures." `
    -Review "Reuse an existing numeric validation abstraction if present. If none exists, introduce one narrowly scoped internal helper rather than parallel validation methods."

Write-Candidate `
    -Severity "MEDIUM REVIEW" `
    -Candidate "Magnitude stored on DiscoveryVectorBase" `
    -Reason "Magnitude is ordinarily derivable from vector components and may become inconsistent if supplied independently." `
    -Review "Prefer computed magnitude or require an explicit consumer-supplied metric contract when Euclidean magnitude is not appropriate."

Write-Candidate `
    -Severity "MEDIUM REVIEW" `
    -Candidate "Centroid represented as IDiscoveryVector" `
    -Reason "A centroid is a derived geometric point and may not require independent identity, contributor provenance, or evaluation time equivalent to a complete Discovery vector." `
    -Review "Consider a smaller point/component-set abstraction or explicitly define why a centroid is a complete Discovery vector."

Write-Candidate `
    -Severity "MEDIUM REVIEW" `
    -Candidate "Truth and Consequence generic object Value" `
    -Reason "Existing Facts already use consumer-owned values and naming conventions. A second unstructured named-value convention may drift from IFact semantics." `
    -Review "Determine whether Truth and Consequence should expose existing Fact-like value metadata, generic typed values, or consumer-defined descriptors rather than repeat object Value."

# ============================================================
# SUMMARY MATRIX
# ============================================================

Write-Section "SUMMARY MATRIX"

Write-Line "Review priorities:"
Write-Line ""
Write-Line "1. Identity reuse"
Write-Line "   Compare Guid Id against IdentifierBase, BaseObject, and BaseUnit."
Write-Line ""
Write-Line "2. Vector reuse"
Write-Line "   Compare Discovery vectors/components against Core emission vectors/scalars."
Write-Line ""
Write-Line "3. Cluster separation"
Write-Line "   Separate Core topological clusters from Discoveries geometric clusters."
Write-Line ""
Write-Line "4. History ownership"
Write-Line "   Compare vector history against Timeline/Epoch rather than inventing parallel time."
Write-Line ""
Write-Line "5. Shared validation"
Write-Line "   Remove repeated numeric validation if an existing helper can be reused."
Write-Line ""
Write-Line "6. Shared immutable snapshots"
Write-Line "   Remove repeated list-copy implementations if a reusable helper exists."
Write-Line ""
Write-Line "7. Semantic layers"
Write-Line "   Preserve Truth and Consequence boundaries if they remain composition results,"
Write-Line "   but align their identity/value/provenance mechanics with existing foundations."

# ============================================================
# FULL SOURCE
# ============================================================

Write-Section "FULL SOURCE"

foreach ($File in $Files)
{
    Write-Line ""
    Write-Line "------------------------------------------------------------"
    Write-Line "PROJECT: $(Get-ProjectName $File.FullName)"
    Write-Line "FILE: $(Get-RelativePath $File.FullName)"
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

Write-Section "AUDIT COMPLETE"

Write-Line "Repo root: $RepoRoot"
Write-Line "Discoveries root: $DiscoveriesRoot"
Write-Line "Selected files: $($Files.Count)"
Write-Line "New H candidate files: $($NewHFiles.Count)"
Write-Line "Missing optional paths: $($MissingOptionalPaths.Count)"
Write-Line "Report: $Report"
Write-Line "Latest: $Latest"

Copy-Item `
    -LiteralPath $Report `
    -Destination $Latest `
    -Force

Write-Host ""
Write-Host "============================================================"
Write-Host " DISCOVERIES STRUCTURE-REUSE AUDIT COMPLETE"
Write-Host "============================================================"
Write-Host "Repo      : $RepoRoot"
Write-Host "Source    : $DiscoveriesRoot"
Write-Host "Files     : $($Files.Count)"
Write-Host "H files   : $($NewHFiles.Count)"
Write-Host "Missing   : $($MissingOptionalPaths.Count)"
Write-Host "Report    : $Report"
Write-Host "Latest    : $Latest"
Write-Host "============================================================"

if ($MissingOptionalPaths.Count -gt 0)
{
    Write-Host ""
    Write-Host "Review REQUESTED PATH VALIDATION in the report."
    Write-Host "Missing optional comparison files were not invented."
}