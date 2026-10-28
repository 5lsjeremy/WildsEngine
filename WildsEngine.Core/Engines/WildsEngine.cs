using System;
using System.Threading;
using System.Threading.Tasks;
using global::WildsEngine.Core.Abstractions;
using WildsEngine.Core.Pooling;
using WildsEngine.Core.Processing.Processor;

namespace WildsEngine.Core
{
    /// <summary>
    /// Composition root for one isolated Wilds runtime instance.
    /// Owns registries and processor pools and coordinates
    /// their runtime lifecycle.
    /// </summary>
    public sealed class WildsEngine : IDisposable
    {
        private readonly DefaultInstructionsRegistry
            _instructionsRegistry;

        private readonly DefaultTaskRegistry
            _taskRegistry;

        private readonly ObjectPool<ITaskProcessor>
            _taskProcessors;

        private readonly ObjectPool<IInstructionsProcessor>
            _instructionsProcessors;

        private int _disposed;

        public WildsEngine(
            WildsEngineOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(options.Layer))
            {
                throw new ArgumentException(
                    "The engine requires a layer.",
                    nameof(options));
            }

            Layer = options.Layer.Trim();

            _instructionsRegistry =
                new DefaultInstructionsRegistry(
                    $"{Layer}.instructions-registry");

            _taskRegistry =
                new DefaultTaskRegistry(
                    $"{Layer}.task-registry");

            _taskProcessors =
                new ObjectPool<ITaskProcessor>(
                    factory: () =>
                        new TaskProcessor(
                            $"{Layer}.task-processor",
                            _taskRegistry),
                    maximumRetained:
                        options.RetainedTaskProcessors,
                    maximumConcurrent:
                        options.ConcurrentTaskProcessors);

            _instructionsProcessors =
                new ObjectPool<IInstructionsProcessor>(
                    factory: () =>
                        new InstructionsProcessor(
                            $"{Layer}.instructions-processor",
                            _instructionsRegistry,
                            _taskProcessors),
                    maximumRetained:
                        options.RetainedInstructionsProcessors,
                    maximumConcurrent:
                        options.ConcurrentInstructionsProcessors);
        }

        public string Layer { get; }

        public bool IsInitialized { get; private set; }

        public IObjectPool<IInstructionsProcessor> Instructions =>
            _instructionsProcessors;

        public bool Initialize()
        {
            ThrowIfDisposed();

            if (IsInitialized)
                return false;

            _instructionsRegistry.Initialize();
            _taskRegistry.Initialize();

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
                    "The Wilds engine must be initialized before processing.");
            }

            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

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

            _taskRegistry.Shutdown();
            _instructionsRegistry.Shutdown();

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
                _taskRegistry.Shutdown();
                _instructionsRegistry.Shutdown();

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