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
            string slug,
            string scope,
            IReadOnlyList<IDiscoveryClusterMapping> mappings,
            IDiscoveryClusterPersistencePolicy? persistencePolicy = null,
            IDiscoveryClusterStabilityMetric? stabilityMetric = null,
            IDiscoveryClusterRecognitionPolicy? recognitionPolicy = null)
            : this(Guid.NewGuid(), slug, "1.0", scope, mappings,
                persistencePolicy, stabilityMetric, recognitionPolicy)
        {
        }

        public DiscoveryGeometryProfile(
            Guid id,
            string slug,
            string version,
            string scope,
            IReadOnlyList<IDiscoveryClusterMapping> mappings,
            IDiscoveryClusterPersistencePolicy? persistencePolicy = null,
            IDiscoveryClusterStabilityMetric? stabilityMetric = null,
            IDiscoveryClusterRecognitionPolicy? recognitionPolicy = null)
            : base(id, slug)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Profile version is required.", nameof(version));
            if (string.IsNullOrWhiteSpace(scope))
                throw new ArgumentException("Profile scope is required.", nameof(scope));
            if (mappings == null || mappings.Count == 0)
                throw new ArgumentException("At least one cluster mapping is required.", nameof(mappings));
            if ((persistencePolicy == null) != (stabilityMetric == null))
                throw new ArgumentException("Persistence policy and stability metric must be supplied together.");
            if (recognitionPolicy != null && persistencePolicy == null)
                throw new ArgumentException("Recognition requires persistence configuration.");

            var copy = new IDiscoveryClusterMapping[mappings.Count];
            var ids = new HashSet<Guid>();
            var slugs = new HashSet<string>(StringComparer.Ordinal);
            var semantics = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < mappings.Count; i++)
            {
                var m = mappings[i] ?? throw new ArgumentException("Mappings cannot contain null.", nameof(mappings));
                if (m.DocumentId != id)
                    throw new ArgumentException("Every mapping must belong to the profile identity.", nameof(mappings));
                if (!string.Equals(m.DocumentVersion, version, StringComparison.Ordinal))
                    throw new ArgumentException("Every mapping must use the profile version.", nameof(mappings));
                if (!ids.Add(m.Id) || !slugs.Add(m.Slug) || !semantics.Add(m.SemanticKey))
                    throw new ArgumentException("Mapping identity, slug, and semantic keys must be unique.", nameof(mappings));
                copy[i] = m;
            }

            Version = version.Trim();
            Scope = scope.Trim();
            Mappings = new ReadOnlyCollection<IDiscoveryClusterMapping>(copy);
            PersistencePolicy = persistencePolicy;
            StabilityMetric = stabilityMetric;
            RecognitionPolicy = recognitionPolicy;
        }

        public string Version { get; }
        public string Scope { get; }
        public IReadOnlyList<IDiscoveryClusterMapping> Mappings { get; }
        public IDiscoveryClusterPersistencePolicy? PersistencePolicy { get; }
        public IDiscoveryClusterStabilityMetric? StabilityMetric { get; }
        public IDiscoveryClusterRecognitionPolicy? RecognitionPolicy { get; }
    }
}
