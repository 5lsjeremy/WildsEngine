using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Signals;

namespace WildsEngine.Tests.Registry
{
    internal sealed class TestSignal :
        WildsSignal
    {
        public TestSignal(
            IComponent component,
            string layer)
            : base(
                component,
                layer)
        {
        }
    }
}