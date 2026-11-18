using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WildsEngine.Discoveries.Abstractions.Motion
{
    public sealed class DiscoveryVectorMotionServiceResult
    {
        public DiscoveryVectorMotionServiceResult(
            IDiscoveryVectorEvaluation evaluation,
            IReadOnlyList<IDiscoveryVectorMotion> motion)
        {
            Evaluation = evaluation ?? throw new ArgumentNullException(nameof(evaluation));
            if (motion == null) throw new ArgumentNullException(nameof(motion));
            var copy = new IDiscoveryVectorMotion[motion.Count];
            for (var i = 0; i < motion.Count; i++)
                copy[i] = motion[i] ?? throw new ArgumentException(
                    "Motion cannot contain null entries.", nameof(motion));
            Motion = new ReadOnlyCollection<IDiscoveryVectorMotion>(copy);
        }
        public IDiscoveryVectorEvaluation Evaluation { get; }
        public IReadOnlyList<IDiscoveryVectorMotion> Motion { get; }
    }
}
