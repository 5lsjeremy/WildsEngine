using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Narratives
{
    public sealed class DiscoveryNarrativeRegistration : IdentifierBase, IDiscoveryNarrativeRegistration
    {
        public DiscoveryNarrativeRegistration(
            Guid id,
            IDiscoveryNarrativeProfile profile,
            DateTimeOffset registeredAt)
            : base(id, "narrative-registration-" + id.ToString("N"))
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (profile.CompositionPolicy.Id == Guid.Empty)
                throw new ArgumentException("Composition policy identity is required.", nameof(profile));
            if (profile.Metric.Id == Guid.Empty)
                throw new ArgumentException("Metric identity is required.", nameof(profile));
            var ids = new Guid[profile.Relationships.Count];
            var unique = new HashSet<Guid>();
            for (var index = 0; index < ids.Length; index++)
            {
                var relationshipId = profile.Relationships[index].Id;
                if (relationshipId == Guid.Empty || !unique.Add(relationshipId))
                    throw new ArgumentException("Relationship identities must be non-empty and unique.", nameof(profile));
                ids[index] = relationshipId;
            }
            ProfileId = profile.Id;
            ProfileSlug = profile.Slug;
            ProfileVersion = profile.Version;
            Scope = profile.Scope;
            RegisteredAt = registeredAt;
            CompositionPolicyId = profile.CompositionPolicy.Id;
            MetricId = profile.Metric.Id;
            RelationshipIds = new ReadOnlyCollection<Guid>(ids);
        }

        public Guid ProfileId { get; }
        public string ProfileSlug { get; }
        public string ProfileVersion { get; }
        public string Scope { get; }
        public DateTimeOffset RegisteredAt { get; }
        public Guid CompositionPolicyId { get; }
        public Guid MetricId { get; }
        public IReadOnlyList<Guid> RelationshipIds { get; }
    }
}
