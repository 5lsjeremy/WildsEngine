using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Tests.Entities.Fixtures
{
    /// <summary>
    /// Minimal test surface over FactBase.
    ///
    /// Adds no Fact semantics of its own.
    /// It only exposes the protected Evidence lifecycle
    /// so the production FactBase implementation and
    /// production contribution policies can be certified.
    /// </summary>
    public sealed class TestFact :
        FactBase
    {
        public TestFact(
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

        public bool AddEvidenceItem(
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
    }
}