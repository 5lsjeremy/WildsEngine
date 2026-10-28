using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Processing
{
    public abstract class ProcessorBase :
        BaseObject,
        IResettablePoolObject
    {
        private readonly IComponent _component;

        protected ProcessorBase(
            IComponent component,
            string layer)
            : base(layer)
        {
            _component = component ??
                         throw new ArgumentNullException(
                             nameof(component));
        }

        public sealed override IComponent Component =>
            _component;

        public bool TryReset()
        {
            return OnReset();
        }

        protected virtual bool OnReset()
        {
            return true;
        }
    }
}