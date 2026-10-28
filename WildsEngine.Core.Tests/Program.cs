using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Benchmarks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Pooling;

namespace WildsEngine.ConsoleTests
{
    internal static class Program
    {
        private static int _passed;
        private static int _failed;

        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine(
                "============================================");
            Console.WriteLine(
                " WildsEngine Console Test Runner");
            Console.WriteLine(
                "============================================");
            Console.WriteLine();

            Run(
                "Pool rents and returns",
                TestRentAndReturn);

            Run(
                "Lease double disposal is safe",
                TestDoubleLeaseDispose);

            Run(
                "Pool respects maximum retention",
                TestMaximumRetention);

            Run(
                "Reset rejection prevents retention",
                TestResetRejection);

            Run(
                "Reset exception releases capacity",
                TestResetException);

            Run(
                "Pool rejects rent after disposal",
                TestRentAfterDispose);

            Run(
                "Pool disposes idle retained objects",
                TestDisposeIdleObjects);

            Run(
                "Pool disposes returned object after disposal",
                TestReturnAfterDispose);

            await RunAsync(
                "Async renter waits for available capacity",
                TestAsyncContention);

            await RunAsync(
                "Async waiting rent supports cancellation",
                TestAsyncCancellation);

            await RunAsync(
                "Waiting renter is rejected after disposal",
                TestWaitingRentAfterDispose);

            Console.WriteLine();
            Console.WriteLine(
                "============================================");
            Console.WriteLine(
                " Processor Pooling Benchmark");
            Console.WriteLine(
                "============================================");
            Console.WriteLine();

            RunBenchmark();

            Console.WriteLine();
            Console.WriteLine(
                "============================================");
            Console.WriteLine(
                " Results");
            Console.WriteLine(
                "============================================");
            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                $" Passed: {_passed}");

            Console.ResetColor();

            if (_failed > 0)
            {
                Console.ForegroundColor =
                    ConsoleColor.Red;
            }

            Console.WriteLine(
                $" Failed: {_failed}");

            Console.ResetColor();

            Console.WriteLine();

