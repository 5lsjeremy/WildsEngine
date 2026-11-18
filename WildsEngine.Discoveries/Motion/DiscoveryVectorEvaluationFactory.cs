using System;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Abstractions.Vectorization;

namespace WildsEngine.Discoveries.Motion
{
    public sealed class DiscoveryVectorEvaluationFactory : IDiscoveryVectorEvaluationFactory
    {
        public IDiscoveryVectorEvaluation Create(
            IDiscoveryVectorizationResult result,
            DateTimeOffset evaluatedAt)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return new DiscoveryVectorEvaluation(result.Vector, result.ServiceExecutionId, evaluatedAt);
        }
    }
}
