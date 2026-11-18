using System;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Vectorization;

namespace WildsEngine.Discoveries.Vectorization
{
    public sealed class DiscoveryVectorizerFactory : IDiscoveryVectorizerFactory
    {
        public IDiscoveryVectorizer Create(
            IDiscoveryVectorizationService service,
            IDiscoveryVectorMetric metric)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (metric == null) throw new ArgumentNullException(nameof(metric));
            return new DiscoveryVectorizer(service, metric);
        }
    }
}
