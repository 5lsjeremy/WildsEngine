using System;

namespace WildsEngine.Discoveries.Abstractions.Vectorization
{
    public sealed class DiscoveryVectorizationRequest
    {
        public DiscoveryVectorizationRequest(IDiscoveryVectorInput input)
        {
            Input = input ?? throw new ArgumentNullException(nameof(input));
        }
        public IDiscoveryVectorInput Input { get; }
    }
}
