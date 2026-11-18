using WildsEngine.Discoveries.Abstractions.Clusters;

namespace WildsEngine.Discoveries.Abstractions.Narratives
{
    public interface IDiscoveryNarrativeCompositionPolicy : IIdentifier
    {
        bool CanCompose(
            IDiscoveryClusterRecognitionResult left,
            IDiscoveryClusterRecognitionResult right,
            double measurement,
            IDiscoveryNarrativeRelationshipDefinition? relationship);
    }
}
