using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterMappingRegistry
    {
        IReadOnlyList<IDiscoveryClusterMapping> Mappings { get; }
        void RegisterDocument(IDiscoveryClusterMappingDocument document);
        bool TryGet(Guid clusterId, out IDiscoveryClusterMapping? mapping);
        bool TryGet(string clusterSlug, out IDiscoveryClusterMapping? mapping);
        bool TryGetBySemanticKey(string semanticKey, out IDiscoveryClusterMapping? mapping);
        IReadOnlyList<IDiscoveryClusterMapping> GetByDocument(Guid documentId);
    }
}
