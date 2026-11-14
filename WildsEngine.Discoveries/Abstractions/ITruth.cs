using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents scoped knowledge that has congealed from the
    /// composition of Discoveries and their geometric clusters.
    ///
    /// Truth remains attributable and scoped.
    ///
    /// It is not an assertion of universal consumer reality.
    /// </summary>
    public interface ITruth :
        IIdentifier
    {
        string Name { get; }

        object? Value { get; }

        IDiscoveryVectorSeries Series { get; }

        IReadOnlyList<IDiscoveryCluster> Clusters { get; }

        IReadOnlyList<IDiscovery> Discoveries { get; }

        double Stability { get; }

        double Confidence { get; }

        DateTimeOffset CongealedAt { get; }
    }
}
