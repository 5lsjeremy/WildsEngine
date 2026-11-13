using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Core.Tests.Entities.Fixtures
{
    public sealed class TestDiscovery :
        DiscoveryBase
    {
        public TestDiscovery(
            IEntity entity,
            int requiredFactCount,
            string? name = null,
            string? externalId = null)
            : base(
                entity,
                requiredFactCount,
                name,
                externalId)
        {
        }

        public bool AddFactItem(
            IFact fact)
        {
            return AddFact(
                fact);
        }

        public bool RemoveFactItem(
            IFact fact)
        {
            return RemoveFact(
                fact);
        }
    }
}
