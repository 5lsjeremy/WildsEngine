using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Consumer-supplied policy that determines whether Discovery
    /// geometry has become stable enough to produce a scoped
    /// Truth.
    /// </summary>
    public interface ITruthCongealmentPolicy
    {
        bool CanCongeal(
            IReadOnlyList<IDiscoveryCluster> clusters,
            IDiscoveryVectorSeries series);

        ITruth Congeal(
            IReadOnlyList<IDiscoveryCluster> clusters,
            IDiscoveryVectorSeries series);
    }
}
