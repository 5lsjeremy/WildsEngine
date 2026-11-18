using System;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Policies
{
    public sealed class RelationshipKeyNarrativeCompositionPolicy : IdentifierBase, IDiscoveryNarrativeCompositionPolicy
    {
        public RelationshipKeyNarrativeCompositionPolicy(
            string requiredRelationshipKey,
            string slug = "relationship-key-composition")
            : base(Guid.NewGuid(), slug)
        {
            if (string.IsNullOrWhiteSpace(requiredRelationshipKey))
                throw new ArgumentException("A required relationship key is required.", nameof(requiredRelationshipKey));
            RequiredRelationshipKey = requiredRelationshipKey.Trim();
        }

        public string RequiredRelationshipKey { get; }

        public bool CanCompose(
            IDiscoveryClusterRecognitionResult left,
            IDiscoveryClusterRecognitionResult right,
            double measurement,
            IDiscoveryNarrativeRelationshipDefinition? relationship)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));
            if (double.IsNaN(measurement) || double.IsInfinity(measurement))
                throw new ArgumentOutOfRangeException(nameof(measurement));
            return relationship != null &&
                   string.Equals(relationship.RelationshipKey, RequiredRelationshipKey, StringComparison.Ordinal);
        }
    }
}
