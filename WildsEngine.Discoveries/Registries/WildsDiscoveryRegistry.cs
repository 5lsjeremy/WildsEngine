using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Core.Processing;

namespace WildsEngine.Discoveries.Registries
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