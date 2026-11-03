using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class MaximumDistanceNeighborPredicate : NeighborPredicateBase
    {
        public MaximumDistanceNeighborPredicate(double maximumDistance) =>
            MaximumDistance = NeighborDistance.Validate(maximumDistance, nameof(maximumDistance));

        public double MaximumDistance { get; }

        protected override bool IncludesCore(IBaseObject subject, INeighborRelationship relationship) =>
            relationship.Distance <= MaximumDistance;
    }
}
