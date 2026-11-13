using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    public interface IEntity :
        IIdentifier
    {
        IReadOnlyList<IFact> Facts { get; }

        IReadOnlyList<IDiscovery> Discoveries { get; }
    }
}