using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Finds geometric structure within immutable Discovery
    /// vectors.
    ///
    /// This is knowledge-space clustering, not Core topology
    /// clustering.
    /// </summary>
    public interface IDiscoveryClusterer
    {
        IReadOnlyList<IDiscoveryCluster> Cluster(
            IReadOnlyList<IDiscoveryVector> vectors);
    }
}
