using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class MembershipNeighborPredicate : NeighborPredicateBase
    {
        private readonly string _membership;

        public MembershipNeighborPredicate(string membership)
        {
            if (string.IsNullOrWhiteSpace(membership))
                throw new ArgumentException("Neighbor membership cannot be empty.", nameof(membership));
            _membership = membership.Trim();
        }

        public string Membership => _membership;

        protected override bool IncludesCore(IBaseObject subject, INeighborRelationship relationship) =>
            string.Equals(relationship.Membership, _membership, StringComparison.Ordinal);
    }
}
