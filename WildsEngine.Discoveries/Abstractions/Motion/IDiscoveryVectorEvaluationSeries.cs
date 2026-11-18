using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions.Motion
{
    public interface IDiscoveryVectorEvaluationSeries
    {
        string Scope { get; }
        IReadOnlyList<IDiscoveryVectorInput> Inputs { get; }
        IReadOnlyList<IDiscoveryVectorEvaluation> Evaluations { get; }
        IDiscoveryVectorEvaluation? Current { get; }
        IDiscoveryVectorEvaluation? Previous { get; }
    }
}
