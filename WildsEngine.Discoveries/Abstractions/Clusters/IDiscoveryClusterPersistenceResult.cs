using System;
using System.Collections.Generic;
namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterPersistenceResult
    {
        IDiscoveryClusterMapping Mapping { get; }
        IReadOnlyList<IDiscoveryClusterObservation> Observations { get; }
        int ObservationCount { get; }
        int InsideObservationCount { get; }
        int ConsecutiveInsideCount { get; }
        double OccupancyRatio { get; }
        DateTimeOffset? ObservedEntryAt { get; }
        DateTimeOffset? ObservedExitAt { get; }
        TimeSpan? ObservedDurationInside { get; }
        double Stability { get; }
        bool IsPersistent { get; }
        Guid PersistencePolicyId { get; }
        string PersistencePolicySlug { get; }
        Guid StabilityMetricId { get; }
        string StabilityMetricSlug { get; }
    }
}
