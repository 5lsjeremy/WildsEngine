using System.Collections.Generic;
namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterStabilityMetric : IIdentifier
    {
        double GetStability(IReadOnlyList<IDiscoveryClusterObservation> observations);
    }
}
