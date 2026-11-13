using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    public sealed class BelievabilityRangePolicy :
        EvidenceContributionPolicyBase
    {
        public BelievabilityRangePolicy(
            double minimumBelievability,
            double maximumBelievability)
        {
            MinimumBelievability =
                RequireNormalized(
                    minimumBelievability,
                    nameof(minimumBelievability));

            MaximumBelievability =
                RequireNormalized(
                    maximumBelievability,
                    nameof(maximumBelievability));

            if (MinimumBelievability > MaximumBelievability)
            {
                throw new ArgumentException(
                    "Minimum Believability cannot exceed maximum Believability.");
            }
        }

        public double MinimumBelievability { get; }
        public double MaximumBelievability { get; }

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            var believability =
                evidence.ProofResult.Believability;

            return believability >= MinimumBelievability &&
                   believability <= MaximumBelievability;
        }
    }
}
