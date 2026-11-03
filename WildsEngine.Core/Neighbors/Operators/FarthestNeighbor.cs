using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class FarthestNeighbor : NeighborOperatorBase<INeighborRelationship?>, INeighborScalarOperator<INeighborRelationship?>
    {
        protected override INeighborRelationship? ApplyCore(IBaseObject subject, IReadOnlyList<INeighborRelationship> relationships)
        {
            INeighborRelationship? best = null;
            var bestDistance = double.MinValue;
            for (var i = 0; i < relationships.Count; i++)
            {
                var r = relationships[i];
                if (best == null || r.Distance > bestDistance ||
                    (r.Distance == bestDistance && r.Neighbor.Id.CompareTo(best.Neighbor.Id) < 0))
                {
                    best = r; bestDistance = r.Distance;
                }
            }
            return best;
        }
    }
}
