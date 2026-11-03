namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class AdjacentNeighborFilter : NeighborFilterOperatorBase
    {
        public AdjacentNeighborFilter() : base(new AdjacentNeighborPredicate()) { }
    }
}
