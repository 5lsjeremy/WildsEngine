using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents one immutable, provenance-bearing evaluation
    /// of Discovery geometry.
    ///
    /// A Discovery vector is not independently identifiable.
    /// It is an evaluation result over authoritative sources.
    /// </summary>
    public interface IDiscoveryVector :
        IDiscoveryVectorPoint
    {
        IReadOnlyList<IDiscoveryVectorContribution> Contributions { get; }

        IReadOnlyList<IDiscoveryVectorMotion> Motion { get; }

        IDiscoveryVectorMetric Metric { get; }

        double Magnitude { get; }

        DateTimeOffset EvaluatedAt { get; }
    }
}
