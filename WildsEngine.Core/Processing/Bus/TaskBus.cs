using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing.Bus
{
    public sealed class TaskBus :
        TaskBusBase
    {
        public TaskBus(
            IComponent component,
            string layer)
            : base(
                component,
                layer)
        {
        }

        protected override bool DispatchTaskCore(
            ITask task)
        {
            return false;
        }

        protected override bool DispatchSignalCore(
            ISignal signal)
        {
            return false;
        }
    }
}