using System;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Surface;
namespace WildsEngine.Discoveries.Surface
{
    public sealed class DiscoveryGeometryObservationResult : IDiscoveryGeometryObservationResult
    {
        public DiscoveryGeometryObservationResult(IDiscoveryClusterSpatialRelationship spatial,IDiscoveryClusterPersistenceResult persistence,IDiscoveryClusterRecognitionResult recognition){Spatial=spatial??throw new ArgumentNullException(nameof(spatial));Persistence=persistence??throw new ArgumentNullException(nameof(persistence));Recognition=recognition??throw new ArgumentNullException(nameof(recognition));}
        public IDiscoveryClusterSpatialRelationship Spatial{get;} public IDiscoveryClusterPersistenceResult Persistence{get;} public IDiscoveryClusterRecognitionResult Recognition{get;}
    }
}
