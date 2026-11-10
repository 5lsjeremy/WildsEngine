using System;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Base
{
    public abstract class DiscoveryBase :
        IdentifierBase,
        IDiscovery
    {
        protected DiscoveryBase(
            string slug,
            IFact fact)
            : base(slug)
        {
            Fact =
                fact ??
                throw new ArgumentNullException(
                    nameof(fact));
        }

        protected DiscoveryBase(
            Guid id,
            string slug,
            IFact fact)
            : base(
                id,
                slug)
        {
            Fact =
                fact ??
                throw new ArgumentNullException(
                    nameof(fact));
        }

        public IFact Fact { get; }
    }
}