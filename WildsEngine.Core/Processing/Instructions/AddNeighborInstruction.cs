using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Processing
{
    public sealed class AddNeighborInstruction :
        StateInstructionBase
    {
        public const string IntentName =
            "topology.neighbor.add";

        public AddNeighborInstruction(
            IBaseObject neighbor,
            string membership,
            double distance,
            IDictionary<string, object?>? metadata = null)
            : base(
                IntentName,
                ValidateNeighbor(
                    neighbor),
                CreateArguments(
                    membership,
                    distance),
                metadata)
        {
            Neighbor =
                neighbor;

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

        private static IBaseObject ValidateNeighbor(
            IBaseObject neighbor)
        {
            return neighbor ??
                throw new ArgumentNullException(
                    nameof(neighbor));
        }

        private static IDictionary<string, object?>
            CreateArguments(
                string membership,
                double distance)
        {
            return new Dictionary<string, object?>
            {
                ["membership"] =
                    ValidateMembership(
                        membership),

                ["distance"] =
                    NeighborDistance.Validate(
                        distance,
                        nameof(distance))
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