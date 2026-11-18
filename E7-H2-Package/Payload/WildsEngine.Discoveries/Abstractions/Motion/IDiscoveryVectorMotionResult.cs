using System;
using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions.Motion
{
    public interface IDiscoveryVectorMotionResult
    {
        Guid ServiceExecutionId { get; }
        IDiscoveryVectorEvaluation Evaluation { get; }
        IReadOnlyList<IDiscoveryVectorMotion> Motion { get; }
    }
}
