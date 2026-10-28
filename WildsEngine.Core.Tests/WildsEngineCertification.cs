using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Tests
{
    /// <summary>
    /// Dependency-free certification suite for the current WildsEngine composition root.
    /// Run with: await WildsEngineCertification.RunAllAsync();
    /// Throws immediately on failure and prints a compact PASS summary on success.
    /// </summary>
    public static class WildsEngineCertification
    {
        private static readonly List<string> Passed = new List<string>();

        public static async Task RunAllAsync()
        {
            Passed.Clear();

            Run("Constructs isolated engine", ConstructsIsolatedEngine);
            Run("Initialize is idempotent", InitializeIsIdempotent);
            Run("Shutdown is idempotent", ShutdownIsIdempotent);
            await RunAsync("Rejects processing before initialization", RejectsProcessingBeforeInitialization);
            await RunAsync("Executes instruction lease", ExecutesInstructionLease);
            await RunAsync("Reuses instruction processor", ReusesInstructionProcessor);
            await RunAsync("Returns processor after operation failure", ReturnsProcessorAfterOperationFailure);
            await RunAsync("Honors cancellation while waiting", HonorsCancellationWhileWaiting);
            Run("Rejects use after disposal", RejectsUseAfterDisposal);
            await RunAsync("Concurrent load respects limit", ConcurrentLoadRespectsLimit);

            Console.WriteLine();
            Console.WriteLine($"WildsEngine certification passed: {Passed.Count}/{Passed.Count}");
            foreach (var name in Passed)
                Console.WriteLine($"[PASS] {name}");
        }

        private static WildsEngineOptions Options(
            int retainedInstructions = 4,
            int concurrentInstructions = 4,
            int retainedTasks = 8,
            int concurrentTasks = 8)
        {
            return new WildsEngineOptions
            {
                Layer = "certification",
                RetainedInstructionsProcessors = retainedInstructions,
                ConcurrentInstructionsProcessors = concurrentInstructions,
                RetainedTaskProcessors = retainedTasks,
                ConcurrentTaskProcessors = concurrentTasks
            };
        }

        private static void ConstructsIsolatedEngine()
        {
            using var a = new Core.WildsEngine(Options());
            using var b = new Core.WildsEngine(Options());

            Assert(!ReferenceEquals(a, b), "Engine instances must be independent.");
            Assert(!ReferenceEquals(a.Instructions, b.Instructions), "Instruction pools must be instance-owned.");
        }

        private static void InitializeIsIdempotent()
        {
            using var engine = new Core.WildsEngine(Options());
            Assert(engine.Initialize(), "First Initialize() should succeed.");
            Assert(engine.IsInitialized, "Engine should report initialized.");
            Assert(!engine.Initialize(), "Second Initialize() should return false.");
        }

        private static void ShutdownIsIdempotent()
        {
            using var engine = new Core.WildsEngine(Options());
            Assert(engine.Initialize(), "Initialize() should succeed.");
            Assert(engine.Shutdown(), "First Shutdown() should succeed.");
            Assert(!engine.IsInitialized, "Engine should report stopped.");
            Assert(!engine.Shutdown(), "Second Shutdown() should return false.");
        }

        private static async Task RejectsProcessingBeforeInitialization()
        {
            using var engine = new Core.WildsEngine(Options());
            await AssertThrowsAsync<InvalidOperationException>(async () =>
            {
                await engine.ExecuteInstructionAsync<int>((_, __) => new ValueTask<int>(1));
            });
        }

        private static async Task ExecutesInstructionLease()
        {
            using var engine = new Core.WildsEngine(Options());
            engine.Initialize();

            var result = await engine.ExecuteInstructionAsync<int>((processor, _) =>
            {
                Assert(processor != null, "Processor must be supplied.");
                return new ValueTask<int>(42);
            });

            Assert(result == 42, "Instruction result should flow through the engine.");
            Assert(engine.Instructions.ActiveCount == 0, "Lease should be returned after execution.");
            Assert(engine.Instructions.AvailableCount == 1, "Returned processor should be retained.");
        }

        private static async Task ReusesInstructionProcessor()
        {
            using var engine = new Core.WildsEngine(Options(retainedInstructions: 1, concurrentInstructions: 1));
            engine.Initialize();

            IInstructionsProcessor first = null!;
            IInstructionsProcessor second = null!;

            await engine.ExecuteInstructionAsync<int>((processor, _) =>
            {
                first = processor;
                return new ValueTask<int>(1);
            });

            await engine.ExecuteInstructionAsync<int>((processor, _) =>
            {
                second = processor;
                return new ValueTask<int>(2);
            });

            Assert(ReferenceEquals(first, second), "Sequential leases should reuse the retained processor.");
            Assert(engine.Instructions.CreatedCount == 1, "Only one instruction processor should have been created.");
        }

        private static async Task ReturnsProcessorAfterOperationFailure()
        {
            using var engine = new Core.WildsEngine(Options(retainedInstructions: 1, concurrentInstructions: 1));
            engine.Initialize();

            await AssertThrowsAsync<ExpectedTestException>(async () =>
            {
                await engine.ExecuteInstructionAsync<int>((_, __) =>
                    throw new ExpectedTestException());
            });

            Assert(engine.Instructions.ActiveCount == 0, "Failed operation must not leak an active lease.");
            Assert(engine.Instructions.AvailableCount == 1, "Processor should return after operation failure.");
        }

        private static async Task HonorsCancellationWhileWaiting()
        {
            using var engine = new Core.WildsEngine(Options(retainedInstructions: 1, concurrentInstructions: 1));
            engine.Initialize();

            using var held = engine.Instructions.Rent();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await AssertThrowsAsync<OperationCanceledException>(async () =>
            {
                await engine.ExecuteInstructionAsync<int>((_, __) => new ValueTask<int>(1), cts.Token);
            });

            Assert(engine.Instructions.ActiveCount == 1, "Canceled waiter must not acquire a second lease.");
        }

        private static void RejectsUseAfterDisposal()
        {
            var engine = new Core.WildsEngine(Options());
            engine.Dispose();

            AssertThrows<ObjectDisposedException>(() => engine.Initialize());
        }

        private static async Task ConcurrentLoadRespectsLimit()
        {
            const int concurrentLimit = 3;
            const int operationCount = 24;

            using var engine = new Core.WildsEngine(
                Options(retainedInstructions: concurrentLimit, concurrentInstructions: concurrentLimit));
            engine.Initialize();

            var current = 0;
            var observedMaximum = 0;
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var tasks = new List<Task<int>>(operationCount);

            for (var i = 0; i < operationCount; i++)
            {
                tasks.Add(engine.ExecuteInstructionAsync<int>(async (_, token) =>
                {
                    var now = Interlocked.Increment(ref current);
                    UpdateMaximum(ref observedMaximum, now);
                    if (now >= concurrentLimit)
                        entered.TrySetResult(true);

                    try
                    {
                        await release.Task.ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        return 1;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref current);
                    }
                }).AsTask());
            }

            await entered.Task.ConfigureAwait(false);
            Assert(engine.Instructions.ActiveCount <= concurrentLimit, "Active leases exceeded MaximumConcurrent.");
            release.TrySetResult(true);
            await Task.WhenAll(tasks).ConfigureAwait(false);

            Assert(observedMaximum == concurrentLimit, "Concurrent load did not exercise the configured limit.");
            Assert(engine.Instructions.ActiveCount == 0, "All concurrent leases should be returned.");
            Assert(engine.Instructions.CreatedCount <= concurrentLimit, "Pool created more processors than concurrent leasing permits.");
        }

        private static void UpdateMaximum(ref int target, int value)
        {
            while (true)
            {
                var current = Volatile.Read(ref target);
                if (value <= current)
                    return;
                if (Interlocked.CompareExchange(ref target, value, current) == current)
                    return;
            }
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                Passed.Add(name);
            }
            catch (Exception ex)
            {
                throw new Exception($"[FAIL] {name}: {ex.Message}", ex);
            }
        }

        private static async Task RunAsync(string name, Func<Task> test)
        {
            try
            {
                await test().ConfigureAwait(false);
                Passed.Add(name);
            }
            catch (Exception ex)
            {
                throw new Exception($"[FAIL] {name}: {ex.Message}", ex);
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void AssertThrows<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }

            throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
        }

        private static async Task AssertThrowsAsync<TException>(Func<Task> action)
            where TException : Exception
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (TException)
            {
                return;
            }

            throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
        }

        private sealed class ExpectedTestException : Exception
        {
        }
    }
}
