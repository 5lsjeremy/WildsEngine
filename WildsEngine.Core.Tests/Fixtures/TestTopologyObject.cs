using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Tests.Fixtures
{
    internal sealed class TestTopologyObject :
        BaseObject
    {
        private readonly IComponent _component;

        public TestTopologyObject(
            IComponent component,
            string layer)
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