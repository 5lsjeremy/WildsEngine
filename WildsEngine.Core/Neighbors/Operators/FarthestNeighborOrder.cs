namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class FarthestNeighborOrder : NeighborOrderOperatorBase
    {
        public FarthestNeighborOrder() : base(new NeighborDistanceDescendingComparer()) { }
    }
}
