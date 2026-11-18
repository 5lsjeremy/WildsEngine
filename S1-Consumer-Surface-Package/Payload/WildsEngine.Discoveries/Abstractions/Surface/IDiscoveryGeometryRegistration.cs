using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions.Surface
{
    public interface IDiscoveryGeometryRegistration : IWildsDiscoveriesRegistration
    {
        IReadOnlyList<Guid> MappingIds { get; }
    }
}
