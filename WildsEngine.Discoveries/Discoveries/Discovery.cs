using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Concrete
{
    public sealed class Discovery :
        DiscoveryBase
    {
        public Discovery(
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

        public bool ReceiveFact(
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