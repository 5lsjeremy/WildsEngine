using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class AdjacentNeighborPredicate : NeighborPredicateBase
    {
        protected override bool IncludesCore(IBaseObject subject, INeighborRelationship relationship) =>
            NeighborDistance.IsAdjacent(relationship.Distance);
    }
}
