using System;

namespace WildsEngine.Discoveries.Abstractions.Vectorization
{
    public interface IDiscoveryVectorizationResult
    {
        Guid ServiceExecutionId { get; }
        IDiscoveryVector Vector { get; }
    }
}
