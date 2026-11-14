using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents a consumer-defined projected outcome derived
    /// from one or more scoped Truths.
    ///
    /// A Consequence describes what may result. It does not
    /// prescribe behavior.
    /// </summary>
    public interface IConsequence :
        IIdentifier
    {
        string Name { get; }

        object? Value { get; }

        IReadOnlyList<ITruth> Truths { get; }

        double Magnitude { get; }

        double Confidence { get; }

        DateTimeOffset ProjectedAt { get; }
    }
}
