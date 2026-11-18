using System;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Motion
{
    public sealed class DiscoveryVectorEvaluation : IDiscoveryVectorEvaluation
    {
        public DiscoveryVectorEvaluation(
            IDiscoveryVector vector,
            Guid vectorizationExecutionId,
            DateTimeOffset evaluatedAt)
        {
            Vector = vector ?? throw new ArgumentNullException(nameof(vector));
            if (vectorizationExecutionId == Guid.Empty)
                throw new ArgumentException("Evaluation requires a vectorization execution ID.", nameof(vectorizationExecutionId));
            VectorizationExecutionId = vectorizationExecutionId;
            EvaluatedAt = evaluatedAt;
        }
        public IDiscoveryVector Vector { get; }
        public Guid VectorizationExecutionId { get; }
        public DateTimeOffset EvaluatedAt { get; }
    }
}
