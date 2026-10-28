using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Pooling;
using WildsEngine.Core.Processing.Processor;
using WildsEngine.Core.Processing.Registry;

namespace WildsEngine.Core
{
    /// <summary>
    /// Composition root for one isolated Wilds runtime instance.
    /// </summary>
    public sealed class WildsEngine :
        IDisposable
    {
        private readonly IComponent _component;

        private readonly DefaultInstructionsRegistry
            _instructionRegistry;

        private readonly DefaultTasksRegistry
            _tasksRegistry;

        private readonly ObjectPool<ITaskProcessor>
            _taskProcessors;

        private readonly ObjectPool<IInstructionsProcessor>
            _instructionsProcessors;

        private int _disposed;

        public WildsEngine(
            IComponent component,
            WildsEngineOptions options)
        {
            _component = component ??
                throw new ArgumentNullException(
                    nameof(component));

            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.Layer))
            {
                throw new ArgumentException(
                    "The engine requires a layer.",
                    nameof(options));
            }

            Layer =
                options.Layer.Trim();

            _instructionRegistry =
                new DefaultInstructionsRegistry(
                    _component,
                    $"{Layer}.instruction-registry");

            _tasksRegistry =
                new DefaultTasksRegistry(
                    _component,
                    $"{Layer}.task-registry");

            _taskProcessors =
                new ObjectPool<ITaskProcessor>(
                    factory: () =>
                        new TaskProcessor(
                            _component,
                            $"{Layer}.task-processor",
                            _tasksRegistry),
                    maximumRetained:
                        options.RetainedTaskProcessors,
                    maximumConcurrent:
                        options.ConcurrentTaskProcessors);

            _instructionsProcessors =
                new ObjectPool<IInstructionsProcessor>(
                    factory: () =>
                        new InstructionsProcessor(
                            _component,
                            $"{Layer}.instructions-processor",
                            _instructionRegistry,
                            _taskProcessors),
                    maximumRetained:
                        options.RetainedInstructionsProcessors,
                    maximumConcurrent:
                        options.ConcurrentInstructionsProcessors);
        }

        public string Layer { get; }

        public IComponent Component =>
            _component;

        public bool IsInitialized { get; private set; }

        public IObjectPool<IInstructionsProcessor> Instructions =>
            _instructionsProcessors;

        public bool Initialize()
        {
            ThrowIfDisposed();

            if (IsInitialized)
                return false;

            _instructionRegistry.Initialize();
            _tasksRegistry.Initialize();

            IsInitialized = true;

            return true;
        }

        public async ValueTask<TResult>
            ExecuteInstructionAsync<TResult>(
                Func<
                    IInstructionsProcessor,
                    CancellationToken,
                    ValueTask<TResult>> operation,
                CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "The WildsEngine must be initialized " +
                    "before processing instructions.");
            }

            if (operation == null)
            {
                throw new ArgumentNullException(
                    nameof(operation));
            }

            using (var lease =
                await _instructionsProcessors
                    .RentAsync(cancellationToken)
                    .ConfigureAwait(false))
            {
                return await operation(
                        lease.Instance,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public bool Shutdown()
        {
            ThrowIfDisposed();

            if (!IsInitialized)
                return false;

            _tasksRegistry.Shutdown();
            _instructionRegistry.Shutdown();

            IsInitialized = false;

            return true;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(
                    ref _disposed,
                    1) != 0)
            {
                return;
            }

            if (IsInitialized)
            {
                _tasksRegistry.Shutdown();
                _instructionRegistry.Shutdown();

                IsInitialized = false;
            }

            _instructionsProcessors.Dispose();
            _taskProcessors.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                throw new ObjectDisposedException(
                    nameof(WildsEngine));
            }
        }
    }
}