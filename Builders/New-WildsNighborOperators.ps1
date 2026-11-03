param(
    [string]$Root = (Get-Location).Path,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

function Write-SourceFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath,

        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    $path = Join-Path $Root $RelativePath
    $directory = Split-Path $path -Parent

    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    if ((Test-Path $path) -and -not $Force) {
        Write-Host "[SKIP] $RelativePath already exists. Use -Force to overwrite." -ForegroundColor Yellow
        return
    }

    Set-Content -Path $path -Value $Content -Encoding UTF8
    Write-Host "[WRITE] $RelativePath" -ForegroundColor Green
}

$abstractions = "WildsEngine.Core\Abstractions\Neighbors\Operators"
$implementations = "WildsEngine.Core\Neighbors\Operators\Base"

Write-SourceFile "$abstractions\INeighborOperator.cs" @'
using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Observes or transforms a snapshot of Neighbor
    /// relationships belonging to a Wilds object.
    ///
    /// Operators do not own or mutate the source collection.
    /// </summary>
    public interface INeighborOperator<TResult>
    {
        TResult Apply(
            IBaseObject subject,
            IReadOnlyList<INeighborRelationship> relationships);
    }
}
'@

Write-SourceFile "$abstractions\INeighborSequenceOperator.cs" @'
using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Produces an ordered or filtered relationship sequence.
    /// </summary>
    public interface INeighborSequenceOperator :
        INeighborOperator<IReadOnlyList<INeighborRelationship>>
    {
    }
}
'@

Write-SourceFile "$abstractions\INeighborGroupingOperator.cs" @'
using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Groups Neighbor relationships under stable string keys.
    /// </summary>
    public interface INeighborGroupingOperator :
        INeighborOperator<
            IReadOnlyDictionary<
                string,
                IReadOnlyList<INeighborRelationship>>>
    {
    }
}
'@

Write-SourceFile "$abstractions\INeighborScalarOperator.cs" @'
namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Produces one scalar value from a Neighbor snapshot.
    /// </summary>
    public interface INeighborScalarOperator<TResult> :
        INeighborOperator<TResult>
    {
    }
}
'@

Write-SourceFile "$abstractions\INeighborPredicate.cs" @'
namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Determines whether a relationship belongs in an
    /// operator result.
    /// </summary>
    public interface INeighborPredicate
    {
        bool Includes(
            IBaseObject subject,
            INeighborRelationship relationship);
    }
}
'@

Write-SourceFile "$abstractions\INeighborComparer.cs" @'
namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Compares two relationships for deterministic ordering.
    /// </summary>
    public interface INeighborComparer
    {
        int Compare(
            IBaseObject subject,
            INeighborRelationship left,
            INeighborRelationship right);
    }
}
'@

Write-SourceFile "$abstractions\INeighborSampler.cs" @'
using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Selects a bounded sample from a relationship snapshot.
    /// </summary>
    public interface INeighborSampler :
        INeighborSequenceOperator
    {
        int Count { get; }
    }
}
'@

Write-SourceFile "$implementations\NeighborOperatorBase.cs" @'
using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Supplies argument validation and a stable execution
    /// boundary for Neighbor operators.
    /// </summary>
    public abstract class NeighborOperatorBase<TResult> :
        INeighborOperator<TResult>
    {
        public TResult Apply(
            IBaseObject subject,
            IReadOnlyList<INeighborRelationship> relationships)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(
                    nameof(subject));
            }

            if (relationships == null)
            {
                throw new ArgumentNullException(
                    nameof(relationships));
            }

            return ApplyCore(
                subject,
                relationships);
        }

        protected abstract TResult ApplyCore(
            IBaseObject subject,
            IReadOnlyList<INeighborRelationship> relationships);
    }
}
'@

