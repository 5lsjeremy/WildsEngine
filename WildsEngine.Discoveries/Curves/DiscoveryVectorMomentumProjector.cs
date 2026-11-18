using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Curves;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Vectors;

namespace WildsEngine.Discoveries.Curves
{
    public sealed class DiscoveryVectorMomentumProjector : IDiscoveryVectorMomentumProjector
    {
        public IDiscoveryVectorMomentumResult Project(
            IDiscoveryVectorMotionResult motion, IDiscoveryMotionCurve curve)
        {
            if (motion == null) throw new ArgumentNullException(nameof(motion));
            if (curve == null) throw new ArgumentNullException(nameof(curve));
            var adjusted = new List<IDiscoveryVectorMotion>(motion.Motion.Count);
            for (var i = 0; i < motion.Motion.Count; i++)
            {
                var source = motion.Motion[i];
                var velocity = curve.TransformVelocity(source.Dimension, source.Velocity);
                var acceleration = curve.TransformAcceleration(source.Dimension, source.Acceleration);
                if (double.IsNaN(velocity) || double.IsInfinity(velocity) || double.IsNaN(acceleration) || double.IsInfinity(acceleration))
                    throw new InvalidOperationException("Curve output must be finite.");
                adjusted.Add(new DiscoveryVectorMotion(source.Dimension, velocity, acceleration));
            }
            return new DiscoveryVectorMomentumResult(motion, adjusted, curve.Slug);
        }
    }
}
