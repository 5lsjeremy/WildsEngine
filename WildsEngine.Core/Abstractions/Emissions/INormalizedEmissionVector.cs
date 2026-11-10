using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents the immutable normalized form of one
    /// resolved-emission vector.
    ///
    /// The source vector and exact emission references remain
    /// authoritative. Normalized components must preserve
    /// dimension and ordering.
    /// </summary>
    public interface INormalizedEmissionVector
    {
        IResolvedEmissionVector Source { get; }

        IReadOnlyList<INormalizedEmission> Emissions { get; }

        int Dimension { get; }

        double NormalizedTotal { get; }
    }
}