using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    public sealed class MinimumStrengthPolicy :
        EvidenceContributionPolicyBase
    {
        public MinimumStrengthPolicy(
            double minimumStrength)
        {
            if (double.IsNaN(minimumStrength) ||
                double.IsInfinity(minimumStrength))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumStrength),
                    "Minimum strength must be finite.");
            }

            MinimumStrength =
                minimumStrength;
        }

        public double MinimumStrength { get; }

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return evidence.Strength >= MinimumStrength;
        }
    }
}
