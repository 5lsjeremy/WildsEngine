using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Tests;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;

namespace WildsEngine.Tests
{
    public static class WildsEngineCertification
    {
        private static readonly List<string> Passed =
            new List<string>();

        public static async Task RunAllAsync()
        {
            Passed.Clear();

            TestLog.Write(
                "=== WildsEngine Composition Tests ===");

            Run(
                "Constructs isolated engine",
                ConstructsIsolatedEngine);

            Run(
                "Initialize is idempotent",
                InitializeIsIdempotent);

            Run(
                "Shutdown is idempotent",
                ShutdownIsIdempotent);

            await RunAsync(
                "Rejects processing before initialization",
                RejectsProcessingBeforeInitialization);

            await RunAsync(
                "Executes instruction lease",
                ExecutesInstructionLease);

            await RunAsync(
                "Reuses instruction processor",
                ReusesInstructionProcessor);

            await RunAsync(
                "Returns processor after operation failure",
                ReturnsProcessorAfterOperationFailure);

            await RunAsync(
                "Honors cancellation while waiting",
                HonorsCancellationWhileWaiting);

            Run(
                "Rejects use after disposal",
                RejectsUseAfterDisposal);

            await RunAsync(
                "Concurrent load respects limit",
                ConcurrentLoadRespectsLimit);

            TestLog.Write(
                $"WildsEngine certification passed: " +
                $"{Passed.Count}/{Passed.Count}");
        }

        private static WildsEngineOptions Options(
            int retainedInstructions = 4,
            int concurrentInstructions = 4,
            int retainedTasks = 8,
            int concurrentTasks = 8)
        {
            return new WildsEngineOptions
            {
                Layer =
                    "certification",

                RetainedInstructionsProcessors =
                    retainedInstructions,

                ConcurrentInstructionsProcessors =
                    concurrentInstructions,

                RetainedTaskProcessors =
                    retainedTasks,

                ConcurrentTaskProcessors =
                    concurrentTasks
            };
        }

        private static IComponent CreateComponent(
            string layer)
        {
            return new TestComponent(
                layer);
        }

        private static Core.WildsEngine CreateEngine(
            string layer,
            int retainedInstructions = 4,
            int concurrentInstructions = 4,
            int retainedTasks = 8,
            int concurrentTasks = 8)
        {
            var component =
                CreateComponent(
                    layer);

            var options =
                Options(
                    retainedInstructions,
                    concurrentInstructions,
                    retainedTasks,
                    concurrentTasks);

            return new Core.WildsEngine(
                component,
                options);
        }

        private static void ConstructsIsolatedEngine()
        {
            using var a =
                CreateEngine(
                    "certification.engine.a");

            using var b =
                CreateEngine(
                    "certification.engine.b");

            Assert(
                !ReferenceEquals(a, b),
                "Engine instances must be independent.");

            Assert(
                !ReferenceEquals(
                    a.Instructions,
                    b.Instructions),
                "Instruction pools must be instance-owned.");
        }

        private static void InitializeIsIdempotent()
        {
            using var engine =
                CreateEngine(
                    "certification.initialize");

            Assert(
                engine.Initialize(),
                "First Initialize() should succeed.");

            Assert(
                engine.IsInitialized,
                "Engine should report initialized.");

            Assert(
                !engine.Initialize(),
                "Second Initialize() should return false.");
        }

        private static void ShutdownIsIdempotent()
        {
            using var engine =
                CreateEngine(
                    "certification.shutdown");

            Assert(
                engine.Initialize(),
                "Initialize() should succeed.");

            Assert(
                engine.Shutdown(),
                "First Shutdown() should succeed.");

            Assert(
                !engine.IsInitialized,
                "Engine should report stopped.");

            Assert(
                !engine.Shutdown(),
                "Second Shutdown() should return false.");
        }

        private static async Task
            RejectsProcessingBeforeInitialization()
        {
            using var engine =
                CreateEngine(
                    "certification.uninitialized");

            await AssertThrowsAsync<InvalidOperationException>(
                async () =>
                {
                    await engine.ExecuteInstructionAsync<int>(
                        (_, __) =>
                            new ValueTask<int>(1));
                });
        }

