using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Abstractions.Surface;

namespace WildsEngine.Discoveries.Narratives
{
    public sealed class DiscoveryRecognizedGeometryPopulation :
        IDiscoveryRecognizedGeometryPopulation
    {
        public DiscoveryRecognizedGeometryPopulation(
            Guid id,
            IDiscoveryGeometryRegistration registration,
            IReadOnlyList<IDiscoveryClusterRecognitionResult> members,
            DateTimeOffset createdAt)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Population identity is required.", nameof(id));
            Registration = registration ?? throw new ArgumentNullException(nameof(registration));
            if (members == null || members.Count == 0)
                throw new ArgumentException("At least one recognized geometry result is required.", nameof(members));

            var allowedMappings = new HashSet<Guid>(registration.MappingIds);
            var memberCopy = new IDiscoveryClusterRecognitionResult[members.Count];
            var semanticKeys = new List<string>();
            var seenSemanticKeys = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < members.Count; index++)
            {
                var member = members[index] ??
                    throw new ArgumentException("Population members cannot contain null.", nameof(members));
                if (!member.IsRecognized)
                    throw new ArgumentException("Every population member must be recognized.", nameof(members));
                if (!allowedMappings.Contains(member.Mapping.Id))
                    throw new ArgumentException("Every population member must belong to the supplied geometry registration.", nameof(members));
                memberCopy[index] = member;
                if (seenSemanticKeys.Add(member.Mapping.SemanticKey))
                    semanticKeys.Add(member.Mapping.SemanticKey);
            }

            semanticKeys.Sort(StringComparer.Ordinal);
            Id = id;
            Scope = registration.Scope;
            Members = new ReadOnlyCollection<IDiscoveryClusterRecognitionResult>(memberCopy);
            SemanticKeys = new ReadOnlyCollection<string>(semanticKeys);
            CreatedAt = createdAt;
        }

        public Guid Id { get; }
        public string Scope { get; }
        public IDiscoveryGeometryRegistration Registration { get; }
        public IReadOnlyList<IDiscoveryClusterRecognitionResult> Members { get; }
        public IReadOnlyList<string> SemanticKeys { get; }
        public DateTimeOffset CreatedAt { get; }
    }
}
