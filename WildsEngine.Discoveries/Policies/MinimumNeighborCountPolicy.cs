using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    public sealed class MinimumNeighborCountPolicy :
        EvidenceContributionPolicyBase
    {
        public MinimumNeighborCountPolicy(
            int minimumNeighborCount)
        {
            if (minimumNeighborCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumNeighborCount));
            }

            MinimumNeighborCount =
                minimumNeighborCount;
        }

        public int MinimumNeighborCount { get; }

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return evidence.Neighbors.Count >=
                   MinimumNeighborCount;
        }
    }
}
