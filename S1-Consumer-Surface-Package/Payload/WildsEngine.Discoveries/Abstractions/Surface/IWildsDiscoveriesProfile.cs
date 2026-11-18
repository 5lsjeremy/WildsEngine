using System;

namespace WildsEngine.Discoveries.Abstractions.Surface
{
    public interface IWildsDiscoveriesProfile : IIdentifier
    {
        string Version { get; }
        string Scope { get; }
    }
}