Write-SourceFile "$implementations\NeighborSequenceOperatorBase.cs" @'
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Base class for operators that return relationship
    /// sequences without mutating the source snapshot.
    /// </summary>
    public abstract class NeighborSequenceOperatorBase :
        NeighborOperatorBase<
            IReadOnlyList<INeighborRelationship>>,
        INeighborSequenceOperator
    {
        protected static IReadOnlyList<INeighborRelationship>
            Empty()
        {
            return new INeighborRelationship[0];
        }

        protected static IReadOnlyList<INeighborRelationship>
            Snapshot(
                List<INeighborRelationship> relationships)
        {
            return relationships.ToArray();
        }
    }
}
'@

Write-SourceFile "$implementations\NeighborGroupingOperatorBase.cs" @'
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Base class for operators that group relationships under
    /// deterministic string keys.
    /// </summary>
    public abstract class NeighborGroupingOperatorBase :
        NeighborOperatorBase<
            IReadOnlyDictionary<
                string,
                IReadOnlyList<INeighborRelationship>>>,
        INeighborGroupingOperator
    {
        protected static IReadOnlyDictionary<
            string,
            IReadOnlyList<INeighborRelationship>> Freeze(
                Dictionary<
                    string,
                    List<INeighborRelationship>> groups)
        {
            var frozen =
                new Dictionary<
                    string,
                    IReadOnlyList<INeighborRelationship>>(
                        groups.Count,
                        System.StringComparer.Ordinal);

            foreach (var pair in groups)
            {
                frozen.Add(
                    pair.Key,
                    pair.Value.ToArray());
            }

            return frozen;
        }
    }
}
'@

Write-SourceFile "$implementations\NeighborPredicateBase.cs" @'
using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Supplies validation for reusable Neighbor predicates.
    /// </summary>
    public abstract class NeighborPredicateBase :
        INeighborPredicate
    {
        public bool Includes(
            IBaseObject subject,
            INeighborRelationship relationship)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(
                    nameof(subject));
            }

            if (relationship == null)
            {
                throw new ArgumentNullException(
                    nameof(relationship));
            }

            return IncludesCore(
                subject,
                relationship);
        }

        protected abstract bool IncludesCore(
            IBaseObject subject,
            INeighborRelationship relationship);
    }
}
'@

Write-SourceFile "$implementations\NeighborComparerBase.cs" @'
using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Supplies validation and deterministic tie handling for
    /// Neighbor relationship comparers.
    /// </summary>
    public abstract class NeighborComparerBase :
        INeighborComparer
    {
        public int Compare(
            IBaseObject subject,
            INeighborRelationship left,
            INeighborRelationship right)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(
                    nameof(subject));
            }

            if (left == null)
            {
                throw new ArgumentNullException(
                    nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(
                    nameof(right));
            }

            var compared =
                CompareCore(
                    subject,
                    left,
                    right);

            if (compared != 0)
            {
                return compared;
            }

            compared = string.Compare(
                left.Membership,
                right.Membership,
                StringComparison.Ordinal);

            if (compared != 0)
            {
                return compared;
            }

            return left.Neighbor.Id.CompareTo(
                right.Neighbor.Id);
        }

        protected abstract int CompareCore(
            IBaseObject subject,
            INeighborRelationship left,
            INeighborRelationship right);
    }
}
'@

Write-SourceFile "$implementations\NeighborFilterOperatorBase.cs" @'
using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Filters a relationship snapshot through one reusable
    /// predicate while preserving source order.
    /// </summary>
    public abstract class NeighborFilterOperatorBase :
        NeighborSequenceOperatorBase
    {
        private readonly INeighborPredicate _predicate;

        protected NeighborFilterOperatorBase(
            INeighborPredicate predicate)
        {
            _predicate =
                predicate ??
                throw new ArgumentNullException(
                    nameof(predicate));
        }

        protected sealed override
            IReadOnlyList<INeighborRelationship> ApplyCore(
                IBaseObject subject,
                IReadOnlyList<INeighborRelationship> relationships)
        {
            var result =
                new List<INeighborRelationship>();

            for (var index = 0;
                 index < relationships.Count;
                 index++)
            {
                var relationship =
                    relationships[index];

                if (_predicate.Includes(
                        subject,
                        relationship))
                {
                    result.Add(
                        relationship);
                }
            }

            return Snapshot(
                result);
        }
    }
}
'@

