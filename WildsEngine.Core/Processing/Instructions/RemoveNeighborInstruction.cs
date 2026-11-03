using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Processing
{
    public sealed class RemoveNeighborInstruction :
        StateInstructionBase
    {
        public const string IntentName =
            "topology.neighbor.remove";

        public RemoveNeighborInstruction(
            Guid neighborId,
            string membership,
            IDictionary<string, object?>? metadata = null)
            : base(
                IntentName,
                ValidateId(
                    neighborId),
                CreateArguments(
                    membership),
                metadata)
        {
            NeighborId =
                neighborId;

            Membership =
                ValidateMembership(
                    membership);
        }

        public Guid NeighborId { get; }

        public string Membership { get; }

        private static Guid ValidateId(
            Guid neighborId)
        {
            if (neighborId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Neighbor removal requires a non-empty GUID.",
                    nameof(neighborId));
            }

            return neighborId;
        }

        private static IDictionary<string, object?>
            CreateArguments(
                string membership)
        {
            return new Dictionary<string, object?>
            {
                ["membership"] =
                    ValidateMembership(
                        membership)
            };
        }

        private static string ValidateMembership(
            string membership)
        {
            if (string.IsNullOrWhiteSpace(
                    membership))
            {
                throw new ArgumentException(
                    "Neighbor membership cannot be empty.",
                    nameof(membership));
            }

            return membership.Trim();
        }
    }
}