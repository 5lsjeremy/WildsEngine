using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Consumer-supplied policy that maps one authoritative
    /// Discovery input into numeric vector contributions.
    ///
    /// The policy owns semantic interpretation.
    ///
    /// Discoveries owns validation and preservation of the
    /// returned provenance.
    /// </summary>
    public interface IDiscoveryVectorizationPolicy
    {
        IReadOnlyList<IDiscoveryVectorContribution> Vectorize(
            IDiscoveryVectorInput input);
    }
}
