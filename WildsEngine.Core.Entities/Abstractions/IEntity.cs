using System.Collections.Generic;

namespace WildsEngine.Core.Entities.Abstractions
{
    public interface IEntity :
        IIdentifier
    {
        IReadOnlyList<IFact> Facts { get; }

        IReadOnlyList<IDiscovery> Discoveries { get; }
    }
}