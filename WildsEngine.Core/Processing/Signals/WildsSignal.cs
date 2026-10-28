using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Signals
{
    public abstract class WildsSignal :
        BaseObject,
        ISignal
    {
        private readonly IComponent _component;

        protected WildsSignal(
            IComponent component,
            string layer)
            : base(layer)
        {
            _component = component
                         ?? throw new ArgumentNullException(
                             nameof(component));
        }

        protected WildsSignal(
            Guid id,
            IComponent component,
            string layer)
            : base(id, layer)
        {
            _component = component
                         ?? throw new ArgumentNullException(
                             nameof(component));
        }

        public sealed override IComponent Component =>
            _component;
    }
}