using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    /// <summary>
    /// Counts Evidence whose Proof concludes that the claim is false.
    /// Useful for disproven claims, deception analysis, and contradiction Facts.
    /// </summary>
    public sealed class FalseEvidencePolicy :
        EvidenceContributionPolicyBase
    {
        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return !evidence.ProofResult.IsTrue;
        }
    }
}
