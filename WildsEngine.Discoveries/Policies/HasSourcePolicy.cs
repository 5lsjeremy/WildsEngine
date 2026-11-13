using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    public sealed class HasSourcePolicy :
        EvidenceContributionPolicyBase
    {
        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return evidence.Source != null;
        }
    }
}
