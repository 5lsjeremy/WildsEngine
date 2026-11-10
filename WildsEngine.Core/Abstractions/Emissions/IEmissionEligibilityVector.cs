using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents eligibility decisions for every component
    /// of one exact propagated emission vector.
    ///
    /// Dimension, ordering, channel identity, source
    /// provenance, and relationship provenance remain
    /// available through Source.
    /// </summary>
    public interface IEmissionEligibilityVector
    {
        IPropagatedEmissionVector Source { get; }

        IReadOnlyList<IEmissionEligibilityResult> Emissions { get; }

        int Dimension { get; }

        int EligibleCount { get; }

        bool AnyEligible { get; }

        bool AllEligible { get; }
    }
}