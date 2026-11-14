$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$ProjectRoot = (Get-Location).Path
$ProjectFile = Join-Path $ProjectRoot "WildsEngine.Discoveries.csproj"
$AbstractionsDir = Join-Path $ProjectRoot "Abstractions"
$BaseDir = Join-Path $ProjectRoot "Base"

if (-not (Test-Path -LiteralPath $ProjectFile))
{
    throw @"
Run this script from the WildsEngine.Discoveries project root.

Expected:
  $ProjectFile
"@
}

function Write-SourceFile
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    Set-Content `
        -LiteralPath $Path `
        -Value $Content `
        -Encoding UTF8

    Write-Host "[WRITE] $Path" -ForegroundColor Green
}

Write-Host ""
Write-Host "============================================================"
Write-Host " FINALIZE E7-H CONTRACTS"
Write-Host "============================================================"
Write-Host "Project: $ProjectRoot"
Write-Host "============================================================"

# ============================================================
# VECTOR INPUT
#
# This is the standalone Discoveries boundary.
#
# A Core integration may supply an INeighborRelationship as
# Provenance without teaching Discoveries about that Core type.
# Another consumer may supply a route, observation record,
# database row, message, or another authorized source.
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
    /// Discovery to enter the sample. Discoveries preserves that
    /// object without interpreting or owning it.
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
# Remove the direct Core INeighborRelationship dependency.
# Exact sampling provenance remains available through Input.
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
# The policy now consumes a standalone Discoveries input instead
# of a Core relationship.
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
    /// The policy owns semantic interpretation. Discoveries owns
    /// validation and preservation of the returned provenance.
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
# A series is explicitly scoped and retains the exact inputs
# whose evaluations it orders.
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
    /// model. Existing history infrastructure or a consumer may
    /// supply the ordered snapshots.
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
# Preserve the series whose geometry and motion congealed.
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
    /// Truth remains attributable and scoped. It is not an
    /// assertion of universal consumer reality.
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
# This remains an internal Discoveries mechanic.
# It does not create a second public vector framework.
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
            var names =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < components.Count;
                 index++)
            {
                var component =
                    components[index];

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
                    contributions[index];

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
                    motion[index];

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
# Validate schema alignment after taking immutable snapshots and
# before asking the metric to calculate magnitude.
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
# Require and retain the exact congealment series.
# Validate that every explicitly listed Discovery appears in one
# of the series inputs.
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
    /// the exact scoped vector series whose composition congealed.
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

        public IDiscoveryVectorSeries Series { get; private set; } = null!;

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
                    if (ReferenceEquals(
                            discovery,
                            series.Inputs[inputIndex].Discovery))
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
# VERIFY REQUIRED FILES
# ============================================================

$RequiredFiles = @(
    (Join-Path $AbstractionsDir "IDiscoveryVectorInput.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorContribution.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorizationPolicy.cs"),
    (Join-Path $AbstractionsDir "IDiscoveryVectorSeries.cs"),
    (Join-Path $AbstractionsDir "ITruth.cs"),
    (Join-Path $BaseDir "DiscoveryDimensions.cs"),
    (Join-Path $BaseDir "DiscoveryVectorBase.cs"),
    (Join-Path $BaseDir "TruthBase.cs")
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
# CORE CHANGE GUARD
# ============================================================

$RepoRoot = Split-Path -Parent $ProjectRoot
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
    $CoreChanges | ForEach-Object {
        Write-Host $_ -ForegroundColor Red
    }

    throw "Core must remain unchanged during E7-H contract work."
}

Write-Host "[PASS] WildsEngine.Core is unchanged." -ForegroundColor Green

# ============================================================
# DIRECT CORE-DEPENDENCY AUDIT FOR NEW INTRINSIC CONTRACTS
# ============================================================

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
# STATUS
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

Write-Host ""
Write-Host "============================================================"
Write-Host " E7-H CONTRACT FINALIZATION COMPLETE"
Write-Host "============================================================"
Write-Host "Core unchanged."
Write-Host "Intrinsic H contracts are Core-neutral."
Write-Host "Vector inputs preserve optional external provenance."
Write-Host "Series preserve scope and exact sampled inputs."
Write-Host "Truth preserves the exact congealment series."
Write-Host "Dimension and provenance alignment are validated."
Write-Host "Release build passed."
Write-Host "============================================================"