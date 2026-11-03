using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class AverageNeighborDistance : NeighborOperatorBase<double?>, INeighborScalarOperator<double?>
    {
        protected override double? ApplyCore(IBaseObject subject, IReadOnlyList<INeighborRelationship> relationships)
        {
            if (relationships.Count == 0) return null;
            var sum = 0d;
            for (var i = 0; i < relationships.Count; i++) sum += relationships[i].Distance;
            return sum / relationships.Count;
        }
    }
}
