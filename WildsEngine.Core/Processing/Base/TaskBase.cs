using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Processing
{
    public abstract class TaskBase :
        BaseObject,
        ITask
    {
        private readonly IComponent _component;

        protected TaskBase(
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
    }
}