using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    /// <summary>
    /// Counts sufficiently believable Evidence regardless of objective truth.
    /// </summary>
    public sealed class MinimumBelievabilityPolicy :
        EvidenceContributionPolicyBase
    {
        public MinimumBelievabilityPolicy(
            double minimumBelievability)
        {
            MinimumBelievability =
                RequireNormalized(
                    minimumBelievability,
                    nameof(minimumBelievability));
        }

        public double MinimumBelievability { get; }

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return evidence.ProofResult.Believability >=
                   MinimumBelievability;
        }
    }
}
