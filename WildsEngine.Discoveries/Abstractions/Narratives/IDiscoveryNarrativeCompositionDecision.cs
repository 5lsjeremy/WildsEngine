using WildsEngine.Discoveries.Abstractions.Clusters;

namespace WildsEngine.Discoveries.Abstractions.Narratives
{
    public interface IDiscoveryNarrativeCompositionDecision
    {
        IDiscoveryClusterRecognitionResult Left { get; }
        IDiscoveryClusterRecognitionResult Right { get; }
        double Measurement { get; }
        IDiscoveryNarrativeRelationshipDefinition? Relationship { get; }
        bool CanCompose { get; }
    }
}
