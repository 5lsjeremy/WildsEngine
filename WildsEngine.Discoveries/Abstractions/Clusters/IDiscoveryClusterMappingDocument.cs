using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterMappingDocument : IIdentifier
    {
        string Version { get; }
        string SourceName { get; }
        IReadOnlyList<IDiscoveryClusterMapping> Mappings { get; }
    }
}
