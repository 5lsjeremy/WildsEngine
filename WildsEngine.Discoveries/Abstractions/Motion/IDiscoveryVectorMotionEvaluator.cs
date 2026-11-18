using System;
using System.Threading;
using System.Threading.Tasks;

namespace WildsEngine.Discoveries.Abstractions.Motion
{
    public interface IDiscoveryVectorMotionEvaluator
    {
        Task<IDiscoveryVectorMotionResult> EvaluateAsync(
            IDiscoveryVectorEvaluationSeries series,
            object? provenance = null,
            Guid? parentExecutionId = null,
            Guid? pulseId = null,
            CancellationToken cancellationToken = default);
    }
}
