using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Processing
{
    public sealed class RemoveNeighborInstruction :
        StateInstruction
    {
        public const string IntentName =
            "topology.neighbor.remove";

        public RemoveNeighborInstruction(
            Guid neighborId,
            IDictionary<string, object?>? metadata = null)
            : base(
                IntentName,
                ValidateId(neighborId),
                metadata: metadata)
        {
            NeighborId = neighborId;
        }

        public Guid NeighborId { get; }

        private static Guid ValidateId(Guid neighborId)
        {
            if (neighborId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Neighbor removal requires a non-empty GUID.",
                    nameof(neighborId));
            }

            return neighborId;
        }
    }
}

