using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NeighborDistanceDescendingComparer : NeighborComparerBase
    {
        protected override int CompareCore(IBaseObject subject, INeighborRelationship left, INeighborRelationship right) =>
            right.Distance.CompareTo(left.Distance);
    }
}
