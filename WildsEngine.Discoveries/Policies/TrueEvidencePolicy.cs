using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    /// <summary>
    /// Counts Evidence whose Proof concludes that the claim is true.
    /// Believability does not affect the decision.
    /// </summary>
    public sealed class TrueEvidencePolicy :
        EvidenceContributionPolicyBase
    {
        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return evidence.ProofResult.IsTrue;
        }
    }
}
