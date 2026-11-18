using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Surface;

namespace WildsEngine.Discoveries.Abstractions.Narratives
{
    public interface IDiscoveryRecognizedGeometryPopulation
    {
        Guid Id { get; }
        string Scope { get; }
        IDiscoveryGeometryRegistration Registration { get; }
        IReadOnlyList<IDiscoveryClusterRecognitionResult> Members { get; }
        IReadOnlyList<string> SemanticKeys { get; }
        DateTimeOffset CreatedAt { get; }
    }
}
