using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Vectors
{
    /// <summary>
    /// Concrete immutable evaluation of Discovery geometry.
    ///
    /// Structural validation, immutable snapshots, and magnitude
    /// calculation remain owned by DiscoveryVectorBase.
    /// </summary>
    public sealed class DiscoveryVector :
        DiscoveryVectorBase
    {
        public DiscoveryVector(
            IReadOnlyList<IDiscoveryVectorComponent> components,
            IReadOnlyList<IDiscoveryVectorContribution> contributions,
            IReadOnlyList<IDiscoveryVectorMotion> motion,
            IDiscoveryVectorMetric metric,
            DateTimeOffset evaluatedAt)
            : base(
                components,
                contributions,
                motion,
                metric,
                evaluatedAt)
        {
        }
    }
}