using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterSpatialRelationship
    {
        IDiscoveryClusterMapping Mapping { get; }
        IDiscoveryVectorEvaluation Evaluation { get; }
        double DistanceToCentroid { get; }
        double SignedDistanceToBoundary { get; }
        bool IsInside { get; }
        bool IsOnBoundary { get; }
        IReadOnlyDictionary<string, double> NearestBoundaryPoint { get; }
    }
}
