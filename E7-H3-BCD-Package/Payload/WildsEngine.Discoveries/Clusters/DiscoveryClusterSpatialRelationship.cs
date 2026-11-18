using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Clusters
{
    public sealed class DiscoveryClusterSpatialRelationship : IDiscoveryClusterSpatialRelationship
    {
        public DiscoveryClusterSpatialRelationship(
            IDiscoveryClusterMapping mapping,
            IDiscoveryVectorEvaluation evaluation,
            double distanceToCentroid,
            double signedDistanceToBoundary,
            bool isInside,
            bool isOnBoundary,
            IReadOnlyDictionary<string, double> nearestBoundaryPoint)
        {
            Mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            Evaluation = evaluation ?? throw new ArgumentNullException(nameof(evaluation));
            DistanceToCentroid = Finite(distanceToCentroid, nameof(distanceToCentroid));
            SignedDistanceToBoundary = Finite(signedDistanceToBoundary, nameof(signedDistanceToBoundary));
            IsInside = isInside;
            IsOnBoundary = isOnBoundary;
            if (nearestBoundaryPoint == null) throw new ArgumentNullException(nameof(nearestBoundaryPoint));
            NearestBoundaryPoint = new ReadOnlyDictionary<string, double>(
                new Dictionary<string, double>(nearestBoundaryPoint, StringComparer.Ordinal));
        }
        public IDiscoveryClusterMapping Mapping { get; }
        public IDiscoveryVectorEvaluation Evaluation { get; }
        public double DistanceToCentroid { get; }
        public double SignedDistanceToBoundary { get; }
        public bool IsInside { get; }
        public bool IsOnBoundary { get; }
        public IReadOnlyDictionary<string, double> NearestBoundaryPoint { get; }
        private static double Finite(double value, string name)
        { if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(name); return value; }
    }
}
