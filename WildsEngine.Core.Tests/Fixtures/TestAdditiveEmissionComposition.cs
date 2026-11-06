using WildsEngine.Core.Emission;

namespace WildsEngine.Core.Tests.Fixtures
{
    internal sealed class TestAdditiveEmissionComposition :
        EmissionCompositionBase
    {
        protected override double ComposeCore(
            double emission,
            double topology)
        {
            return emission +
                   topology;
        }
    }
}