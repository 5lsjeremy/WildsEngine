using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Vectors
{
    /// <summary>
    /// Concrete authoritative input selected for Discovery
    /// vectorization.
    ///
    /// The Discovery remains authoritative. Provenance is
    /// preserved opaquely and is not interpreted by Discoveries.
    /// </summary>
    public sealed class DiscoveryVectorInput :
        IDiscoveryVectorInput
    {
        public DiscoveryVectorInput(
            IDiscovery discovery,
            object? provenance = null)
        {
            Discovery =
                discovery ??
                throw new ArgumentNullException(
                    nameof(discovery));

            Provenance =
                provenance;
        }

        public IDiscovery Discovery { get; }

        public object? Provenance { get; }
    }
}