Write-SourceFile "$implementations\NeighborOrderOperatorBase.cs" @'
using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Orders a copied relationship snapshot through a reusable
    /// comparer and never mutates the source sequence.
    /// </summary>
    public abstract class NeighborOrderOperatorBase :
        NeighborSequenceOperatorBase
    {
        private readonly INeighborComparer _comparer;

        protected NeighborOrderOperatorBase(
            INeighborComparer comparer)
        {
            _comparer =
                comparer ??
                throw new ArgumentNullException(
                    nameof(comparer));
        }

        protected sealed override
            IReadOnlyList<INeighborRelationship> ApplyCore(
                IBaseObject subject,
                IReadOnlyList<INeighborRelationship> relationships)
        {
            var result =
                new List<INeighborRelationship>(
                    relationships.Count);

            for (var index = 0;
                 index < relationships.Count;
                 index++)
            {
                result.Add(
                    relationships[index]);
            }

            result.Sort(
                (left, right) =>
                    _comparer.Compare(
                        subject,
                        left,
                        right));

            return Snapshot(
                result);
        }
    }
}
'@

Write-SourceFile "$implementations\NeighborSamplerBase.cs" @'
using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Base class for bounded, snapshot-safe Neighbor sampling.
    /// </summary>
    public abstract class NeighborSamplerBase :
        NeighborSequenceOperatorBase,
        INeighborSampler
    {
        protected NeighborSamplerBase(
            int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    count,
                    "Neighbor sample count cannot be negative.");
            }

            Count = count;
        }

        public int Count { get; }

        protected sealed override
            IReadOnlyList<INeighborRelationship> ApplyCore(
                IBaseObject subject,
                IReadOnlyList<INeighborRelationship> relationships)
        {
            if (Count == 0 ||
                relationships.Count == 0)
            {
                return Empty();
            }

            var boundedCount =
                Math.Min(
                    Count,
                    relationships.Count);

            return SampleCore(
                subject,
                relationships,
                boundedCount);
        }

        protected abstract IReadOnlyList<INeighborRelationship>
            SampleCore(
                IBaseObject subject,
                IReadOnlyList<INeighborRelationship> relationships,
                int count);
    }
}
'@

Write-SourceFile "$implementations\NeighborCompositeOperatorBase.cs" @'
using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Applies a deterministic sequence of Neighbor sequence
    /// operators. Each stage receives the prior stage snapshot.
    /// </summary>
    public abstract class NeighborCompositeOperatorBase :
        NeighborSequenceOperatorBase
    {
        private readonly IReadOnlyList<INeighborSequenceOperator>
            _operators;

        protected NeighborCompositeOperatorBase(
            IReadOnlyList<INeighborSequenceOperator> operators)
        {
            if (operators == null)
            {
                throw new ArgumentNullException(
                    nameof(operators));
            }

            var snapshot =
                new INeighborSequenceOperator[operators.Count];

            for (var index = 0;
                 index < operators.Count;
                 index++)
            {
                snapshot[index] =
                    operators[index] ??
                    throw new ArgumentException(
                        "Neighbor operator pipelines cannot contain null stages.",
                        nameof(operators));
            }

            _operators = snapshot;
        }

        protected sealed override
            IReadOnlyList<INeighborRelationship> ApplyCore(
                IBaseObject subject,
                IReadOnlyList<INeighborRelationship> relationships)
        {
            var current =
                relationships;

            for (var index = 0;
                 index < _operators.Count;
                 index++)
            {
                current =
                    _operators[index].Apply(
                        subject,
                        current);
            }

            return current;
        }
    }
}
'@

Write-Host ""
Write-Host "Neighbor operator contracts and base implementations created." -ForegroundColor Cyan
Write-Host "Root: $Root"
Write-Host ""
Write-Host "Next build command:" -ForegroundColor Cyan
Write-Host "dotnet build WildsEngine.sln"
