using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class DistanceRangeNeighborPredicate : NeighborPredicateBase
    {
        public DistanceRangeNeighborPredicate(double minimumDistance, double maximumDistance)
        {
            MinimumDistance = NeighborDistance.Validate(minimumDistance, nameof(minimumDistance));
            MaximumDistance = NeighborDistance.Validate(maximumDistance, nameof(maximumDistance));
            if (MaximumDistance < MinimumDistance)
                throw new ArgumentException("Maximum distance cannot be less than minimum distance.", nameof(maximumDistance));
        }

        public double MinimumDistance { get; }
        public double MaximumDistance { get; }

        protected override bool IncludesCore(IBaseObject subject, INeighborRelationship relationship) =>
            relationship.Distance >= MinimumDistance && relationship.Distance <= MaximumDistance;
    }
}