            return _failed == 0
                ? 0
                : 1;
        }

        private static void TestRentAndReturn()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject(),
                    maximumRetained: 4,
                    maximumConcurrent: 4);

            AssertEqual(
                0,
                pool.ActiveCount,
                "Pool should begin with no active objects.");

            using (var lease = pool.Rent())
            {
                AssertNotNull(
                    lease.Instance,
                    "Rent must return an instance.");

                AssertEqual(
                    1,
                    pool.ActiveCount,
                    "Pool should contain one active lease.");
            }

            AssertEqual(
                0,
                pool.ActiveCount,
                "Returned lease must no longer be active.");

            AssertEqual(
                1,
                pool.AvailableCount,
                "Returned object should be retained.");

            AssertEqual(
                1,
                pool.CreatedCount,
                "Only one instance should have been created.");
        }

        private static void TestDoubleLeaseDispose()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject(),
                    maximumRetained: 4,
                    maximumConcurrent: 4);

            var lease = pool.Rent();

            lease.Dispose();
            lease.Dispose();

            AssertEqual(
                0,
                pool.ActiveCount,
                "Double disposal must not decrement active count twice.");

            AssertEqual(
                1,
                pool.AvailableCount,
                "Double disposal must not return the object twice.");

            AssertEqual(
                1,
                pool.CreatedCount,
                "Only one object should have been created.");
        }

        private static void TestMaximumRetention()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject(),
                    maximumRetained: 2,
                    maximumConcurrent: 4);

            var lease1 = pool.Rent();
            var lease2 = pool.Rent();
            var lease3 = pool.Rent();
            var lease4 = pool.Rent();

            var object1 = lease1.Instance;
            var object2 = lease2.Instance;
            var object3 = lease3.Instance;
            var object4 = lease4.Instance;

            lease1.Dispose();
            lease2.Dispose();
            lease3.Dispose();
            lease4.Dispose();

            AssertEqual(
                2,
                pool.AvailableCount,
                "Pool should retain no more than its configured maximum.");

            AssertEqual(
                4,
                pool.CreatedCount,
                "Four simultaneous leases should create four objects.");

            var disposedCount =
                CountDisposed(
                    object1,
                    object2,
                    object3,
                    object4);

            AssertEqual(
                2,
                disposedCount,
                "Objects exceeding retention should be disposed.");
        }

        private static void TestResetRejection()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject
                        {
                            AllowReset = false
                        },
                    maximumRetained: 4,
                    maximumConcurrent: 4);

            TestPoolObject instance;

            using (var lease = pool.Rent())
            {
                instance = lease.Instance;
            }

            AssertEqual(
                0,
                pool.ActiveCount,
                "Rejected object must no longer be active.");

            AssertEqual(
                0,
                pool.AvailableCount,
                "Rejected object must not be retained.");

            AssertTrue(
                instance.IsDisposed,
                "Rejected object should be disposed.");
        }

        private static void TestResetException()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject
                        {
                            ThrowOnReset = true
                        },
                    maximumRetained: 4,
                    maximumConcurrent: 1);

            var lease = pool.Rent();
            var instance = lease.Instance;

            AssertThrows<InvalidOperationException>(
                () => lease.Dispose(),
                "Reset exception should propagate.");

            AssertTrue(
                instance.IsDisposed,
                "Object should be disposed when reset throws.");

            AssertEqual(
                0,
                pool.ActiveCount,
                "Reset failure must release the active lease.");

            AssertEqual(
                0,
                pool.AvailableCount,
                "Object that failed reset must not be retained.");

            using var replacement =
                pool.Rent();

            AssertNotNull(
                replacement.Instance,
                "Capacity must remain usable after reset failure.");

            AssertEqual(
                2,
                pool.CreatedCount,
                "A replacement should be created after reset failure.");
        }

        private static void TestRentAfterDispose()
        {
            var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject(),
                    maximumRetained: 4,
                    maximumConcurrent: 4);

            pool.Dispose();

            AssertThrows<ObjectDisposedException>(
                () =>
                {
                    using var lease =
                        pool.Rent();
                },
                "Rent after disposal must be rejected.");
        }

        private static void TestDisposeIdleObjects()
        {
            var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject(),
                    maximumRetained: 4,
                    maximumConcurrent: 4);

            TestPoolObject instance;

            using (var lease = pool.Rent())
            {
                instance = lease.Instance;
            }

            AssertEqual(
                1,
                pool.AvailableCount,
                "Returned object should be idle before disposal.");

            pool.Dispose();

            AssertTrue(
                instance.IsDisposed,
                "Disposing the pool should dispose idle retained objects.");

            AssertEqual(
                0,
                pool.AvailableCount,
                "Disposed pool should contain no retained objects.");
        }

        private static void TestReturnAfterDispose()
        {
            var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject(),
                    maximumRetained: 4,
                    maximumConcurrent: 1);

            var lease =
                pool.Rent();

            var instance =
                lease.Instance;

            pool.Dispose();

            lease.Dispose();

            AssertTrue(
                instance.IsDisposed,
                "Object returned after pool disposal should be disposed.");

            AssertEqual(
                0,
                pool.ActiveCount,
                "Returned object must no longer be active.");

            AssertEqual(
                0,
                pool.AvailableCount,
                "Disposed pool must not retain returned objects.");
        }

        private static async Task TestAsyncContention()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject(),
                    maximumRetained: 2,
                    maximumConcurrent: 1);

            var first =
                pool.Rent();

            var waiting =
                pool.RentAsync()
                    .AsTask();

            await Task.Delay(50);

            AssertFalse(
                waiting.IsCompleted,
                "Second renter should wait while capacity is occupied.");

            first.Dispose();

            using var second =
                await waiting;

            AssertNotNull(
                second.Instance,
                "Waiting renter should receive an instance after capacity returns.");

            AssertEqual(
                1,
                pool.ActiveCount,
                "Second lease should become active.");

            AssertEqual(
                1,
                pool.CreatedCount,
                "Contention should reuse the returned object.");
        }

        private static async Task TestAsyncCancellation()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject(),
                    maximumRetained: 2,
                    maximumConcurrent: 1);

            using var first =
                pool.Rent();

            using var cancellation =
                new CancellationTokenSource();

            cancellation.CancelAfter(100);

            var canceled = false;

            try
            {
                using var second =
                    await pool.RentAsync(
                        cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                canceled = true;
            }

            AssertTrue(
                canceled,
                "Waiting asynchronous rent should honor cancellation.");

            AssertEqual(
                1,
                pool.ActiveCount,
                "Canceled renter must not affect the active lease.");
        }

        private static async Task TestWaitingRentAfterDispose()
        {
            var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () =>
                        new TestPoolObject(),
                    maximumRetained: 2,
                    maximumConcurrent: 1);

            var first =
                pool.Rent();

            var waiting =
                pool.RentAsync()
                    .AsTask();

            await Task.Delay(50);

            AssertFalse(
                waiting.IsCompleted,
                "Second renter should be waiting before disposal.");

            pool.Dispose();

            first.Dispose();

            var rejected = false;

            try
            {
                using var second =
                    await waiting;
            }
            catch (ObjectDisposedException)
            {
                rejected = true;
            }

            AssertTrue(
                rejected,
                "Waiting renter must be rejected after pool disposal.");

            AssertEqual(
                0,
                pool.ActiveCount,
                "No active leases should remain after disposal.");
        }

        private static void RunBenchmark()
        {
            try
            {
                /*
                 * Replace this with the concrete task registry
                 * used by the current WildsEngine runtime.
                 *
                 * Example:
                 *
                 * var registry =
                 *     new DefaultTaskRegistry(
                 *         "benchmark.task-registry");
                 *
                 * registry.Initialize();
                 *
                 * var benchmark =
                 *     new ProcessorPoolingBenchmark(
                 *         registry);
                 *
                 * var result =
                 *     benchmark.Run(
                 *         iterations: 100000,
                 *         maximumRetained: 64,
                 *         maximumConcurrent: 64);
                 *
                 * Console.WriteLine(result);
                 *
                 * registry.Shutdown();
                 */

                Console.ForegroundColor =
                    ConsoleColor.DarkYellow;

                Console.WriteLine(
                    " Benchmark ready; concrete task registry hookup pending.");

                Console.ResetColor();
            }
            catch (Exception exception)
            {
                Console.ForegroundColor =
                    ConsoleColor.Red;

                Console.WriteLine(
                    $" [BENCHMARK FAILED] {exception.GetType().Name}: " +
                    exception.Message);

                Console.ResetColor();
            }
        }

        private static void Run(
            string name,
            Action test)
        {
            try
            {
                test();

                _passed++;

                Console.ForegroundColor =
                    ConsoleColor.Green;

                Console.WriteLine(
                    $" [PASS] {name}");

                Console.ResetColor();
            }
            catch (Exception exception)
            {
                _failed++;

                Console.ForegroundColor =
                    ConsoleColor.Red;

                Console.WriteLine(
                    $" [FAIL] {name}");

                Console.ResetColor();

                Console.WriteLine(
                    $"        {exception.GetType().Name}: " +
                    exception.Message);
            }
        }

        private static async Task RunAsync(
            string name,
            Func<Task> test)
        {
            try
            {
                await test();

                _passed++;

                Console.ForegroundColor =
                    ConsoleColor.Green;

                Console.WriteLine(
                    $" [PASS] {name}");

                Console.ResetColor();
            }
            catch (Exception exception)
            {
                _failed++;

                Console.ForegroundColor =
                    ConsoleColor.Red;

                Console.WriteLine(
                    $" [FAIL] {name}");

                Console.ResetColor();

                Console.WriteLine(
                    $"        {exception.GetType().Name}: " +
                    exception.Message);
            }
        }

        private static void AssertTrue(
            bool condition,
            string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void AssertFalse(
            bool condition,
            string message)
        {
            if (condition)
                throw new InvalidOperationException(message);
        }

        private static void AssertNotNull(
            object? value,
            string message)
        {
            if (value == null)
                throw new InvalidOperationException(message);
        }

        private static void AssertEqual<T>(
            T expected,
            T actual,
            string message)
        {
            if (Equals(expected, actual))
                return;

            throw new InvalidOperationException(
                $"{message} Expected: {expected}. Actual: {actual}.");
        }

        private static void AssertThrows<TException>(
            Action action,
            string message)
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

            throw new InvalidOperationException(
                $"{message} Expected exception: " +
                $"{typeof(TException).Name}.");
        }

        private static int CountDisposed(
            params TestPoolObject[] instances)
        {
            var count = 0;

            for (var i = 0; i < instances.Length; i++)
            {
                if (instances[i].IsDisposed)
                    count++;
            }

            return count;
        }

        private sealed class TestPoolObject :
            IResettablePoolObject,
            IDisposable
        {
            public bool AllowReset { get; set; } = true;

            public bool ThrowOnReset { get; set; }

            public bool IsDisposed { get; private set; }

            public int ResetCount { get; private set; }

            public bool TryReset()
            {
                ResetCount++;

                if (ThrowOnReset)
                {
                    throw new InvalidOperationException(
                        "Synthetic reset failure.");
                }

                return AllowReset;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}