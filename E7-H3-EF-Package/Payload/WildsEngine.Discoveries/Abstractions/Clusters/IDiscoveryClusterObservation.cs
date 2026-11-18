using System;
namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterObservation
    {
        IDiscoveryClusterSpatialRelationship Relationship { get; }
        DateTimeOffset ObservedAt { get; }
    }
}
