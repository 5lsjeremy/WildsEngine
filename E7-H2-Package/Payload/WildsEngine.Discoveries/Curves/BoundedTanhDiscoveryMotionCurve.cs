using System;
using WildsEngine.Discoveries.Abstractions.Curves;

namespace WildsEngine.Discoveries.Curves
{
    public sealed class BoundedTanhDiscoveryMotionCurve : IDiscoveryMotionCurve
    {
        private readonly double _gain;
        private readonly double _limit;
        public BoundedTanhDiscoveryMotionCurve(double gain, double limit)
        {
            if (double.IsNaN(gain) || double.IsInfinity(gain) || gain <= 0.0) throw new ArgumentOutOfRangeException(nameof(gain));
            if (double.IsNaN(limit) || double.IsInfinity(limit) || limit <= 0.0) throw new ArgumentOutOfRangeException(nameof(limit));
            _gain = gain; _limit = limit;
        }
        public string Slug => "bounded-tanh";
        public double TransformVelocity(string dimension, double velocity) => Transform(velocity);
        public double TransformAcceleration(string dimension, double acceleration) => Transform(acceleration);
        private double Transform(double value)
        {
            var result = _limit * Math.Tanh(_gain * value);
            if (double.IsNaN(result) || double.IsInfinity(result)) throw new InvalidOperationException("Curve output must be finite.");
            return result;
        }
    }
}
