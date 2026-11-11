using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Entities.Concrete
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