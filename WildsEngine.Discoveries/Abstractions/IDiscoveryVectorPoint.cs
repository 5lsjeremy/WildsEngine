using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents an immutable point in consumer-defined
    /// Discovery geometry.
    ///
    /// A point has coordinates but no independent identity,
    /// provenance, evaluation time, or temporal motion.
    /// </summary>
    public interface IDiscoveryVectorPoint
    {
        IReadOnlyList<IDiscoveryVectorComponent> Components { get; }

        int Dimension { get; }
    }
}
