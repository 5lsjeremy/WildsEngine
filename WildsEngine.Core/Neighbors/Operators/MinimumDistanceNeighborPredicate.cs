using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class MinimumDistanceNeighborPredicate : NeighborPredicateBase
    {
        public MinimumDistanceNeighborPredicate(double minimumDistance) =>
            MinimumDistance = NeighborDistance.Validate(minimumDistance, nameof(minimumDistance));

        public double MinimumDistance { get; }

        protected override bool IncludesCore(IBaseObject subject, INeighborRelationship relationship) =>
            relationship.Distance >= MinimumDistance;
    }
}
