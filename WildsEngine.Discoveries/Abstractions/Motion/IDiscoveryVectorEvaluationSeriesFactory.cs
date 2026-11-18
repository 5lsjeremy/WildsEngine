using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions.Motion
{
    public interface IDiscoveryVectorEvaluationSeriesFactory
    {
        IDiscoveryVectorEvaluationSeries Create(
            string scope,
            IReadOnlyList<IDiscoveryVectorInput> inputs,
            IReadOnlyList<IDiscoveryVectorEvaluation> evaluations);
    }
}