        private static async Task
            ExecutesInstructionLease()
        {
            using var engine =
                CreateEngine(
                    "certification.execute");

            Assert(
                engine.Initialize(),
                "Initialize() should succeed.");

            var result =
                await engine.ExecuteInstructionAsync<int>(
                    (processor, _) =>
                    {
                        Assert(
                            processor != null,
                            "Processor must be supplied.");

                        return new ValueTask<int>(
                            42);
                    });

            LogPool(
                engine,
                "After instruction execution");

            Assert(
                result == 42,
                "Instruction result should flow through the engine.");

            Assert(
                engine.Instructions.ActiveCount == 0,
                "Lease should be returned after execution.");

            Assert(
                engine.Instructions.AvailableCount == 1,
                "Returned processor should be retained.");
        }

        private static async Task
            ReusesInstructionProcessor()
        {
            using var engine =
                CreateEngine(
                    "certification.reuse",
                    retainedInstructions: 1,
                    concurrentInstructions: 1);

            Assert(
                engine.Initialize(),
                "Initialize() should succeed.");

            IInstructionsProcessor? first =
                null;

            IInstructionsProcessor? second =
                null;

            await engine.ExecuteInstructionAsync<int>(
                (processor, _) =>
                {
                    first =
                        processor;

                    return new ValueTask<int>(
                        1);
                });

            await engine.ExecuteInstructionAsync<int>(
                (processor, _) =>
                {
                    second =
                        processor;

                    return new ValueTask<int>(
                        2);
                });

            Assert(
                first != null,
                "First instruction processor should be captured.");

            Assert(
                second != null,
                "Second instruction processor should be captured.");

            Assert(
                ReferenceEquals(
                    first,
                    second),
                "Sequential leases should reuse the retained processor.");

            Assert(
                engine.Instructions.CreatedCount == 1,
                "Only one instruction processor should have been created.");
        }

        private static async Task
            ReturnsProcessorAfterOperationFailure()
        {
            using var engine =
                CreateEngine(
                    "certification.failure",
                    retainedInstructions: 1,
                    concurrentInstructions: 1);

            Assert(
                engine.Initialize(),
                "Initialize() should succeed.");

            await AssertThrowsAsync<ExpectedTestException>(
                async () =>
                {
                    await engine.ExecuteInstructionAsync<int>(
                        (_, __) =>
                            throw new ExpectedTestException());
                });

            Assert(
                engine.Instructions.ActiveCount == 0,
                "Failed operation must not leak an active lease.");

            Assert(
                engine.Instructions.AvailableCount == 1,
                "Processor should return after operation failure.");
        }

        private static async Task
            HonorsCancellationWhileWaiting()
        {
            using var engine =
                CreateEngine(
                    "certification.cancellation",
                    retainedInstructions: 1,
                    concurrentInstructions: 1);

            Assert(
                engine.Initialize(),
                "Initialize() should succeed.");

            using var held =
                engine.Instructions.Rent();

            using var cancellation =
                new CancellationTokenSource();

            cancellation.Cancel();

            await AssertThrowsAsync<OperationCanceledException>(
                async () =>
                {
                    await engine.ExecuteInstructionAsync<int>(
                        (_, __) =>
                            new ValueTask<int>(1),
                        cancellation.Token);
                });

            Assert(
                engine.Instructions.ActiveCount == 1,
                "Canceled waiter must not acquire a second lease.");
        }

        private static void RejectsUseAfterDisposal()
        {
            var engine =
                CreateEngine(
                    "certification.disposal");

            engine.Dispose();

            AssertThrows<ObjectDisposedException>(
                () =>
                    engine.Initialize());
        }

