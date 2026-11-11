using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Policies
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
