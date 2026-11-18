using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Motion
{
    internal sealed class DiscoveryVectorMotionEvaluator : IDiscoveryVectorMotionEvaluator
    {
        private readonly IDiscoveryVectorMotionService _service;
        public DiscoveryVectorMotionEvaluator(IDiscoveryVectorMotionService service)
        { _service = service ?? throw new ArgumentNullException(nameof(service)); }
        public async Task<IDiscoveryVectorMotionResult> EvaluateAsync(
            IDiscoveryVectorEvaluationSeries series, object? provenance = null,
            Guid? parentExecutionId = null, Guid? pulseId = null,
            CancellationToken cancellationToken = default)
        {
            if (series == null) throw new ArgumentNullException(nameof(series));
            var result = await _service.ExecuteAsync(
                new DiscoveryVectorMotionRequest(series), provenance,
                parentExecutionId, pulseId, cancellationToken);
            return new DiscoveryVectorMotionResult(result.ExecutionId, result.Value);
        }
    }
}
