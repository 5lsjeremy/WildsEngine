using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Policies
{
    /// <summary>
    /// Counts every Evidence item that reaches policy evaluation.
    /// FactBase must still enforce validation and exact provenance first.
    /// </summary>
    public sealed class AnyEvidencePolicy :
        EvidenceContributionPolicyBase
    {
        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return true;
        }
    }
}
