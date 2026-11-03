using System;
using System.Collections.Generic;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Processing
{
    public sealed class SetNeighborDistanceInstruction : StateInstructionBase
    {
        public const string IntentName = "topology.neighbor.distance.set";

        public SetNeighborDistanceInstruction(
            Guid neighborId,
            string membership,
            double distance,
            IDictionary<string, object?>? metadata = null)
            : base(IntentName, ValidateId(neighborId), CreateArguments(membership, distance), metadata)
        {
            NeighborId = neighborId;
            Membership = ValidateMembership(membership);
            Distance = NeighborDistance.Validate(distance, nameof(distance));
        }

        public Guid NeighborId { get; }
        public string Membership { get; }
        public double Distance { get; }

        private static Guid ValidateId(Guid value)
        {
            if (value == Guid.Empty) throw new ArgumentException("Neighbor distance update requires a non-empty GUID.", nameof(value));
            return value;
        }

        private static string ValidateMembership(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Neighbor membership cannot be empty.", nameof(value));
            return value.Trim();
        }

        private static IDictionary<string, object?> CreateArguments(string membership, double distance) =>
            new Dictionary<string, object?>
            {
                ["membership"] = ValidateMembership(membership),
                ["distance"] = NeighborDistance.Validate(distance, nameof(distance))
            };
    }
}
