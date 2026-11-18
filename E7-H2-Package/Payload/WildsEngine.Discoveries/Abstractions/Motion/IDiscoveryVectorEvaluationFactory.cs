using System;
using WildsEngine.Discoveries.Abstractions.Vectorization;

namespace WildsEngine.Discoveries.Abstractions.Motion
{
    public interface IDiscoveryVectorEvaluationFactory
    {
        IDiscoveryVectorEvaluation Create(
            IDiscoveryVectorizationResult result,
            DateTimeOffset evaluatedAt);
    }
}
