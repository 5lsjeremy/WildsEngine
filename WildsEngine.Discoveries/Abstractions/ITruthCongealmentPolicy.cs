using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Narratives;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Consumer-supplied policy that determines whether scoped
    /// Discovery knowledge has become sufficient to produce Truth.
    ///
    /// Truth congealment is allowed to consider:
    ///
    /// - the competing narratives produced from recognized knowledge,
    /// - the geometric clusters underlying that knowledge, and
    /// - the authoritative vector series from which it developed.
    ///
    /// Narratives inform the Truth decision but do not dictate it.
    /// The consumer remains responsible for deciding whether Truth
    /// may congeal and, when it does, what that Truth means.
    /// </summary>
    public interface ITruthCongealmentPolicy
    {
        /// <summary>
        /// Determines whether the supplied narrative, geometry, and
        /// vector-history context is sufficient for Truth to congeal.
        /// </summary>
        bool CanCongeal(
            IDiscoveryCompetingNarrativesResult narratives,
            IReadOnlyList<IDiscoveryCluster> clusters,
            IDiscoveryVectorSeries series);

        /// <summary>
        /// Produces the consumer-defined Truth after CanCongeal has
        /// authorized congealment.
        ///
        /// The same competing narratives, clusters, and vector series
        /// considered during authorization are supplied here so the
        /// consumer can derive Truth from the complete knowledge
        /// context without Discoveries choosing its meaning.
        /// </summary>
        ITruth Congeal(
            IDiscoveryCompetingNarrativesResult narratives,
            IReadOnlyList<IDiscoveryCluster> clusters,
            IDiscoveryVectorSeries series);
    }
}