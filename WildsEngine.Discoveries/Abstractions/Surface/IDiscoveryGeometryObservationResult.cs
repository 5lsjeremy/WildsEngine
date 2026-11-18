using WildsEngine.Discoveries.Abstractions.Clusters;
namespace WildsEngine.Discoveries.Abstractions.Surface
{
    public interface IDiscoveryGeometryObservationResult
    {
        IDiscoveryClusterSpatialRelationship Spatial { get; }
        IDiscoveryClusterPersistenceResult Persistence { get; }
        IDiscoveryClusterRecognitionResult Recognition { get; }
    }
}
