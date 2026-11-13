using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    /// <summary>
    /// Counts true Evidence that remains below a Believability ceiling.
    /// Useful for dismissed truths and warnings nobody accepts.
    /// </summary>
    public sealed class TrueButUnbelievableEvidencePolicy :
        EvidenceContributionPolicyBase
    {
        public TrueButUnbelievableEvidencePolicy(
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
            return evidence.ProofResult.IsTrue &&
                   evidence.ProofResult.Believability <=
                   MaximumBelievability;
        }
    }
}
