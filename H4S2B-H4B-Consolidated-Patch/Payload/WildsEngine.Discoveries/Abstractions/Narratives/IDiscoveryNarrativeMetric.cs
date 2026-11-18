using WildsEngine.Discoveries.Abstractions.Clusters;

namespace WildsEngine.Discoveries.Abstractions.Narratives
{
    public interface IDiscoveryNarrativeMetric : IIdentifier
    {
        double Measure(
            IDiscoveryClusterRecognitionResult left,
            IDiscoveryClusterRecognitionResult right);
    }
}
