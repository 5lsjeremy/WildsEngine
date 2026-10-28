using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing.Processor
{
    public class InstructionsProcessor :
        ProcessorBase,
        IInstructionsProcessor
    {
        private readonly IObjectPool<ITaskProcessor>
            _taskProcessors;

        public InstructionsProcessor(
            string layer,
            Abstractions.IInstructionsRegistry registry,
            IObjectPool<ITaskProcessor> taskProcessors)
            : base(layer)
        {
            Registry = registry ??
                       throw new ArgumentNullException(nameof(registry));

            _taskProcessors = taskProcessors ??
                              throw new ArgumentNullException(
                                  nameof(taskProcessors));
        }

        public Abstractions.IInstructionsRegistry Registry { get; }

        public override IComponent Component { get; }

        /// <summary>
        /// Rents a task processor, performs delegated work,
        /// and guarantees that the processor is returned.
        /// </summary>
        protected async ValueTask<TResult> ExecuteTaskAsync<TResult>(
            Func<
                ITaskProcessor,
                CancellationToken,
                ValueTask<TResult>> operation,
            CancellationToken cancellationToken = default)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            using (var lease =
                   await _taskProcessors
                       .RentAsync(cancellationToken)
                       .ConfigureAwait(false))
            {
                return await operation(
                        lease.Instance,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        protected override bool OnReset()
        {
            // Clear future per-instruction state here.
            return true;
        }
    }
}