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
