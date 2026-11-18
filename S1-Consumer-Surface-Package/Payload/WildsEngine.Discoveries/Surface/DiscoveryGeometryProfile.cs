using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Surface;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Surface
{
    public sealed class DiscoveryGeometryProfile : IdentifierBase, IDiscoveryGeometryProfile
    {
        public DiscoveryGeometryProfile(
            Guid id,
            string slug,
            string version,
            string scope,
            IReadOnlyList<IDiscoveryClusterMapping> mappings)
            : base(id, slug)
        {
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("Profile version is required.", nameof(version));
            if (string.IsNullOrWhiteSpace(scope)) throw new ArgumentException("Profile scope is required.", nameof(scope));
            if (mappings == null || mappings.Count == 0) throw new ArgumentException("At least one cluster mapping is required.", nameof(mappings));
            var copy = new IDiscoveryClusterMapping[mappings.Count];
            var ids = new HashSet<Guid>();
            var slugs = new HashSet<string>(StringComparer.Ordinal);
            var semantics = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < mappings.Count; index++)
            {
                var mapping = mappings[index] ?? throw new ArgumentException("Mappings cannot contain null.", nameof(mappings));
                if (mapping.DocumentId != id) throw new ArgumentException("Every mapping must belong to the profile identity.", nameof(mappings));
                if (!string.Equals(mapping.DocumentVersion, version, StringComparison.Ordinal)) throw new ArgumentException("Every mapping must use the profile version.", nameof(mappings));
                if (!ids.Add(mapping.Id)) throw new ArgumentException("Mapping IDs must be unique within a profile.", nameof(mappings));
                if (!slugs.Add(mapping.Slug)) throw new ArgumentException("Mapping slugs must be unique within a profile.", nameof(mappings));
                if (!semantics.Add(mapping.SemanticKey)) throw new ArgumentException("Semantic keys must be unique within a profile.", nameof(mappings));
                copy[index] = mapping;
            }
            Version = version.Trim();
            Scope = scope.Trim();
            Mappings = new ReadOnlyCollection<IDiscoveryClusterMapping>(copy);
        }
        public string Version { get; }
        public string Scope { get; }
        public IReadOnlyList<IDiscoveryClusterMapping> Mappings { get; }
    }
}