        private static async Task
            ConcurrentLoadRespectsLimit()
        {
            const int limit = 3;
            const int count = 24;

            using var engine =
                CreateEngine(
                    "certification.concurrent",
                    retainedInstructions: limit,
                    concurrentInstructions: limit);

            Assert(
                engine.Initialize(),
                "Initialize() should succeed.");

            var current = 0;
            var maximum = 0;

            var release =
                new TaskCompletionSource<bool>(
                    TaskCreationOptions
                        .RunContinuationsAsynchronously);

            var entered =
                new TaskCompletionSource<bool>(
                    TaskCreationOptions
                        .RunContinuationsAsynchronously);

            var tasks =
                new List<Task<int>>(
                    count);

            for (var i = 0; i < count; i++)
            {
                tasks.Add(
                    engine.ExecuteInstructionAsync<int>(
                            async (_, token) =>
                            {
                                var now =
                                    Interlocked.Increment(
                                        ref current);

                                UpdateMaximum(
                                    ref maximum,
                                    now);

                                if (now >= limit)
                                {
                                    entered.TrySetResult(
                                        true);
                                }

                                try
                                {
                                    await release.Task
                                        .ConfigureAwait(false);

                                    token.ThrowIfCancellationRequested();

                                    return 1;
                                }
                                finally
                                {
                                    Interlocked.Decrement(
                                        ref current);
                                }
                            })
                        .AsTask());
            }

            await entered.Task
                .ConfigureAwait(false);

            LogPool(
                engine,
                "At concurrency limit");

            Assert(
                engine.Instructions.ActiveCount <= limit,
                "Active leases exceeded MaximumConcurrent.");

            release.TrySetResult(
                true);

            await Task.WhenAll(
                    tasks)
                .ConfigureAwait(false);

            Assert(
                maximum == limit,
                "Concurrent load did not exercise the configured limit.");

            Assert(
                engine.Instructions.ActiveCount == 0,
                "All concurrent leases should be returned.");

            Assert(
                engine.Instructions.CreatedCount <= limit,
                "Pool created more processors than " +
                "concurrent leasing permits.");
        }

        private static void UpdateMaximum(
            ref int target,
            int value)
        {
            while (true)
            {
                var current =
                    Volatile.Read(
                        ref target);

                if (value <= current)
                    return;

                if (Interlocked.CompareExchange(
                        ref target,
                        value,
                        current) == current)
                {
                    return;
                }
            }
        }

        private static void Run(
            string name,
            Action test)
        {
            TestLog.Test(
                name);

            try
            {
                test();

                Passed.Add(
                    name);

                TestLog.Pass(
                    name);
            }
            catch (Exception exception)
            {
                TestLog.Fail(
                    name,
                    exception);

                throw;
            }
        }

        private static async Task RunAsync(
            string name,
            Func<Task> test)
        {
            TestLog.Test(
                name);

            try
            {
                await test()
                    .ConfigureAwait(false);

                Passed.Add(
                    name);

                TestLog.Pass(
                    name);
            }
            catch (Exception exception)
            {
                TestLog.Fail(
                    name,
                    exception);

                throw;
            }
        }

        private static void LogPool(
            Core.WildsEngine engine,
            string label)
        {
            TestLog.State(
                $"{label} | " +
                $"Active={engine.Instructions.ActiveCount}, " +
                $"Available={engine.Instructions.AvailableCount}, " +
                $"Created={engine.Instructions.CreatedCount}, " +
                $"Maximum={engine.Instructions.MaximumConcurrent}");
        }

        private static void Assert(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    message);
            }
        }

        private static void AssertThrows<TException>(
            Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                TestLog.Expected(
                    typeof(TException).Name);

                return;
            }

            throw new InvalidOperationException(
                $"Expected {typeof(TException).Name}.");
        }

        private static async Task AssertThrowsAsync<TException>(
            Func<Task> action)
            where TException : Exception
        {
            try
            {
                await action()
                    .ConfigureAwait(false);
            }
            catch (TException)
            {
                TestLog.Expected(
                    typeof(TException).Name);

                return;
            }

            throw new InvalidOperationException(
                $"Expected {typeof(TException).Name}.");
        }

        private sealed class ExpectedTestException :
            Exception
        {
        }
    }
}