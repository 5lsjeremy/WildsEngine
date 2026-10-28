using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing
{
    public sealed class AddNeighborInstruction :
        StateInstructionBase
    {
        public const string IntentName =
            "topology.neighbor.add";

        public AddNeighborInstruction(
            IBaseObject neighbor,
            IDictionary<string, object?>? metadata = null)
            : base(
                IntentName,
                ValidateNeighbor(neighbor),
                metadata: metadata)
        {
            Neighbor = neighbor;
        }

        public IBaseObject Neighbor { get; }

        private static IBaseObject ValidateNeighbor(
            IBaseObject neighbor)
        {
            return neighbor
                ?? throw new ArgumentNullException(
                    nameof(neighbor));
        }
    }
}

