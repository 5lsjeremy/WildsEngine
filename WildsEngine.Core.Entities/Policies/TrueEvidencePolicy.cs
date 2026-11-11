using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Policies
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
