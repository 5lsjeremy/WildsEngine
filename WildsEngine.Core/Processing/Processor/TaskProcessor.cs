using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing.Processor
{
    public class TaskProcessor :
        ProcessorBase,
        ITaskProcessor
    {
        public TaskProcessor(
            string layer,
            ITaskRegistry registry)
            : base(layer)
        {
            Registry = registry ??
                       throw new ArgumentNullException(nameof(registry));
        }

        public ITaskRegistry Registry { get; }

        public override IComponent Component { get; }

        protected override bool OnReset()
        {
            // Clear future per-task state here.
            return true;
        }
    }
}