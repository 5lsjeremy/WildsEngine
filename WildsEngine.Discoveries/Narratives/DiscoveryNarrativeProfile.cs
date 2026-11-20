using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Narratives
{
    public sealed class DiscoveryNarrativeProfile : IdentifierBase, IDiscoveryNarrativeProfile
    {
        public DiscoveryNarrativeProfile(
            string slug,
            string scope,
            IDiscoveryNarrativeCompositionPolicy compositionPolicy,
            IDiscoveryNarrativeMetric metric,
            IReadOnlyList<IDiscoveryNarrativeRelationshipDefinition> relationships)
            : this(Guid.NewGuid(), slug, "1.0", scope, compositionPolicy, metric, relationships)
        {
        }

        public DiscoveryNarrativeProfile(
            Guid id,
            string slug,
            string version,
            string scope,
            IDiscoveryNarrativeCompositionPolicy compositionPolicy,
            IDiscoveryNarrativeMetric metric,
            IReadOnlyList<IDiscoveryNarrativeRelationshipDefinition> relationships)
            : base(id, slug)
        {
            if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException("Version is required.", nameof(version));
            if (string.IsNullOrWhiteSpace(scope)) throw new ArgumentException("Scope is required.", nameof(scope));
            CompositionPolicy = compositionPolicy ?? throw new ArgumentNullException(nameof(compositionPolicy));
            Metric = metric ?? throw new ArgumentNullException(nameof(metric));
            if (relationships == null) throw new ArgumentNullException(nameof(relationships));

            var copy = new IDiscoveryNarrativeRelationshipDefinition[relationships.Count];
            var ids = new HashSet<Guid>();
            var slugs = new HashSet<string>(StringComparer.Ordinal);
            var pairs = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < relationships.Count; index++)
            {
                var relationship = relationships[index] ??
                    throw new ArgumentException("Relationships cannot contain null.", nameof(relationships));
                if (relationship.Id == Guid.Empty || !ids.Add(relationship.Id))
                    throw new ArgumentException("Relationship IDs must be non-empty and unique.", nameof(relationships));
                if (!slugs.Add(relationship.Slug))
                    throw new ArgumentException("Relationship slugs must be unique.", nameof(relationships));
                var pair = PairKey(relationship.LeftSemanticKey, relationship.RightSemanticKey);
                if (!pairs.Add(pair))
                    throw new ArgumentException("Only one relationship may be supplied for each unordered semantic pair.", nameof(relationships));
                copy[index] = relationship;
            }

            Version = version.Trim();
            Scope = scope.Trim();
            Relationships = new ReadOnlyCollection<IDiscoveryNarrativeRelationshipDefinition>(copy);
        }

        public string Version { get; }
        public string Scope { get; }
        public IDiscoveryNarrativeCompositionPolicy CompositionPolicy { get; }
        public IDiscoveryNarrativeMetric Metric { get; }
        public IReadOnlyList<IDiscoveryNarrativeRelationshipDefinition> Relationships { get; }

        private static string PairKey(string left, string right) =>
            string.CompareOrdinal(left, right) <= 0 ? left + "" + right : right + "" + left;
    }
}
