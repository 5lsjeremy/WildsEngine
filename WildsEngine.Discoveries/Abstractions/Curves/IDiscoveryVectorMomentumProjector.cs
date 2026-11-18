using WildsEngine.Discoveries.Abstractions.Motion;

namespace WildsEngine.Discoveries.Abstractions.Curves
{
    public interface IDiscoveryVectorMomentumProjector
    {
        IDiscoveryVectorMomentumResult Project(
            IDiscoveryVectorMotionResult motion,
            IDiscoveryMotionCurve curve);
    }
}
