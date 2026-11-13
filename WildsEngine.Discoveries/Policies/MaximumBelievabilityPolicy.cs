using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    /// <summary>
    /// Counts Evidence at or below a Believability ceiling.
    /// Useful for doubt, weak-rumor, and suspicious-source Facts.
    /// </summary>
    public sealed class MaximumBelievabilityPolicy :
        EvidenceContributionPolicyBase
    {
        public MaximumBelievabilityPolicy(
            double maximumBelievability)
        {
            MaximumBelievability =
                RequireNormalized(
                    maximumBelievability,
                    nameof(maximumBelievability));
        }

        public double MaximumBelievability { get; }

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return evidence.ProofResult.Believability <=
                   MaximumBelievability;
        }
    }
}
