using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Motion
{
    public sealed class DiscoveryVectorMotionResult : IDiscoveryVectorMotionResult
    {
        public DiscoveryVectorMotionResult(
            Guid serviceExecutionId,
            DiscoveryVectorMotionServiceResult result)
        {
            if (serviceExecutionId == Guid.Empty) throw new ArgumentException("Motion result requires a service execution ID.", nameof(serviceExecutionId));
            if (result == null) throw new ArgumentNullException(nameof(result));
            ServiceExecutionId = serviceExecutionId; Evaluation = result.Evaluation; Motion = result.Motion;
        }
        public Guid ServiceExecutionId { get; }
        public IDiscoveryVectorEvaluation Evaluation { get; }
        public IReadOnlyList<IDiscoveryVectorMotion> Motion { get; }
    }
}
