using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Policies
{
    public sealed class MinimumNormalizedStrengthPolicy :
        EvidenceContributionPolicyBase
    {
        public MinimumNormalizedStrengthPolicy(
            double minimumNormalizedStrength)
        {
            MinimumNormalizedStrength =
                RequireNormalized(
                    minimumNormalizedStrength,
                    nameof(minimumNormalizedStrength));
        }

        public double MinimumNormalizedStrength { get; }

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return evidence.NormalizedStrength >=
                   MinimumNormalizedStrength;
        }
    }
}
