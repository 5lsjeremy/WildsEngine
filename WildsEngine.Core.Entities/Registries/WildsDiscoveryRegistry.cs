using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Processing;

namespace WildsEngine.Core.Entities.Registries
{
    public sealed class WildsDiscoveryRegistry :
        RegistryBase<IWildsDiscovery>
    {
        public WildsDiscoveryRegistry(
            IComponent component,
            string layer)
            : base(
                component,
                layer,
                discovery => discovery.Id)
        {
        }

        protected override void OnInitialize()
        {
        }

        protected override void OnShutdown()
        {
            Clear();
        }
    }
}