namespace WildsEngine.Discoveries.Abstractions.Vectorization
{
    public interface IDiscoveryVectorizerFactory
    {
        IDiscoveryVectorizer Create(
            IDiscoveryVectorizationService service,
            IDiscoveryVectorMetric metric);
    }
}
