using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Provides a scoped, ordered view over immutable Discovery
    /// vector evaluations.
    ///
    /// A series does not establish a second temporal ownership
    /// model.
    ///
    /// Existing history infrastructure or a consumer may supply
    /// the ordered snapshots.
    /// </summary>
    public interface IDiscoveryVectorSeries
    {
        string Scope { get; }

        IReadOnlyList<IDiscoveryVectorInput> Inputs { get; }

        IReadOnlyList<IDiscoveryVector> Vectors { get; }

        IDiscoveryVector? Current { get; }

        IDiscoveryVector? Previous { get; }
    }
}
