using System;

namespace WildsEngine.Discoveries.Abstractions.Motion
{
    public interface IDiscoveryVectorEvaluation
    {
        IDiscoveryVector Vector { get; }
        Guid VectorizationExecutionId { get; }
        DateTimeOffset EvaluatedAt { get; }
    }
}
