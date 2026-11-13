using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    /// <summary>
    /// Counts true Evidence only when it also meets a Believability threshold.
    /// </summary>
    public sealed class BelievableTrueEvidencePolicy :
        EvidenceContributionPolicyBase
    {
        public BelievableTrueEvidencePolicy(
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
            return evidence.ProofResult.IsTrue &&
                   evidence.ProofResult.Believability >=
                   MinimumBelievability;
        }
    }
}
