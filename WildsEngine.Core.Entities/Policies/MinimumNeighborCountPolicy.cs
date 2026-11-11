using System;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Policies
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
