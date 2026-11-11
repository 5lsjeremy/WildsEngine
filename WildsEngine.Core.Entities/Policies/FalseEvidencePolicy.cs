using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Policies
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
