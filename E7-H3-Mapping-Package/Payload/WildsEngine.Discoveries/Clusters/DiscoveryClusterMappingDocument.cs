using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Clusters
{
    public sealed class DiscoveryClusterMappingDocument : IdentifierBase, IDiscoveryClusterMappingDocument
    {
        public DiscoveryClusterMappingDocument(
            Guid id, string slug, string version, string sourceName,
            IReadOnlyList<IDiscoveryClusterMapping> mappings)
            : base(id, slug)
        {
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("Version is required.", nameof(version));
            if (string.IsNullOrWhiteSpace(sourceName)) throw new ArgumentException("Source name is required.", nameof(sourceName));
            if (mappings == null || mappings.Count == 0) throw new ArgumentException("At least one mapping is required.", nameof(mappings));
            var copy = new IDiscoveryClusterMapping[mappings.Count];
            for (var i = 0; i < mappings.Count; i++) copy[i] = mappings[i] ?? throw new ArgumentException("Mappings cannot contain null.", nameof(mappings));
            Version = version.Trim();
            SourceName = sourceName.Trim();
            Mappings = new ReadOnlyCollection<IDiscoveryClusterMapping>(copy);
        }
        public string Version { get; }
        public string SourceName { get; }
        public IReadOnlyList<IDiscoveryClusterMapping> Mappings { get; }
    }
}
