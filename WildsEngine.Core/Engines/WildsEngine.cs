using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Pooling;
using WildsEngine.Core.Processing.Coordinator;
using WildsEngine.Core.Processing.Processor;
using WildsEngine.Core.Processing.Registry;
using WildsEngine.Core.Units;

namespace WildsEngine.Core
{
    /// <summary>
    /// Composition root for one isolated Wilds runtime instance.
    ///
    /// Wilds accepts complete requests as Pulses, owns their
    /// internal WorkUnits, resolves them into Moments, and
    /// records those Moments in its Timeline and Epoch.
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

        private readonly Timeline _timeline;
        private readonly Epoch _epoch;

        private int _disposed;

        public WildsEngine(
            IComponent component,
            WildsEngineOptions options)
        {
            _component =
                component ??
                throw new ArgumentNullException(
                    nameof(component));

            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options));
            }

            if (string.IsNullOrWhiteSpace(
                    options.Layer))
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

            _timeline =
                new Timeline();

            _epoch =
                new Epoch();

            if (!_epoch.AddTimeline(
                    _timeline))
            {
                throw new InvalidOperationException(
                    "The engine could not initialize " +
                    "its primary Timeline.");
            }
        }

        public string Layer { get; }

        public IComponent Component =>
            _component;

        public bool IsInitialized { get; private set; }

        public IObjectPool<IInstructionsProcessor>
            Instructions =>
                _instructionsProcessors;

        /// <summary>
        /// Contains the ordered Moment history produced by
        /// this engine instance.
        /// </summary>
        public ITimeline Timeline =>
            _timeline;

        /// <summary>
        /// Contains the Timeline history owned by this
        /// engine instance.
        /// </summary>
        public IEpoch Epoch =>
            _epoch;

        public bool Initialize()
        {
            ThrowIfDisposed();

            if (IsInitialized)
                return false;

            _instructionRegistry.Initialize();
            _tasksRegistry.Initialize();

            IsInitialized =
                true;

            return true;
        }

        /// <summary>
        /// Executes one complete consumer request through the
        /// certified single-WorkUnit Pulse lifecycle.
        ///
        /// Wilds creates and correlates the Pulse and WorkUnit,
        /// owns their lifecycle transitions, finalizes the
        /// terminal Moment, and records that Moment in the
        /// engine Timeline.
        ///
        /// The supplied operation represents internal work
        /// performed by Wilds or an integration adapter.
        /// </summary>
        public async ValueTask<IMoment> ExecuteAsync(
            object? request,
            Func<
                object?,
                CancellationToken,
                ValueTask<object?>> operation,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfNotInitialized();

            if (operation == null)
            {
                throw new ArgumentNullException(
                    nameof(operation));
            }

            var pulse =
                new Pulse(
                    request);

            var work =
                new WorkUnit(
                    pulse.Id,
                    request);

            var coordinator =
                new PulseCoordinator(
                    pulse);

            BeginPulse(
                pulse,
                work,
                coordinator);

            object? result;

            try
            {
                cancellationToken
                    .ThrowIfCancellationRequested();

                result =
                    await operation(
                            request,
                            cancellationToken)
                        .ConfigureAwait(false);

                cancellationToken
                    .ThrowIfCancellationRequested();

                if (!work.TryComplete(
                        result))
                {
                    throw new InvalidOperationException(
                        "The WorkUnit could not complete.");
                }
            }
            catch (OperationCanceledException)
            {
                if (!work.IsTerminal &&
                    !work.TryCancel())
                {
                    throw new InvalidOperationException(
                        "The WorkUnit could not be cancelled.");
                }

                var cancelledMoment =
                    FinalizePulse(
                        coordinator,
                        null);

                RecordMoment(
                    cancelledMoment);

                return cancelledMoment;
            }
            catch (Exception exception)
            {
                if (!work.IsTerminal &&
                    !work.TryFail(
                        exception))
                {
                    throw new InvalidOperationException(
                        "The WorkUnit could not record failure.",
                        exception);
                }

                var failedMoment =
                    FinalizePulse(
                        coordinator,
                        null);

                RecordMoment(
                    failedMoment);

                return failedMoment;
            }

            var completedMoment =
                FinalizePulse(
                    coordinator,
                    result);

            RecordMoment(
                completedMoment);

            return completedMoment;
        }

        /// <summary>
        /// Executes one complete consumer request as multiple
        /// required internal pieces of work.
        ///
        /// Wilds creates and owns the Pulse, every required
        /// WorkUnit, coordination, terminal outcome, Moment,
        /// and temporal history.
        ///
        /// The result selector explicitly chooses the one
        /// authoritative Pulse result after every required
        /// operation has produced its subordinate result.
        /// </summary>
        public async ValueTask<IMoment> ExecuteAsync(
            object? request,
            IReadOnlyList<
                Func<
                    object?,
                    CancellationToken,
                    ValueTask<object?>>> operations,
            Func<
                IReadOnlyList<object?>,
                object?> resultSelector,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfNotInitialized();

            if (operations == null)
            {
                throw new ArgumentNullException(
                    nameof(operations));
            }

            if (operations.Count == 0)
            {
                throw new ArgumentException(
                    "At least one operation is required.",
                    nameof(operations));
            }

            if (resultSelector == null)
            {
                throw new ArgumentNullException(
                    nameof(resultSelector));
            }

            for (var index = 0;
                 index < operations.Count;
                 index++)
            {
                if (operations[index] == null)
                {
                    throw new ArgumentException(
                        "Every operation must be non-null.",
                        nameof(operations));
                }
            }

            var pulse =
                new Pulse(
                    request);

            var coordinator =
                new PulseCoordinator(
                    pulse);

            var workUnits =
                new WorkUnit[operations.Count];

            var results =
                new object?[operations.Count];

            BeginDecomposedPulse(
                pulse,
                workUnits,
                coordinator,
                request);

            for (var index = 0;
                 index < operations.Count;
                 index++)
            {
                var work =
                    workUnits[index];

                try
                {
                    cancellationToken
                        .ThrowIfCancellationRequested();

                    if (!work.TryBeginProcessing())
                    {
                        throw new InvalidOperationException(
                            "The WorkUnit could not begin processing.");
                    }

                    var result =
                        await operations[index](
                                request,
                                cancellationToken)
                            .ConfigureAwait(false);

                    cancellationToken
                        .ThrowIfCancellationRequested();

                    results[index] =
                        result;

                    /*
                     * The final WorkUnit remains Processing
                     * until the authoritative Pulselected.
                     *
                     * Earlier WorkUnits may complete immediately
                     * because their results are subordinate.
                     */
                    if (index <
                        operations.Count - 1)
                    {
                        if (!work.TryComplete(
                                result))
                        {
                            throw new InvalidOperationException(
                                "The WorkUnit could not complete.");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    if (!work.IsTerminal &&
                        !work.TryCancel())
                    {
                        throw new InvalidOperationException(
                            "The active WorkUnit could not be cancelled.");
                    }

                    CancelRemainingWork(
                        workUnits,
                        index + 1);

                    var cancelledMoment =
                        FinalizePulse(
                            coordinator,
                            null);

                    RecordMoment(
                        cancelledMoment);

                    return cancelledMoment;
                }
                catch (Exception exception)
                {
                    if (!work.IsTerminal &&
                        !work.TryFail(
                            exception))
                    {
                        throw new InvalidOperationException(
                            "The active WorkUnit could not record failure.",
                            exception);
                    }

                    CancelRemainingWork(
                        workUnits,
                        index + 1);

                    var failedMoment =
                        FinalizePulse(
                            coordinator,
                            null);

                    RecordMoment(
                        failedMoment);

                    return failedMoment;
                }
            }

            var finalWork =
                workUnits[
                    workUnits.Length - 1];

            object? finalResult;

            try
            {
                cancellationToken
                    .ThrowIfCancellationRequested();

                finalResult =
                    resultSelector(
                        results);

                cancellationToken
                    .ThrowIfCancellationRequested();

                if (!finalWork.TryComplete(
                        results[
                            results.Length - 1]))
                {
                    throw new InvalidOperationException(
                        "The final WorkUnit could not complete.");
                }
            }
            catch (OperationCanceledException)
            {
                if (!finalWork.IsTerminal &&
                    !finalWork.TryCancel())
                {
                    throw new InvalidOperationException(
                        "The final WorkUnit could not be cancelled.");
                }

                var cancelledMoment =
                    FinalizePulse(
                        coordinator,
                        null);

                RecordMoment(
                    cancelledMoment);

                return cancelledMoment;
            }
            catch (Exception exception)
            {
                if (!finalWork.IsTerminal &&
                    !finalWork.TryFail(
                        exception))
                {
                    throw new InvalidOperationException(
                        "The final WorkUnit could not record failure.",
                        exception);
                }

                var failedMoment =
                    FinalizePulse(
                        coordinator,
                        null);

                RecordMoment(
                    failedMoment);

                return failedMoment;
            }

            var completedMoment =
                FinalizePulse(
                    coordinator,
                    finalResult);

            RecordMoment(
                completedMoment);

            return completedMoment;
        }

        /// <summary>
        /// Preserves the existing certified instruction
        /// processor lease boundary.
        /// </summary>
        public async ValueTask<TResult>
            ExecuteInstructionAsync<TResult>(
                Func<
                    IInstructionsProcessor,
                    CancellationToken,
                    ValueTask<TResult>> operation,
                CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfNotInitialized();

            if (operation == null)
            {
                throw new ArgumentNullException(
                    nameof(operation));
            }

            using (var lease =
                await _instructionsProcessors
                    .RentAsync(
                        cancellationToken)
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

            IsInitialized =
                false;

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

                IsInitialized =
                    false;
            }

            _instructionsProcessors.Dispose();
            _taskProcessors.Dispose();
        }

        private static void BeginPulse(
            Pulse pulse,
            WorkUnit work,
            IPulseCoordinator coordinator)
        {
            if (!pulse.TryBeginDecomposition())
            {
                throw new InvalidOperationException(
                    "The Pulse could not begin decomposition.");
            }

            if (!coordinator.AddWorkUnit(
                    work))
            {
                throw new InvalidOperationException(
                    "The WorkUnit could not be correlated " +
                    "with its Pulse.");
            }

            if (!work.TryMarkPending())
            {
                throw new InvalidOperationException(
                    "The WorkUnit could not become pending.");
            }

            if (!pulse.TryMarkPending())
            {
                throw new InvalidOperationException(
                    "The Pulse could not become pending.");
            }

            if (!pulse.TryMarkReady())
            {
                throw new InvalidOperationException(
                    "The Pulse could not become ready.");
            }

            if (!pulse.TryBeginProcessing())
            {
                throw new InvalidOperationException(
                    "The Pulse could not begin processing.");
            }

            if (!work.TryBeginProcessing())
            {
                throw new InvalidOperationException(
                    "The WorkUnit could not begin processing.");
            }
        }

        private static void BeginDecomposedPulse(
            Pulse pulse,
            WorkUnit[] workUnits,
            IPulseCoordinator coordinator,
            object? request)
        {
            if (!pulse.TryBeginDecomposition())
            {
                throw new InvalidOperationException(
                    "The Pulse could not begin decomposition.");
            }

            for (var index = 0;
                 index < workUnits.Length;
                 index++)
            {
                var work =
                    new WorkUnit(
                        pulse.Id,
                        request);

                workUnits[index] =
                    work;

                if (!coordinator.AddWorkUnit(
                        work))
                {
                    throw new InvalidOperationException(
                        "A WorkUnit could not be correlated " +
                        "with its Pulse.");
                }

                if (!work.TryMarkPending())
                {
                    throw new InvalidOperationException(
                        "A WorkUnit could not become pending.");
                }
            }

            if (!pulse.TryMarkPending())
            {
                throw new InvalidOperationException(
                    "The Pulse could not become pending.");
            }

            if (!pulse.TryMarkReady())
            {
                throw new InvalidOperationException(
                    "The Pulse could not become ready.");
            }

            if (!pulse.TryBeginProcessing())
            {
                throw new InvalidOperationException(
                    "The Pulse could not begin processing.");
            }
        }

        private static void CancelRemainingWork(
            WorkUnit[] workUnits,
            int startIndex)
        {
            for (var index = startIndex;
                 index < workUnits.Length;
                 index++)
            {
                var work =
                    workUnits[index];

                if (work.IsTerminal)
                {
                    continue;
                }

                if (!work.TryCancel())
                {
                    throw new InvalidOperationException(
                        "A remaining WorkUnit could not be cancelled.");
                }
            }
        }

        private static IMoment FinalizePulse(
            IPulseCoordinator coordinator,
            object? result)
        {
            if (!coordinator.CanFinalize)
            {
                throw new InvalidOperationException(
                    "The Pulse cannot finalize while required " +
                    "WorkUnits remain unresolved.");
            }

            if (!coordinator.TryFinalize(
                    result,
                    out var moment))
            {
                throw new InvalidOperationException(
                    "The Pulse coordinator could not create " +
                    "a terminal Moment.");
            }

            return moment ??
                throw new InvalidOperationException(
                    "Pulse finalization returned no Moment.");
        }

        private void RecordMoment(
            IMoment moment)
        {
            if (!_timeline.AddMoment(
                    moment))
            {
                throw new InvalidOperationException(
                    "The terminal Moment could not be " +
                    "recorded in the engine Timeline.");
            }
        }

        private void ThrowIfNotInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "The WildsEngine must be initialized " +
                    "before processing requests.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(
                    ref _disposed) != 0)
            {
                throw new ObjectDisposedException(
                    nameof(WildsEngine));
            }
        }
    }
}