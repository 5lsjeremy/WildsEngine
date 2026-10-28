using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Processing.Processor
{
    /// <summary>
    /// Processes task work using a task registry.
    /// Provides the task lifecycle boundary used by
    /// pooled task processors.
    /// </summary>
    public class TaskProcessor :
        ProcessorBase,
        ITaskProcessor
    {
        public TaskProcessor(
            IComponent component,
            string layer,
            ITaskRegistry registry)
            : base(
                component,
                layer)
        {
            Registry = registry ??
                throw new ArgumentNullException(
                    nameof(registry));
        }

        public ITaskRegistry Registry { get; }

        /// <summary>
        /// Parks a task for processing.
        /// </summary>
        public bool RegisterTask(
            ITask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(
                    nameof(task));
            }

            return Registry.Register(task);
        }

        /// <summary>
        /// Determines whether a task is currently parked.
        /// </summary>
        public bool ContainsTask(
            Guid id)
        {
            return Registry.Contains(id);
        }

        /// <summary>
        /// Retrieves a parked task by identity.
        /// </summary>
        public ITask? GetTask(
            Guid id)
        {
            return Registry.Get(id);
        }

        /// <summary>
        /// Attempts to retrieve a parked task by identity.
        /// </summary>
        public bool TryGetTask(
            Guid id,
            out ITask? task)
        {
            return Registry.TryGet(
                id,
                out task);
        }

        /// <summary>
        /// Removes a parked task after processing completes.
        /// </summary>
        public bool RemoveTask(
            Guid id)
        {
            return Registry.Remove(id);
        }

        /// <summary>
        /// Clears transient processor state before this
        /// processor is returned to its pool.
        ///
        /// Registry contents intentionally survive processor
        /// reuse because task lifetime is independent from
        /// processor lifetime.
        /// </summary>
        protected override bool OnReset()
        {
            return true;
        }
    }
}