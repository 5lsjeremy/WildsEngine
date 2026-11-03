namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NearestNeighborOrder : NeighborOrderOperatorBase
    {
        public NearestNeighborOrder() : base(new NeighborDistanceAscendingComparer()) { }
    }
}
