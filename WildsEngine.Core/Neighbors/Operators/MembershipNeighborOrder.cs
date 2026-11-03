namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class MembershipNeighborOrder : NeighborOrderOperatorBase
    {
        public MembershipNeighborOrder() : base(new NeighborMembershipComparer()) { }
    }
}
