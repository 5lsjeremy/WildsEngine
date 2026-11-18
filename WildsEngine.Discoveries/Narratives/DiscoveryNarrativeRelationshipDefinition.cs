using System;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Narratives
{
    public sealed class DiscoveryNarrativeRelationshipDefinition :
        IdentifierBase,
        IDiscoveryNarrativeRelationshipDefinition
    {
        public DiscoveryNarrativeRelationshipDefinition(
            Guid id,
            string slug,
            string leftSemanticKey,
            string rightSemanticKey,
            string relationshipKey)
            : base(id, slug)
        {
            LeftSemanticKey = Require(leftSemanticKey, nameof(leftSemanticKey));
            RightSemanticKey = Require(rightSemanticKey, nameof(rightSemanticKey));
            RelationshipKey = Require(relationshipKey, nameof(relationshipKey));
            if (string.Equals(LeftSemanticKey, RightSemanticKey, StringComparison.Ordinal))
                throw new ArgumentException("A narrative relationship requires two distinct semantic keys.");
        }

        public string LeftSemanticKey { get; }
        public string RightSemanticKey { get; }
        public string RelationshipKey { get; }

        private static string Require(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Narrative relationship vocabulary cannot be empty.", parameterName);
            return value.Trim();
        }
    }
}
