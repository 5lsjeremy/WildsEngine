namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NeighborWithinDistanceFilter : NeighborFilterOperatorBase
    {
        public NeighborWithinDistanceFilter(double maximumDistance) : base(new MaximumDistanceNeighborPredicate(maximumDistance)) { }
    }
}
