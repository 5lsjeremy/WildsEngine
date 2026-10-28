using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Processing
{
    public abstract class TaskBusBase :
        BaseObject,
        ITaskBus
    {
        private readonly IComponent _component;

        protected TaskBusBase(
            IComponent component,
            string layer)
            : base(layer)
        {
            _component = component
                         ?? throw new ArgumentNullException(
                             nameof(component));
        }

        protected TaskBusBase(
            Guid id,
            IComponent component,
            string layer)
            : base(
                id,
                layer)
        {
            _component = component
                         ?? throw new ArgumentNullException(
                             nameof(component));
        }

        public sealed override IComponent Component =>
            _component;

        public bool Dispatch(ITask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(
                    nameof(task));
            }

            return DispatchTaskCore(task);
        }

        public bool Dispatch(ISignal signal)
        {
            if (signal == null)
            {
                throw new ArgumentNullException(
                    nameof(signal));
            }

            return DispatchSignalCore(signal);
        }

        protected abstract bool DispatchTaskCore(
            ITask task);

        protected abstract bool DispatchSignalCore(
            ISignal signal);
    }
}