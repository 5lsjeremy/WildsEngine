using System;
using WildsEngine.Discoveries.Abstractions.Clusters;
namespace WildsEngine.Discoveries.Clusters
{
    public sealed class DiscoveryClusterObservation : IDiscoveryClusterObservation
    {
        public DiscoveryClusterObservation(IDiscoveryClusterSpatialRelationship relationship)
        {
            Relationship=relationship??throw new ArgumentNullException(nameof(relationship));
            ObservedAt=relationship.Evaluation.EvaluatedAt;
        }
        public IDiscoveryClusterSpatialRelationship Relationship { get; }
        public DateTimeOffset ObservedAt { get; }
    }
}
