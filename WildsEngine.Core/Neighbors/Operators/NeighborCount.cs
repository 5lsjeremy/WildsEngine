using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NeighborCount : NeighborOperatorBase<int>, INeighborScalarOperator<int>
    {
        protected override int ApplyCore(IBaseObject subject, IReadOnlyList<INeighborRelationship> relationships) => relationships.Count;
    }
}
