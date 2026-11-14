using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents one geometrically coherent region found within
    /// a Discovery-vector sample.
    ///
    /// A cluster is a derived result and has no independent
    /// domain or Wilds runtime identity.
    /// </summary>
    public interface IDiscoveryCluster
    {
        IReadOnlyList<IDiscoveryVector> Members { get; }

        IDiscoveryVectorPoint Centroid { get; }

        double Density { get; }

        double Dispersion { get; }

        double Coherence { get; }
    }
}
