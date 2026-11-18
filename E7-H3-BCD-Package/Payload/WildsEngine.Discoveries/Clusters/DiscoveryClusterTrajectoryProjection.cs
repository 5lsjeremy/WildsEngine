using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Clusters
{
    public sealed class DiscoveryClusterTrajectoryProjection : IDiscoveryClusterTrajectoryProjection
    {
        public DiscoveryClusterTrajectoryProjection(
            IDiscoveryClusterSpatialRelationship spatial,
            IDiscoveryVectorMotionResult motion,
            DiscoveryClusterTransitKind transitKind,
            double radialRate,
            double timeToClosestApproachSeconds,
            double closestApproachDistanceToCentroid,
            double closestApproachDistanceToBoundary,
            double? timeToEntrySeconds,
            double? timeToExitSeconds,
            double? projectedDurationInsideSeconds,
            IReadOnlyDictionary<string,double>? entryPoint,
            IReadOnlyDictionary<string,double>? exitPoint)
        {
            Spatial = spatial ?? throw new ArgumentNullException(nameof(spatial));
            Motion = motion ?? throw new ArgumentNullException(nameof(motion));
            ProjectionBasis = "constant-velocity";
            TransitKind = transitKind;
            RadialRate = radialRate;
            IsApproaching = radialRate < 0.0;
            IsReceding = radialRate > 0.0;
            TimeToClosestApproachSeconds = timeToClosestApproachSeconds;
            ClosestApproachDistanceToCentroid = closestApproachDistanceToCentroid;
            ClosestApproachDistanceToBoundary = closestApproachDistanceToBoundary;
            TimeToEntrySeconds = timeToEntrySeconds;
            TimeToExitSeconds = timeToExitSeconds;
            ProjectedDurationInsideSeconds = projectedDurationInsideSeconds;
            EntryPoint = Copy(entryPoint);
            ExitPoint = Copy(exitPoint);
        }
        public IDiscoveryClusterSpatialRelationship Spatial { get; }
        public IDiscoveryVectorMotionResult Motion { get; }
        public string ProjectionBasis { get; }
        public DiscoveryClusterTransitKind TransitKind { get; }
        public double RadialRate { get; }
        public bool IsApproaching { get; }
        public bool IsReceding { get; }
        public double TimeToClosestApproachSeconds { get; }
        public double ClosestApproachDistanceToCentroid { get; }
        public double ClosestApproachDistanceToBoundary { get; }
        public double? TimeToEntrySeconds { get; }
        public double? TimeToExitSeconds { get; }
        public double? ProjectedDurationInsideSeconds { get; }
        public IReadOnlyDictionary<string, double>? EntryPoint { get; }
        public IReadOnlyDictionary<string, double>? ExitPoint { get; }
        private static IReadOnlyDictionary<string,double>? Copy(IReadOnlyDictionary<string,double>? source)
        { return source == null ? null : new ReadOnlyDictionary<string,double>(new Dictionary<string,double>(source,StringComparer.Ordinal)); }
    }
}
