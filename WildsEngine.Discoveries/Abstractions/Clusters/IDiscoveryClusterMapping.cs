using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterMapping : IIdentifier
    {
        Guid DocumentId { get; }
        string DocumentSlug { get; }
        string DocumentVersion { get; }
        string SemanticKey { get; }
        string GeometryType { get; }
        IReadOnlyDictionary<string, double> Centroid { get; }
        double Radius { get; }
        IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
