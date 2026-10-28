using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing.Processor
{
    /// <summary>
    /// Processes instructions using the instruction registry
    /// and delegates task work through the task processor pool.
    /// </summary>
    public class InstructionsProcessor :
        ProcessorBase,
        IInstructionsProcessor
    {
        private readonly IObjectPool<ITaskProcessor> _taskProcessors;

        public InstructionsProcessor(
            IComponent component,
            string layer,
            IInstructionsRegistry registry,
            IObjectPool<ITaskProcessor> taskProcessors)
            : base(
                component,
                layer)
        {
            Registry = registry ??
                       throw new ArgumentNullException(
                           nameof(registry));

            _taskProcessors = taskProcessors ??
                              throw new ArgumentNullException(
                                  nameof(taskProcessors));
        }

        public IInstructionsRegistry Registry { get; }

        /// <summary>
        /// Rents a task processor for delegated task processing.
        /// The caller owns the returned lease and must dispose it.
        /// </summary>
        public ValueTask<ObjectLease<ITaskProcessor>>
            RentTaskProcessorAsync(
                CancellationToken cancellationToken = default)
        {
            return _taskProcessors.RentAsync(
                cancellationToken);
        }

        /// <summary>
        /// Clears transient processor state before this processor
        /// is returned to its pool.
        /// </summary>
        protected override bool OnReset()
        {
            return true;
        }
    }
}