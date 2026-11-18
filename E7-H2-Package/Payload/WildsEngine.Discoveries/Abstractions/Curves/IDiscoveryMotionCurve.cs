namespace WildsEngine.Discoveries.Abstractions.Curves
{
    public interface IDiscoveryMotionCurve
    {
        string Slug { get; }
        double TransformVelocity(string dimension, double velocity);
        double TransformAcceleration(string dimension, double acceleration);
    }
}
