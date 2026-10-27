using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.States
{
    public abstract class WildsState :
        BaseObject,
        IState
    {
        private readonly IComponent _component;

        protected WildsState(
            IComponent component,
            IContext context,
            string layer)
            : base(layer)
        {
            _component = component
                ?? throw new ArgumentNullException(
                    nameof(component));

            Context = context
                ?? throw new ArgumentNullException(
                    nameof(context));
        }

        protected WildsState(
            Guid id,
            IComponent component,
            IContext context,
            string layer)
            : base(id, layer)
        {
            _component = component
                ?? throw new ArgumentNullException(
                    nameof(component));

            Context = context
                ?? throw new ArgumentNullException(
                    nameof(context));
        }

        public sealed override IComponent Component =>
            _component;

        public IContext Context { get; }

        public abstract bool Process(
            IStateInstruction instruction);
    }
}
