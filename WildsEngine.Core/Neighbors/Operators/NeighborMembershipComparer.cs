using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NeighborMembershipComparer : NeighborComparerBase
    {
        protected override int CompareCore(IBaseObject subject, INeighborRelationship left, INeighborRelationship right) =>
            string.Compare(left.Membership, right.Membership, StringComparison.Ordinal);
    }
}
