using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Surface;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Surface
{
    public sealed class DiscoveryGeometryRegistration : IdentifierBase, IDiscoveryGeometryRegistration
    {
        public DiscoveryGeometryRegistration(
            Guid id,
            string slug,
            Guid profileId,
            string profileSlug,
            string profileVersion,
            string scope,
            DateTimeOffset registeredAt,
            IReadOnlyList<Guid> mappingIds)
            : base(id, slug)
        {
            if (profileId == Guid.Empty) throw new ArgumentException("Profile ID is required.", nameof(profileId));
            if (string.IsNullOrWhiteSpace(profileSlug)) throw new ArgumentException("Profile slug is required.", nameof(profileSlug));
            if (string.IsNullOrWhiteSpace(profileVersion)) throw new ArgumentException("Profile version is required.", nameof(profileVersion));
            if (string.IsNullOrWhiteSpace(scope)) throw new ArgumentException("Scope is required.", nameof(scope));
            if (mappingIds == null || mappingIds.Count == 0) throw new ArgumentException("At least one mapping ID is required.", nameof(mappingIds));
            var copy = new Guid[mappingIds.Count];
            var unique = new HashSet<Guid>();
            for (var index = 0; index < mappingIds.Count; index++)
            {
                if (mappingIds[index] == Guid.Empty || !unique.Add(mappingIds[index])) throw new ArgumentException("Mapping IDs must be non-empty and unique.", nameof(mappingIds));
                copy[index] = mappingIds[index];
            }
            ProfileId = profileId;
            ProfileSlug = profileSlug.Trim();
            ProfileVersion = profileVersion.Trim();
            Scope = scope.Trim();
            RegisteredAt = registeredAt;
            MappingIds = new ReadOnlyCollection<Guid>(copy);
        }
        public Guid ProfileId { get; }
        public string ProfileSlug { get; }
        public string ProfileVersion { get; }
        public string Scope { get; }
        public DateTimeOffset RegisteredAt { get; }
        public IReadOnlyList<Guid> MappingIds { get; }
    }
}
