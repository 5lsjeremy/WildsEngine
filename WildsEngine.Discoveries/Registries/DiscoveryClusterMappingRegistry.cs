using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;

namespace WildsEngine.Discoveries.Registries
{
    public sealed class DiscoveryClusterMappingRegistry : IDiscoveryClusterMappingRegistry
    {
        private readonly object _gate = new object();
        private readonly ConcurrentDictionary<Guid, IDiscoveryClusterMapping> _byId = new ConcurrentDictionary<Guid, IDiscoveryClusterMapping>();
        private readonly ConcurrentDictionary<string, IDiscoveryClusterMapping> _bySlug = new ConcurrentDictionary<string, IDiscoveryClusterMapping>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, IDiscoveryClusterMapping> _bySemantic = new ConcurrentDictionary<string, IDiscoveryClusterMapping>(StringComparer.Ordinal);

        public IReadOnlyList<IDiscoveryClusterMapping> Mappings
        {
            get
            {
                var result = new List<IDiscoveryClusterMapping>(_byId.Values);
                result.Sort((a, b) => string.CompareOrdinal(a.Slug, b.Slug));
                return result.AsReadOnly();
            }
        }

        public void RegisterDocument(IDiscoveryClusterMappingDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            lock (_gate)
            {
                var ids = new HashSet<Guid>();
                var slugs = new HashSet<string>(StringComparer.Ordinal);
                var keys = new HashSet<string>(StringComparer.Ordinal);
                foreach (var mapping in document.Mappings)
                {
                    if (mapping.DocumentId != document.Id) throw new InvalidOperationException("Mapping document identity mismatch.");
                    if (!ids.Add(mapping.Id) || _byId.ContainsKey(mapping.Id)) throw new InvalidOperationException("Duplicate cluster ID.");
                    if (!slugs.Add(mapping.Slug) || _bySlug.ContainsKey(mapping.Slug)) throw new InvalidOperationException("Duplicate cluster slug.");
                    if (!keys.Add(mapping.SemanticKey) || _bySemantic.ContainsKey(mapping.SemanticKey)) throw new InvalidOperationException("Duplicate semantic key.");
                }
                foreach (var mapping in document.Mappings)
                {
                    _byId[mapping.Id] = mapping;
                    _bySlug[mapping.Slug] = mapping;
                    _bySemantic[mapping.SemanticKey] = mapping;
                }
            }
        }

        public bool TryGet(Guid clusterId, out IDiscoveryClusterMapping? mapping) => _byId.TryGetValue(clusterId, out mapping);
        public bool TryGet(string clusterSlug, out IDiscoveryClusterMapping? mapping)
        {
            if (string.IsNullOrWhiteSpace(clusterSlug)) { mapping = null; return false; }
            return _bySlug.TryGetValue(clusterSlug, out mapping);
        }
        public bool TryGetBySemanticKey(string semanticKey, out IDiscoveryClusterMapping? mapping)
        {
            if (string.IsNullOrWhiteSpace(semanticKey)) { mapping = null; return false; }
            return _bySemantic.TryGetValue(semanticKey, out mapping);
        }
        public IReadOnlyList<IDiscoveryClusterMapping> GetByDocument(Guid documentId)
        {
            var result = new List<IDiscoveryClusterMapping>();
            foreach (var mapping in _byId.Values) if (mapping.DocumentId == documentId) result.Add(mapping);
            result.Sort((a, b) => string.CompareOrdinal(a.Slug, b.Slug));
            return result.AsReadOnly();
        }
    }
}
