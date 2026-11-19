using System.Collections.Generic;
namespace WildsEngine.Discoveries.Abstractions.Narratives
{
    public interface IDiscoveryNarrativeGroupRelationship
    {
        IDiscoveryNarrativeGroup Left { get; }
        IDiscoveryNarrativeGroup Right { get; }
        IReadOnlyList<IDiscoveryNarrativeCompositionDecision> Decisions { get; }
        IReadOnlyList<IDiscoveryNarrativeRelationshipDefinition> Definitions { get; }
    }
}
