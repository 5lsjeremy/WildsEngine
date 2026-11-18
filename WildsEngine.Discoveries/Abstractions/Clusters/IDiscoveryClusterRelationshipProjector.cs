using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterRelationshipProjector
    {
        IDiscoveryClusterSpatialRelationship Locate(
            IDiscoveryVectorEvaluation evaluation,
            IDiscoveryClusterMapping mapping);

        IDiscoveryClusterTrajectoryProjection Project(
            IDiscoveryVectorMotionResult motion,
            IDiscoveryClusterMapping mapping);
    }
}
