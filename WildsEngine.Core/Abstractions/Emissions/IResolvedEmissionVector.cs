using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents an immutable ordered set of resolved
    /// emissions participating together in one propagation
    /// operation.
    ///
    /// The vector owns no normalization, propagation,
    /// eligibility, or receiver behavior.
    ///
    /// Every component must preserve the same exact source
    /// emitter.
    /// </summary>
    public interface IResolvedEmissionVector
    {
        IEmitter Source { get; }

        IReadOnlyList<IResolvedEmission> Emissions { get; }

        int Dimension { get; }

        double ResolvedTotal { get; }
    }
}