using System.Collections.Generic;
using WildsEngine.Core.Entities.Abstractions;

public interface IFact :
    IIdentifier
{
    IEntity Entity { get; }

    string Path { get; }

    object? Value { get; }

    IReadOnlyList<IDiscovery> Discoveries { get; }
}