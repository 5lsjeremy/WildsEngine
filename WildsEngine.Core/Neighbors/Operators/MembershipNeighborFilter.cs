namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class MembershipNeighborFilter : NeighborFilterOperatorBase
    {
        public MembershipNeighborFilter(string membership) : base(new MembershipNeighborPredicate(membership)) { }
    }
}
