using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class MaximumNeighborDistance : NeighborOperatorBase<double?>, INeighborScalarOperator<double?>
    {
        protected override double? ApplyCore(IBaseObject subject, IReadOnlyList<INeighborRelationship> relationships)
        {
            if (relationships.Count == 0) return null;
            var value = double.MinValue;
            for (var i = 0; i < relationships.Count; i++)
            {
                var r = relationships[i];
                if (r.Distance > value) value = r.Distance;
            }
            return value;
        }
    }
}
