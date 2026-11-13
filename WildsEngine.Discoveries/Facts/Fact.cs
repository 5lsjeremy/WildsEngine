using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Concrete
{
    public sealed class Fact :
        FactBase
    {
        public Fact(
            IEntity entity,
            string name,
            object? value,
            int requiredEvidenceCount,
            IEvidenceContributionPolicy contributionPolicy,
            string? externalId = null)
            : base(
                entity,
                name,
                value,
                requiredEvidenceCount,
                contributionPolicy,
                externalId)
        {
        }

        public bool ReceiveEvidence(
            IDiscoveryEvidence evidence)
        {
            return AddEvidence(
                evidence);
        }

        public bool RemoveEvidenceItem(
            IDiscoveryEvidence evidence)
        {
            return RemoveEvidence(
                evidence);
        }

        public void UpdateValue(
            object? value)
        {
            SetValue(
                value);
        }
    }
}