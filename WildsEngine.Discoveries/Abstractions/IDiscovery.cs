using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    public interface IDiscovery :
        IResolutionThreshold<IFact>
    {
        string? ExternalId { get; }
        string? Name { get; }
        IEntity Entity { get; }
        IReadOnlyList<IFact> Facts { get; }
        int RequiredFactCount { get; }
        int CurrentFactCount { get; }
        double FactPercentage { get; }
        bool IsAvailable { get; }
    }
}
