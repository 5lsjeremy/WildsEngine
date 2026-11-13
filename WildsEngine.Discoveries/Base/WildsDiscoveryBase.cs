using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    public abstract class WildsDiscoveryBase :
        WildsProjectionBase<IDiscovery>,
        IWildsDiscovery
    {
        protected WildsDiscoveryBase(
            IDiscovery discovery,
            IComponent component,
            Guid id,
            string layer)
            : base(
                discovery,
                component,
                id,
                layer)
        {
        }

        public IDiscovery Discovery =>
            Source;
    }
}
