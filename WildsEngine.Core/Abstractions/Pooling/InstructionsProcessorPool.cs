using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Pooling
{
    /// <summary>
    /// Provides bounded leasing and reuse of instruction processors.
    /// </summary>
    public sealed class InstructionsProcessorPool :
        ObjectPool<IInstructionsProcessor>,
        IInstructionsProcessorPool
    {
        public InstructionsProcessorPool(
            Func<IInstructionsProcessor> factory,
            int maximumRetained,
            int maximumConcurrent)
            : base(
                factory,
                maximumRetained,
                maximumConcurrent)
        {
        }
    }
}


namespace WildsEngine.Core.Pooling
{
    /// <summary>
    /// Provides bounded leasing and reuse of task processors.
    /// </summary>
    public sealed class TaskProcessorPool :
        ObjectPool<ITaskProcessor>,
        ITaskProcessorPool
    {
        public TaskProcessorPool(
            Func<ITaskProcessor> factory,
            int maximumRetained,
            int maximumConcurrent)
            : base(
                factory,
                maximumRetained,
                maximumConcurrent)
        {
        }
    }
}