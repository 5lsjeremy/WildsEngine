using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Tests.Entities.Fixtures
{
    public sealed class TestNeighbor :
        BaseObject
    {
        private readonly IComponent _component;

        public TestNeighbor(
            string layer,
            IComponent component)
            : base(
                layer)
        {
            _component =
                component ??
                throw new ArgumentNullException(
                    nameof(component));
        }

        public override IComponent Component =>
            _component;
    }
}
