namespace WildsEngine.Discoveries.Abstractions.Narratives
{
 public interface IDiscoveryNarrativeRelationshipDefinition : IIdentifier
 { string LeftSemanticKey { get; } string RightSemanticKey { get; } string RelationshipKey { get; } }
}
