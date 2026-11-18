using WildsEngine.Discoveries.Abstractions.Curves;

namespace WildsEngine.Discoveries.Curves
{
    public sealed class IdentityDiscoveryMotionCurve : IDiscoveryMotionCurve
    {
        public string Slug => "identity";
        public double TransformVelocity(string dimension, double velocity) => velocity;
        public double TransformAcceleration(string dimension, double acceleration) => acceleration;
    }
}
