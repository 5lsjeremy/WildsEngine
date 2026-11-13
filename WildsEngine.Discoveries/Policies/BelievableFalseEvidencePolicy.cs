using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    /// <summary>
    /// Counts convincing Evidence whose claim is nevertheless false.
    /// This is the central social-deduction and deception policy.
    /// </summary>
    public sealed class BelievableFalseEvidencePolicy :
        EvidenceContributionPolicyBase
    {
        public BelievableFalseEvidencePolicy(
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
            return !evidence.ProofResult.IsTrue &&
                   evidence.ProofResult.Believability >=
                   MinimumBelievability;
        }
    }
}
