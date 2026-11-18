using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Curves;
using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Curves
{
    public sealed class DiscoveryVectorMomentumResult : IDiscoveryVectorMomentumResult
    {
        public DiscoveryVectorMomentumResult(
            IDiscoveryVectorMotionResult motion,
            IReadOnlyList<IDiscoveryVectorMotion> adjusted,
            string curveSlug)
        {
            if (motion == null) throw new ArgumentNullException(nameof(motion));
            if (adjusted == null) throw new ArgumentNullException(nameof(adjusted));
            if (string.IsNullOrWhiteSpace(curveSlug)) throw new ArgumentException("Curve slug required.", nameof(curveSlug));
            var copy = new IDiscoveryVectorMotion[adjusted.Count];
            for (var i = 0; i < adjusted.Count; i++) copy[i] = adjusted[i];
            MotionServiceExecutionId = motion.ServiceExecutionId; Evaluation = motion.Evaluation;
            RawMotion = motion.Motion; AdjustedMotion = new ReadOnlyCollection<IDiscoveryVectorMotion>(copy); CurveSlug = curveSlug;
        }
        public Guid MotionServiceExecutionId { get; }
        public IDiscoveryVectorEvaluation Evaluation { get; }
        public IReadOnlyList<IDiscoveryVectorMotion> RawMotion { get; }
        public IReadOnlyList<IDiscoveryVectorMotion> AdjustedMotion { get; }
        public string CurveSlug { get; }
    }
}
