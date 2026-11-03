using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NeighborDistanceAscendingComparer : NeighborComparerBase
    {
        protected override int CompareCore(IBaseObject subject, INeighborRelationship left, INeighborRelationship right) =>
            left.Distance.CompareTo(right.Distance);
    }
}
