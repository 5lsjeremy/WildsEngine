using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Motion
{
    public sealed class DiscoveryVectorEvaluationSeriesFactory : IDiscoveryVectorEvaluationSeriesFactory
    {
        public IDiscoveryVectorEvaluationSeries Create(
            string scope,
            IReadOnlyList<IDiscoveryVectorInput> inputs,
            IReadOnlyList<IDiscoveryVectorEvaluation> evaluations)
        {
            return new DiscoveryVectorEvaluationSeries(scope, inputs, evaluations);
        }
    }
}
