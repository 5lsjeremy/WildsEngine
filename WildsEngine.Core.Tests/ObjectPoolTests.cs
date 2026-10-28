using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Pooling;

namespace WildsEngine.Tests.Pooling
{
    public static class ObjectPoolTests
    {
        public static async Task RunAllAsync()
        {
            Console.WriteLine("=== ObjectPool Tests ===");
            Console.WriteLine();

            RentCreatesInstance();
            ReturnRetainsInstance();
            RentReusesInstance();
            DoubleLeaseDisposeIsSafe();
            RetentionLimitIsEnforced();
            ResetIsCalledOnReturn();
            FailedResetRejectsInstance();
            DisposeClearsRetainedInstances();
            RentAfterDisposeThrows();

            await ConcurrentRentIsBoundedAsync();
            await WaitingRentCanBeCancelledAsync();

            Console.WriteLine();
            Console.WriteLine("=== ObjectPool Tests Passed ===");
        }

        private static void RentCreatesInstance()
        {
            using var pool = CreatePool();

            using var lease = pool.Rent();

            Assert(
                lease.Instance != null,
                "Rent should return an instance.");

            Assert(
                pool.CreatedCount == 1,
                "First rent should create one instance.");

            Assert(
                pool.ActiveCount == 1,
                "First rent should create one active lease.");

            Pass(nameof(RentCreatesInstance));
        }

        private static void ReturnRetainsInstance()
        {
            using var pool = CreatePool();

            var lease = pool.Rent();

            Assert(
                pool.ActiveCount == 1,
                "Rented instance should be active.");

            lease.Dispose();

            Assert(
                pool.ActiveCount == 0,
                "Returned instance should no longer be active.");

            Assert(
                pool.AvailableCount == 1,
                "Returned instance should be retained.");

            Pass(nameof(ReturnRetainsInstance));
        }

        private static void RentReusesInstance()
        {
            using var pool = CreatePool();

            TestPoolObject first;

            using (var lease = pool.Rent())
            {
                first = lease.Instance;
            }

            using (var lease = pool.Rent())
            {
                Assert(
                    ReferenceEquals(first, lease.Instance),
                    "Second rent should reuse retained instance.");
            }

            Assert(
                pool.CreatedCount == 1,
                "Reusing an instance should not create another object.");

            Pass(nameof(RentReusesInstance));
        }

        private static void DoubleLeaseDisposeIsSafe()
        {
            using var pool = CreatePool();

            var lease = pool.Rent();

            lease.Dispose();
            lease.Dispose();

            Assert(
                pool.ActiveCount == 0,
                "Double lease disposal must not double-return.");

            Assert(
                pool.AvailableCount == 1,
                "Double disposal must retain only one instance.");

            Pass(nameof(DoubleLeaseDisposeIsSafe));
        }

        private static void RetentionLimitIsEnforced()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () => new TestPoolObject(),
                    maximumRetained: 1,
                    maximumConcurrent: 2);

            var first = pool.Rent();
            var second = pool.Rent();

            first.Dispose();
            second.Dispose();

            Assert(
                pool.AvailableCount == 1,
                "Pool should retain no more than MaximumRetained.");

            Assert(
                pool.CreatedCount == 2,
                "Two simultaneous leases should require two instances.");

            Pass(nameof(RetentionLimitIsEnforced));
        }

        private static void ResetIsCalledOnReturn()
        {
            using var pool = CreatePool();

            TestPoolObject instance;

            using (var lease = pool.Rent())
            {
                instance = lease.Instance;
                instance.Value = 42;
            }

            Assert(
                instance.ResetCount == 1,
                "Returning pooled object should call TryReset.");

            Assert(
                instance.Value == 0,
                "Reset should clear transient state.");

            Pass(nameof(ResetIsCalledOnReturn));
        }

        private static void FailedResetRejectsInstance()
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

            using (var lease = pool.Rent())
            {
            }

            Assert(
                pool.ActiveCount == 0,
                "Rejected instance should no longer be active.");

            Assert(
                pool.AvailableCount == 0,
                "Failed reset should prevent retention.");

            Pass(nameof(FailedResetRejectsInstance));
        }

        private static void DisposeClearsRetainedInstances()
        {
            var pool = CreatePool();

            TestPoolObject instance;

            using (var lease = pool.Rent())
            {
                instance = lease.Instance;
            }

            Assert(
                pool.AvailableCount == 1,
                "Object should be retained before disposal.");

            pool.Dispose();

            Assert(
                pool.AvailableCount == 0,
                "Disposal should clear retained instances.");

            Assert(
                instance.IsDisposed,
                "Retained disposable instance should be disposed.");

            Pass(nameof(DisposeClearsRetainedInstances));
        }

        private static void RentAfterDisposeThrows()
        {
            var pool = CreatePool();

            pool.Dispose();

            var threw = false;

            try
            {
                pool.Rent();
            }
            catch (ObjectDisposedException)
            {
                threw = true;
            }

            Assert(
                threw,
                "Rent after pool disposal should throw.");

            Pass(nameof(RentAfterDisposeThrows));
        }

        private static async Task ConcurrentRentIsBoundedAsync()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () => new TestPoolObject(),
                    maximumRetained: 2,
                    maximumConcurrent: 2);

            using var first =
                await pool.RentAsync();

            using var second =
                await pool.RentAsync();

            Assert(
                pool.ActiveCount == 2,
                "Pool should allow MaximumConcurrent active leases.");

            using var cancellation =
                new CancellationTokenSource();

            var waiting =
                pool.RentAsync(cancellation.Token);

            Assert(
                !waiting.IsCompletedSuccessfully,
                "Additional asynchronous rent should wait.");

            cancellation.Cancel();

            var cancelled = false;

            try
            {
                using var ignored =
                    await waiting;
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            Assert(
                cancelled,
                "Waiting rent should observe cancellation.");

            Pass(nameof(ConcurrentRentIsBoundedAsync));
        }

        private static async Task WaitingRentCanBeCancelledAsync()
        {
            using var pool =
                new ObjectPool<TestPoolObject>(
                    factory: () => new TestPoolObject(),
                    maximumRetained: 1,
                    maximumConcurrent: 1);

            using var active =
                await pool.RentAsync();

            using var cancellation =
                new CancellationTokenSource();

            var waiting =
                pool.RentAsync(cancellation.Token);

            cancellation.Cancel();

            var cancelled = false;

            try
            {
                using var ignored =
                    await waiting;
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }

            Assert(
                cancelled,
                "Cancellation should release a waiting renter.");

            Assert(
                pool.ActiveCount == 1,
                "Cancellation must not affect existing lease.");

            Pass(nameof(WaitingRentCanBeCancelledAsync));
        }

        private static ObjectPool<TestPoolObject> CreatePool()
        {
            return new ObjectPool<TestPoolObject>(
                factory: () => new TestPoolObject(),
                maximumRetained: 4,
                maximumConcurrent: 4);
        }

        private static void Assert(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    $"ObjectPool test failed: {message}");
            }
        }

        private static void Pass(string name)
        {
            Console.WriteLine(
                $"[PASS] {name}");
        }

        private sealed class TestPoolObject :
            IResettablePoolObject,
            IDisposable
        {
            public int Value { get; set; }

            public int ResetCount { get; private set; }

            public bool AllowReset { get; set; } = true;

            public bool IsDisposed { get; private set; }

            public bool TryReset()
            {
                ResetCount++;

                if (!AllowReset)
                    return false;

                Value = 0;

                return true;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}