using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents one normalized emission vector after
    /// propagation through one exact Neighbor relationship.
    ///
    /// The complete resolved and normalized provenance
    /// remains available through Source.
    /// </summary>
    public interface IPropagatedEmissionVector
    {
        INormalizedEmissionVector Source { get; }

        INeighborRelationship Relationship { get; }

        double PropagationDistance { get; }

        IReadOnlyList<IPropagatedEmission> Emissions { get; }

        int Dimension { get; }

        double ReceivedTotal { get; }
    }
}