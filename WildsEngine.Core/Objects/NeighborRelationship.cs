using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Objects
{
    /// <summary>
    /// Immutable description of a directional Neighbor
    /// relationship between Wilds objects.
    ///
    /// Relationship identity is defined by the owning object,
    /// neighboring object, and membership. Distance describes
    /// the current proximity within that relationship and is
    /// not itself relationship identity.
    /// </summary>
    public sealed class NeighborRelationship :
        INeighborRelationship
    {
        public NeighborRelationship(
            IBaseObject neighbor,
            string membership,
            double distance)
        {
            Neighbor =
                neighbor ??
                throw new ArgumentNullException(
                    nameof(neighbor));

            Membership =
                ValidateMembership(
                    membership);

            Distance =
                NeighborDistance.Validate(
                    distance,
                    nameof(distance));
        }

        public IBaseObject Neighbor { get; }

        public string Membership { get; }

        public double Distance { get; }

        private static string ValidateMembership(
            string membership)
        {
            if (string.IsNullOrWhiteSpace(
                    membership))
            {
                throw new ArgumentException(
                    "Neighbor relationships require a membership.",
                    nameof(membership));
            }

            return membership.Trim();
        }
    }
}