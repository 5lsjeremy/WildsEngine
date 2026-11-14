using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Reusable immutable geometric cluster over Discovery
    /// vectors.
    ///
    /// The centroid is a geometric point, not a complete
    /// provenance-bearing vector evaluation.
    /// </summary>
    public abstract class DiscoveryClusterBase :
        IDiscoveryCluster
    {
        protected DiscoveryClusterBase(
            IReadOnlyList<IDiscoveryVector> members,
            IDiscoveryVectorPoint centroid,
            double density,
            double dispersion,
            double coherence)
        {
            if (members == null)
            {
                throw new ArgumentNullException(
                    nameof(members));
            }

            if (members.Count == 0)
            {
                throw new ArgumentException(
                    "A Discovery cluster requires at least one member.",
                    nameof(members));
            }

            Members =
                DiscoverySnapshots.Required(
                    members,
                    nameof(members));

            Centroid =
                centroid ??
                throw new ArgumentNullException(
                    nameof(centroid));

            Density =
                DiscoveryNumbers.Unit(
                    density,
                    nameof(density));

            Dispersion =
                DiscoveryNumbers.NonNegative(
                    dispersion,
                    nameof(dispersion));

            Coherence =
                DiscoveryNumbers.Unit(
                    coherence,
                    nameof(coherence));
        }

        public IReadOnlyList<IDiscoveryVector> Members { get; }

        public IDiscoveryVectorPoint Centroid { get; }

        public double Density { get; }

        public double Dispersion { get; }

        public double Coherence { get; }
    }
}
