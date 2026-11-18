using System;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Vectorization;

namespace WildsEngine.Discoveries.Vectorization
{
    public sealed class DiscoveryVectorizationResult : IDiscoveryVectorizationResult
    {
        public DiscoveryVectorizationResult(Guid serviceExecutionId, IDiscoveryVector vector)
        {
            if (serviceExecutionId == Guid.Empty) throw new ArgumentException(
                "Vectorization result requires a service execution ID.", nameof(serviceExecutionId));
            ServiceExecutionId = serviceExecutionId;
            Vector = vector ?? throw new ArgumentNullException(nameof(vector));
        }
        public Guid ServiceExecutionId { get; }
        public IDiscoveryVector Vector { get; }
    }
}
