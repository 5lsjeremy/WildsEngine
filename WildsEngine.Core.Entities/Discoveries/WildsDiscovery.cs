using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Entities.Concrete
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
