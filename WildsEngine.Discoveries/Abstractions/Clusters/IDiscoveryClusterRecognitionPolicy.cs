namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterRecognitionPolicy : IIdentifier
    {
        bool Recognizes(IDiscoveryClusterPersistenceResult persistence);
    }
}
