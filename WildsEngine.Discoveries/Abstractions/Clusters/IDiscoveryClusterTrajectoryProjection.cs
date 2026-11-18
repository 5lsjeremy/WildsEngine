using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterTrajectoryProjection
    {
        IDiscoveryClusterSpatialRelationship Spatial { get; }
        IDiscoveryVectorMotionResult Motion { get; }
        string ProjectionBasis { get; }
        DiscoveryClusterTransitKind TransitKind { get; }
        double RadialRate { get; }
        bool IsApproaching { get; }
        bool IsReceding { get; }
        double TimeToClosestApproachSeconds { get; }
        double ClosestApproachDistanceToCentroid { get; }
        double ClosestApproachDistanceToBoundary { get; }
        double? TimeToEntrySeconds { get; }
        double? TimeToExitSeconds { get; }
        double? ProjectedDurationInsideSeconds { get; }
        IReadOnlyDictionary<string, double>? EntryPoint { get; }
        IReadOnlyDictionary<string, double>? ExitPoint { get; }
    }
}
