using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Concrete
{
    public sealed class WildsDiscovery :
        WildsDiscoveryBase
    {
        public WildsDiscovery(
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
    }
}
