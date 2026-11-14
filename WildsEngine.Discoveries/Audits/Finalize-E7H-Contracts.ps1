$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# ============================================================
# FINALIZE E7-H CONTRACTS
#
# Expected script location:
# <repo>\WildsEngine.Discoveries\Audits\Finalize-E7H-Contracts.ps1
#
# The current working directory does not matter.
#
# This stage:
# - keeps all changes inside WildsEngine.Discoveries;
# - removes Core topology types from intrinsic H contracts;
# - introduces a standalone Discovery-vector input boundary;
# - preserves external provenance without interpreting it;
# - gives vector series explicit scope and exact inputs;
# - makes Truth preserve the exact congealment series;
# - validates vector dimensions, contributions, motion, and
#   exact Discovery/Fact ownership relationships;
# - makes zero changes to WildsEngine.Core;
# - fails if Core already has uncommitted changes;
# - builds WildsEngine.Discoveries in Release after changes.
# ============================================================

# ============================================================
# PATHS
#
# Resolve everything from the script location.
# ============================================================

$AuditsRoot = $PSScriptRoot
$ProjectRoot = Split-Path -Parent $AuditsRoot
$RepoRoot = Split-Path -Parent $ProjectRoot

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

if (-not (Test-Path -LiteralPath $ProjectFile))
{
    throw @"
Could not locate the WildsEngine.Discoveries project.

Script root:
  $AuditsRoot

Expected project:
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

# ============================================================
# HELPERS
# ============================================================

function Write-SourceFile
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    $Parent =
    Split-Path -Parent $Path

    if (-not (Test-Path -LiteralPath $Parent))
    {
        New-Item `
            -ItemType Directory `
            -Path $Parent `
            -Force |
                Out-Null
    }

    Set-Content `
        -LiteralPath $Path `
        -Value $Content `
        -Encoding UTF8

    Write-Host (
    "[WRITE] {0}" -f
    $Path
    ) -ForegroundColor Green
}

Write-Host ""
Write-Host "============================================================"
Write-Host " FINALIZE E7-H CONTRACTS"
Write-Host "============================================================"
Write-Host "Repo    : $RepoRoot"
Write-Host "Project : $ProjectRoot"
Write-Host "Script  : $AuditsRoot"
Write-Host "============================================================"

# ============================================================
# PRE-FLIGHT CORE GUARD
#
# Core is canon. Do not continue if Core has working-tree
# changes because this stage must not obscure their origin.
# ============================================================

Write-Host ""
Write-Host "============================================================"
Write-Host " PRE-FLIGHT CORE GUARD"
Write-Host "============================================================"

$CoreChanges =
git `
    -C $RepoRoot `
    status `
    --short `
    -- `
    .\WildsEngine.Core

if ($CoreChanges)
{
    Write-Host ""
    Write-Host "CORE CHANGES DETECTED:" -ForegroundColor Red

    $CoreChanges |
            ForEach-Object {
                Write-Host $_ -ForegroundColor Red
            }

    throw "Core must remain unchanged during E7-H contract work."
}

Write-Host "[PASS] WildsEngine.Core is unchanged." -ForegroundColor Green

# ============================================================
# VECTOR INPUT
#
# This becomes the standalone Discoveries boundary.
#
# A Core integration can supply an INeighborRelationship as
# Provenance without making that type part of the intrinsic
# Discoveries contract.
#
# A future standalone consumer could supply some other
# authorized provenance object.
# ============================================================

Write-SourceFile `
    -Path (Join-Path $AbstractionsDir "IDiscoveryVectorInput.cs") `
    -Content @'
namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents one authoritative Discovery selected for
    /// vectorization.
    ///
    /// Provenance may carry the external source that caused this
    /// Discovery to enter the sample.
    ///
    /// Discoveries preserves that object without interpreting
    /// or owning it.
    /// </summary>
    public interface IDiscoveryVectorInput
    {
        IDiscovery Discovery { get; }

        object? Provenance { get; }
    }
}
'@

# ============================================================
# VECTOR CONTRIBUTION
#
# Remove the direct INeighborRelationship dependency.
# Exact external sampling provenance remains available through
# Input.Provenance.
# ============================================================

Write-SourceFile `
    -Path (Join-Path $AbstractionsDir "IDiscoveryVectorContribution.cs") `
    -Content @'
namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents one consumer-authorized contribution from an
    /// authoritative Discovery input into a named vector
    /// dimension.
    /// </summary>
    public interface IDiscoveryVectorContribution
    {
        IDiscoveryVectorInput Input { get; }

        IFact? Fact { get; }

        string Dimension { get; }

        double Value { get; }

        double Weight { get; }
    }
}
'@

# ============================================================
# VECTORIZATION POLICY
#
# Consumer semantics enter here.
# The policy now consumes an intrinsic Discoveries input rather
# than a Core topology type.
# ============================================================

Write-SourceFile `
    -Path (Join-Path $AbstractionsDir "IDiscoveryVectorizationPolicy.cs") `
    -Content @'
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Consumer-supplied policy that maps one authoritative
    /// Discovery input into numeric vector contributions.
    ///
    /// The policy owns semantic interpretation.
    ///
    /// Discoveries owns validation and preservation of the
    /// returned provenance.
    /// </summary>
    public interface IDiscoveryVectorizationPolicy
    {
        IReadOnlyList<IDiscoveryVectorContribution> Vectorize(
            IDiscoveryVectorInput input);
    }
}
'@

# ============================================================
# VECTOR SERIES
#
# Series is a scoped ordered view, not another temporal system.
#
# Inputs preserve the exact knowledge population represented by
# the series.
# ============================================================

Write-SourceFile `
    -Path (Join-Path $AbstractionsDir "IDiscoveryVectorSeries.cs") `
    -Content @'
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Provides a scoped, ordered view over immutable Discovery
    /// vector evaluations.
    ///
    /// A series does not establish a second temporal ownership
    /// model.
    ///
    /// Existing history infrastructure or a consumer may supply
    /// the ordered snapshots.
    /// </summary>
    public interface IDiscoveryVectorSeries
    {
        string Scope { get; }

        IReadOnlyList<IDiscoveryVectorInput> Inputs { get; }

        IReadOnlyList<IDiscoveryVector> Vectors { get; }

        IDiscoveryVector? Current { get; }

        IDiscoveryVector? Previous { get; }
    }
}
'@

# ============================================================
# TRUTH
#
# Preserve the exact vector series whose geometry and movement
# congealed.
# ============================================================

Write-SourceFile `
    -Path (Join-Path $AbstractionsDir "ITruth.cs") `
    -Content @'
using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents scoped knowledge that has congealed from the
    /// composition of Discoveries and their geometric clusters.
    ///
    /// Truth remains attributable and scoped.
    ///
    /// It is not an assertion of universal consumer reality.
    /// </summary>
    public interface ITruth :
        IIdentifier
    {
        string Name { get; }

        object? Value { get; }

        IDiscoveryVectorSeries Series { get; }

        IReadOnlyList<IDiscoveryCluster> Clusters { get; }

        IReadOnlyList<IDiscovery> Discoveries { get; }

        double Stability { get; }

        double Confidence { get; }

        DateTimeOffset CongealedAt { get; }
    }
}
'@

# ============================================================
# DIMENSION VALIDATION
#
# Internal Discoveries mechanics only.
#
# This validates relationships among the already-defined public
# contracts without creating another public vector framework.
# ============================================================

Write-SourceFile `
    -Path (Join-Path $BaseDir "DiscoveryDimensions.cs") `
    -Content @'
using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Shared dimension validation for Discovery geometry.
    /// </summary>
    internal static class DiscoveryDimensions
    {
        public static void ValidateComponents(
            IReadOnlyList<IDiscoveryVectorComponent> components,
            string parameterName)
        {
            if (components == null)
            {
                throw new ArgumentNullException(
                    parameterName);
            }

            var names =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < components.Count;
                 index++)
            {
                var component =
                    components[index] ??
                    throw new ArgumentException(
                        "Discovery-vector components cannot contain null items.",
                        parameterName);

                if (string.IsNullOrWhiteSpace(
                        component.Name))
                {
                    throw new ArgumentException(
                        "Discovery-vector component names cannot be empty.",
                        parameterName);
                }

                if (!names.Add(
                        component.Name))
                {
                    throw new ArgumentException(
                        "Discovery-vector component names must be unique.",
                        parameterName);
                }

                DiscoveryNumbers.Finite(
                    component.Value,
                    parameterName);
            }
        }

        public static void ValidateContributions(
            IReadOnlyList<IDiscoveryVectorContribution> contributions,
            IReadOnlyList<IDiscoveryVectorComponent> components,
            string parameterName)
        {
            if (contributions == null)
            {
                throw new ArgumentNullException(
                    parameterName);
            }

            if (components == null)
            {
                throw new ArgumentNullException(
                    nameof(components));
            }

            var dimensions =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < components.Count;
                 index++)
            {
                dimensions.Add(
                    components[index].Name);
            }

            for (var index = 0;
                 index < contributions.Count;
                 index++)
            {
                var contribution =
                    contributions[index] ??
                    throw new ArgumentException(
                        "Discovery-vector contributions cannot contain null items.",
                        parameterName);

                if (contribution.Input == null)
                {
                    throw new ArgumentException(
                        "Discovery-vector contributions require an input.",
                        parameterName);
                }

                if (contribution.Input.Discovery == null)
                {
                    throw new ArgumentException(
                        "Discovery-vector inputs require a Discovery.",
                        parameterName);
                }

                if (string.IsNullOrWhiteSpace(
                        contribution.Dimension))
                {
                    throw new ArgumentException(
                        "Discovery-vector contribution dimensions cannot be empty.",
                        parameterName);
                }

                if (!dimensions.Contains(
                        contribution.Dimension))
                {
                    throw new ArgumentException(
                        "Every contribution dimension must exist in the vector.",
                        parameterName);
                }

                if (contribution.Fact != null &&
                    !ReferenceEquals(
                        contribution.Fact.Entity,
                        contribution.Input.Discovery.Entity))
                {
                    throw new ArgumentException(
                        "A contributing Fact must belong to the same exact Entity as its Discovery.",
                        parameterName);
                }

                DiscoveryNumbers.Finite(
                    contribution.Value,
                    parameterName);

                DiscoveryNumbers.NonNegative(
                    contribution.Weight,
                    parameterName);
            }
        }

        public static void ValidateMotion(
            IReadOnlyList<IDiscoveryVectorMotion> motion,
            IReadOnlyList<IDiscoveryVectorComponent> components,
            string parameterName)
        {
            if (motion == null)
            {
                throw new ArgumentNullException(
                    parameterName);
            }

            if (components == null)
            {
                throw new ArgumentNullException(
                    nameof(components));
            }

            var dimensions =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < components.Count;
                 index++)
            {
                dimensions.Add(
                    components[index].Name);
            }

            var motionDimensions =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < motion.Count;
                 index++)
            {
                var item =
                    motion[index] ??
                    throw new ArgumentException(
                        "Discovery-vector motion cannot contain null items.",
                        parameterName);

                if (string.IsNullOrWhiteSpace(
                        item.Dimension))
                {
                    throw new ArgumentException(
                        "Discovery-vector motion dimensions cannot be empty.",
                        parameterName);
                }

                if (!dimensions.Contains(
                        item.Dimension))
                {
                    throw new ArgumentException(
                        "Every motion dimension must exist in the vector.",
                        parameterName);
                }

                if (!motionDimensions.Add(
                        item.Dimension))
                {
                    throw new ArgumentException(
                        "Only one motion value may exist per vector dimension.",
                        parameterName);
                }

                DiscoveryNumbers.Finite(
                    item.Velocity,
                    parameterName);

                DiscoveryNumbers.Finite(
                    item.Acceleration,
                    parameterName);
            }
        }
    }
}
'@

# ============================================================
# VECTOR BASE
#
# Validate component schema, contribution alignment, and motion
# alignment after immutable snapshots are taken.
#
# Magnitude remains calculated by the supplied metric.
# ============================================================

Write-SourceFile `
    -Path (Join-Path $BaseDir "DiscoveryVectorBase.cs") `
    -Content @'
using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Reusable immutable Discovery-vector evaluation.
    ///
    /// Magnitude is calculated by the supplied metric from the
    /// exact validated component snapshot.
    /// </summary>
    public abstract class DiscoveryVectorBase :
        IDiscoveryVector
    {
        protected DiscoveryVectorBase(
            IReadOnlyList<IDiscoveryVectorComponent> components,
            IReadOnlyList<IDiscoveryVectorContribution> contributions,
            IReadOnlyList<IDiscoveryVectorMotion> motion,
            IDiscoveryVectorMetric metric,
            DateTimeOffset evaluatedAt)
        {
            Components =
                DiscoverySnapshots.Required(
                    components,
                    nameof(components));

            Contributions =
                DiscoverySnapshots.Required(
                    contributions,
                    nameof(contributions));

            Motion =
                DiscoverySnapshots.Required(
                    motion,
                    nameof(motion));

            DiscoveryDimensions.ValidateComponents(
                Components,
                nameof(components));

            DiscoveryDimensions.ValidateContributions(
                Contributions,
                Components,
                nameof(contributions));

            DiscoveryDimensions.ValidateMotion(
                Motion,
                Components,
                nameof(motion));

            Metric =
                metric ??
                throw new ArgumentNullException(
                    nameof(metric));

            EvaluatedAt =
                evaluatedAt;

            var magnitude =
                Metric.GetMagnitude(
                    this);

            Magnitude =
                DiscoveryNumbers.NonNegative(
                    magnitude,
                    nameof(magnitude));
        }

        public IReadOnlyList<IDiscoveryVectorComponent> Components { get; }

        public int Dimension =>
            Components.Count;

        public IReadOnlyList<IDiscoveryVectorContribution> Contributions
        {
            get;
        }

        public IReadOnlyList<IDiscoveryVectorMotion> Motion { get; }

        public IDiscoveryVectorMetric Metric { get; }

        public double Magnitude { get; }

        public DateTimeOffset EvaluatedAt { get; }
    }
}
'@

# ============================================================
# TRUTH BASE
#
# Truth retains:
# - existing Discoveries identity;
# - exact vector series;
# - exact clusters;
# - exact authoritative Discoveries;
# - stability and confidence;
# - congealment timestamp.
#
# Explicitly listed Discoveries must occur in the exact series
# input population.
# ============================================================

Write-SourceFile `
    -Path (Join-Path $BaseDir "TruthBase.cs") `
    -Content @'
using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Reusable base for scoped Truths congealed from Discovery
    /// composition.
    ///
    /// Truth reuses established Discoveries identity and retains
    /// the exact scoped vector series whose composition
    /// congealed.
    /// </summary>
    public abstract class TruthBase :
        IdentifierBase,
        ITruth
    {
        protected TruthBase(
            string slug,
            string name,
            object? value,
            IDiscoveryVectorSeries series,
            IReadOnlyList<IDiscoveryCluster> clusters,
            IReadOnlyList<IDiscovery> discoveries,
            double stability,
            double confidence,
            DateTimeOffset congealedAt)
            : base(
                slug)
        {
            Initialize(
                name,
                value,
                series,
                clusters,
                discoveries,
                stability,
                confidence,
                congealedAt);
        }

        protected TruthBase(
            Guid id,
            string slug,
            string name,
            object? value,
            IDiscoveryVectorSeries series,
            IReadOnlyList<IDiscoveryCluster> clusters,
            IReadOnlyList<IDiscovery> discoveries,
            double stability,
            double confidence,
            DateTimeOffset congealedAt)
            : base(
                id,
                slug)
        {
            Initialize(
                name,
                value,
                series,
                clusters,
                discoveries,
                stability,
                confidence,
                congealedAt);
        }

        public string Name { get; private set; } = null!;

        public object? Value { get; private set; }

        public IDiscoveryVectorSeries Series
        {
            get;
            private set;
        } = null!;

        public IReadOnlyList<IDiscoveryCluster> Clusters
        {
            get;
            private set;
        } = null!;

        public IReadOnlyList<IDiscovery> Discoveries
        {
            get;
            private set;
        } = null!;

        public double Stability { get; private set; }

        public double Confidence { get; private set; }

        public DateTimeOffset CongealedAt { get; private set; }

        private void Initialize(
            string name,
            object? value,
            IDiscoveryVectorSeries series,
            IReadOnlyList<IDiscoveryCluster> clusters,
            IReadOnlyList<IDiscovery> discoveries,
            double stability,
            double confidence,
            DateTimeOffset congealedAt)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Truth name cannot be empty.",
                    nameof(name));
            }

            Name =
                name.Trim();

            Value =
                value;

            Series =
                series ??
                throw new ArgumentNullException(
                    nameof(series));

            if (string.IsNullOrWhiteSpace(
                    Series.Scope))
            {
                throw new ArgumentException(
                    "Truth requires a scoped Discovery-vector series.",
                    nameof(series));
            }

            if (Series.Inputs == null)
            {
                throw new ArgumentException(
                    "Truth requires a series with an input population.",
                    nameof(series));
            }

            if (Series.Vectors == null)
            {
                throw new ArgumentException(
                    "Truth requires a series with vector evaluations.",
                    nameof(series));
            }

            Clusters =
                DiscoverySnapshots.Required(
                    clusters,
                    nameof(clusters));

            Discoveries =
                DiscoverySnapshots.Required(
                    discoveries,
                    nameof(discoveries));

            ValidateDiscoveriesBelongToSeries(
                Discoveries,
                Series);

            Stability =
                DiscoveryNumbers.Unit(
                    stability,
                    nameof(stability));

            Confidence =
                DiscoveryNumbers.Unit(
                    confidence,
                    nameof(confidence));

            CongealedAt =
                congealedAt;
        }

        private static void ValidateDiscoveriesBelongToSeries(
            IReadOnlyList<IDiscovery> discoveries,
            IDiscoveryVectorSeries series)
        {
            for (var discoveryIndex = 0;
                 discoveryIndex < discoveries.Count;
                 discoveryIndex++)
            {
                var discovery =
                    discoveries[discoveryIndex];

                var found =
                    false;

                for (var inputIndex = 0;
                     inputIndex < series.Inputs.Count;
                     inputIndex++)
                {
                    var input =
                        series.Inputs[inputIndex];

                    if (input == null)
                    {
                        throw new ArgumentException(
                            "Truth series inputs cannot contain null items.",
                            nameof(series));
                    }

                    if (ReferenceEquals(
                            discovery,
                            input.Discovery))
                    {
                        found =
                            true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new ArgumentException(
                        "Every Truth Discovery must belong to the exact congealment series.",
                        nameof(discoveries));
                }
            }
        }
    }
}
'@

# ============================================================
# REQUIRED FILE VERIFICATION
# ============================================================

Write-Host ""
Write-Host "============================================================"
Write-Host " STRUCTURE VERIFICATION"
Write-Host "============================================================"

$RequiredFiles = @(
    (Join-Path $AbstractionsDir "IDiscoveryVectorInput.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorComponent.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorPoint.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorMotion.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorContribution.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorMetric.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVector.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorSeries.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorizationPolicy.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryCluster.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryClusterer.cs"),
    (Join-Path $AbstractionsDir "ITruth.cs"),
    (Join-Path $AbstractionsDir "ITruthCongealmentPolicy.cs"),
    (Join-Path $AbstractionsDir "IConsequence.cs"),
    (Join-Path $AbstractionsDir "IConsequenceProjectionPolicy.cs"),
    (Join-Path $BaseDir "DiscoveryNumbers.cs"),
    (Join-Path $BaseDir "DiscoverySnapshots.cs"),
    (Join-Path $BaseDir "DiscoveryDimensions.cs"),
    (Join-Path $BaseDir "DiscoveryVectorBase.cs"),
    (Join-Path $BaseDir "DiscoveryClusterBase.cs"),
    (Join-Path $BaseDir "TruthBase.cs"),
    (Join-Path $BaseDir "ConsequenceBase.cs")
)

foreach ($RequiredFile in $RequiredFiles)
{
    if (-not (Test-Path -LiteralPath $RequiredFile))
    {
        throw "Required file is missing: $RequiredFile"
    }

    Write-Host "[FOUND] $RequiredFile" -ForegroundColor Green
}

# ============================================================
# OBSOLETE CONTRACT VERIFICATION
# ============================================================

$ForbiddenFiles = @(
    (Join-Path $AbstractionsDir "IDiscoveryVectorHistory.cs")
)

foreach ($ForbiddenFile in $ForbiddenFiles)
{
    if (Test-Path -LiteralPath $ForbiddenFile)
    {
        throw "Obsolete file still exists: $ForbiddenFile"
    }

    Write-Host "[ABSENT] $ForbiddenFile" -ForegroundColor Green
}

# ============================================================
# INTRINSIC CONTRACT CORE-DEPENDENCY AUDIT
#
# These are intrinsic H knowledge contracts.
# They must not directly expose Core topology/runtime types.
# ============================================================

Write-Host ""
Write-Host "============================================================"
Write-Host " INTRINSIC CONTRACT CORE-DEPENDENCY AUDIT"
Write-Host "============================================================"

$IntrinsicFiles = @(
    (Join-Path $AbstractionsDir "IDiscoveryVectorInput.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorContribution.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorizationPolicy.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorComponent.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorPoint.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorMotion.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorMetric.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVector.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorSeries.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryCluster.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryClusterer.cs"),
    (Join-Path $AbstractionsDir "ITruth.cs"),
    (Join-Path $AbstractionsDir "ITruthCongealmentPolicy.cs"),
    (Join-Path $AbstractionsDir "IConsequence.cs"),
    (Join-Path $AbstractionsDir "IConsequenceProjectionPolicy.cs")
)

$CoreDependencyHits =
$IntrinsicFiles |
        Select-String `
    -Pattern "WildsEngine\.Core|INeighborRelationship|IBaseObject|IBaseUnit" `
    -CaseSensitive:$false

if ($CoreDependencyHits)
{
    foreach ($Hit in $CoreDependencyHits)
    {
        Write-Host (
        "[CORE DEPENDENCY] {0}:{1}: {2}" -f
        $Hit.Path,
        $Hit.LineNumber,
        $Hit.Line.Trim()
        ) -ForegroundColor Red
    }

    throw "Intrinsic E7-H contracts contain direct Core dependencies."
}

Write-Host "[PASS] Intrinsic E7-H contracts are Core-neutral." -ForegroundColor Green

# ============================================================
# OBSOLETE SYMBOL AUDIT
# ============================================================

Write-Host ""
Write-Host "============================================================"
Write-Host " OBSOLETE SYMBOL AUDIT"
Write-Host "============================================================"

$ProductionFiles =
Get-ChildItem `
    -Path $ProjectRoot `
    -Recurse `
    -File `
    -Filter *.cs |
        Where-Object {
            $_.FullName -notmatch "\\bin\\" -and
                    $_.FullName -notmatch "\\obj\\" -and
                    $_.FullName -notmatch "\\Audits\\"
        }

$ObsoleteHits =
$ProductionFiles |
        Select-String `
    -Pattern "\bIDiscoveryVectorHistory\b|\.Position\b" `
    -CaseSensitive:$false

if ($ObsoleteHits)
{
    foreach ($Hit in $ObsoleteHits)
    {
        Write-Host (
        "[OBSOLETE] {0}:{1}: {2}" -f
        $Hit.Path,
        $Hit.LineNumber,
        $Hit.Line.Trim()
        ) -ForegroundColor Red
    }

    throw "Obsolete E7-H symbols remain in production source."
}

Write-Host "[PASS] No obsolete E7-H symbols remain." -ForegroundColor Green

# ============================================================
# RELEASE BUILD
# ============================================================

Write-Host ""
Write-Host "============================================================"
Write-Host " RELEASE BUILD"
Write-Host "============================================================"

dotnet build `
    $ProjectFile `
    -c Release

if ($LASTEXITCODE -ne 0)
{
    throw "WildsEngine.Discoveries Release build failed."
}

# ============================================================
# POST-BUILD CORE GUARD
#
# Verify this script did not alter Core.
# ============================================================

Write-Host ""
Write-Host "============================================================"
Write-Host " POST-BUILD CORE GUARD"
Write-Host "============================================================"

$CoreChangesAfter =
git `
    -C $RepoRoot `
    status `
    --short `
    -- `
    .\WildsEngine.Core

if ($CoreChangesAfter)
{
    Write-Host ""
    Write-Host "CORE CHANGES DETECTED:" -ForegroundColor Red

    $CoreChangesAfter |
            ForEach-Object {
                Write-Host $_ -ForegroundColor Red
            }

    throw "Core changed during E7-H contract finalization."
}

Write-Host "[PASS] WildsEngine.Core remains unchanged." -ForegroundColor Green

# ============================================================
# DISCOVERIES STATUS
# ============================================================

Write-Host ""
Write-Host "============================================================"
Write-Host " DISCOVERIES STATUS"
Write-Host "============================================================"

git `
    -C $RepoRoot `
    status `
    --short `
    -- `
    .\WildsEngine.Discoveries

# ============================================================
# COMPLETE
# ============================================================

Write-Host ""
Write-Host "============================================================"
Write-Host " E7-H CONTRACT FINALIZATION COMPLETE"
Write-Host "============================================================"
Write-Host "Repo    : $RepoRoot"
Write-Host "Project : $ProjectRoot"
Write-Host ""
Write-Host "Core unchanged."
Write-Host "Intrinsic H contracts are Core-neutral."
Write-Host "External sampling provenance remains preservable."
Write-Host "Vector series preserve scope and exact inputs."
Write-Host "Truth preserves the exact congealment series."
Write-Host "Dimension and contribution alignment are validated."
Write-Host "Release build passed."
Write-Host "============================================================"