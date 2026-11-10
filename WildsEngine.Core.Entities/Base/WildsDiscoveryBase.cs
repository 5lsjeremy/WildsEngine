using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Entities.Base
{
    public abstract class WildsDiscoveryBase :
        BaseObject,
        IWildsDiscovery
    {
        private readonly IComponent _component;

        protected WildsDiscoveryBase(
            IDiscovery discovery,
            IComponent component,
            string layer)
            : base(
                RequireDiscovery(discovery).Id,
                layer)
        {
            Discovery =
                discovery;

            _component =
                component ??
                throw new ArgumentNullException(
                    nameof(component));
        }

        public IDiscovery Discovery { get; }

        public sealed override IComponent Component =>
            _component;

        private static IDiscovery RequireDiscovery(
            IDiscovery discovery)
        {
            return discovery ??
                   throw new ArgumentNullException(
                       nameof(discovery));
        }
    }
}