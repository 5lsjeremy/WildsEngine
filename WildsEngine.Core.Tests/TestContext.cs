using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Contexts;

namespace WildsEngine.Tests.Registry
{
    internal sealed class TestContext :
        WildsContext
    {
        public TestContext(
            IComponent component,
            string layer)
            : base(
                component,
                layer)
        {
        }
    }
}