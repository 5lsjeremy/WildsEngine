namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NeighborDistanceRangeFilter : NeighborFilterOperatorBase
    {
        public NeighborDistanceRangeFilter(double minimumDistance, double maximumDistance)
            : base(new DistanceRangeNeighborPredicate(minimumDistance, maximumDistance)) { }
    }
}
