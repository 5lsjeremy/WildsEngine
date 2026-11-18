using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
namespace WildsEngine.Discoveries.Abstractions.Surface
{
    public interface IDiscoveryGeometryProfile : IWildsDiscoveriesProfile
    {
        IReadOnlyList<IDiscoveryClusterMapping> Mappings { get; }
        IDiscoveryClusterPersistencePolicy? PersistencePolicy { get; }
        IDiscoveryClusterStabilityMetric? StabilityMetric { get; }
        IDiscoveryClusterRecognitionPolicy? RecognitionPolicy { get; }
    }
}
