using System;

namespace WildsEngine.Discoveries.Abstractions.Motion
{
    public sealed class DiscoveryVectorMotionRequest
    {
        public DiscoveryVectorMotionRequest(IDiscoveryVectorEvaluationSeries series)
        {
            Series = series ?? throw new ArgumentNullException(nameof(series));
        }
        public IDiscoveryVectorEvaluationSeries Series { get; }
    }
}
