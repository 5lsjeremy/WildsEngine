namespace WildsEngine.Discoveries.Abstractions.Clusters
{
    public interface IDiscoveryClusterPersistencePolicy : IIdentifier
    {
        bool IsSatisfied(IDiscoveryClusterPersistenceResult result);
    }
}
