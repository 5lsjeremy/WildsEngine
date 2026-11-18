using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Surface;

namespace WildsEngine.Discoveries.Abstractions.Narratives
{
    public interface IDiscoveryRecognizedGeometryPopulationFactory
    {
        IDiscoveryRecognizedGeometryPopulation Create(
            IDiscoveryGeometryRegistration registration,
            IReadOnlyList<IDiscoveryClusterRecognitionResult> members);
    }
}
