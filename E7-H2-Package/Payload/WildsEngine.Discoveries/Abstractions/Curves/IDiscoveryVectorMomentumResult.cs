using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Abstractions.Curves
{
    public interface IDiscoveryVectorMomentumResult
    {
        Guid MotionServiceExecutionId { get; }
        IDiscoveryVectorEvaluation Evaluation { get; }
        IReadOnlyList<IDiscoveryVectorMotion> RawMotion { get; }
        IReadOnlyList<IDiscoveryVectorMotion> AdjustedMotion { get; }
        string CurveSlug { get; }
    }
}
