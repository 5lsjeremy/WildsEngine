using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Pooling;
using WildsEngine.Tests.Logging;

namespace WildsEngine.Tests.Pooling
{
    public static class ObjectPoolTests
    {
        private static int _passed;

        public static async Task RunAllAsync()
        {
            _passed = 0;
            TestLog.Write("=== ObjectPool Tests ===");
            Run("Rent creates instance", RentCreatesInstance);
            Run("Return retains instance", ReturnRetainsInstance);
            Run("Rent reuses instance", RentReusesInstance);
            Run("Double lease dispose is safe", DoubleLeaseDisposeIsSafe);
            Run("Retention limit is enforced", RetentionLimitIsEnforced);
            Run("Reset is called on return", ResetIsCalledOnReturn);
            Run("Failed reset rejects instance", FailedResetRejectsInstance);
            Run("Dispose clears retained instances", DisposeClearsRetainedInstances);
            Run("Rent after dispose throws", RentAfterDisposeThrows);
            await RunAsync("Concurrent rent is bounded", ConcurrentRentIsBoundedAsync);
            await RunAsync("Waiting rent can be cancelled", WaitingRentCanBeCancelledAsync);
            TestLog.Write($"=== ObjectPool Tests Passed: {_passed}/{_passed} ===");
        }

        private static void RentCreatesInstance()
        {
            using var pool = CreatePool();
            using var lease = pool.Rent();
            LogState(pool, "After first rent");
            Assert(lease.Instance != null, "Rent should return an instance.");
            Assert(pool.CreatedCount == 1, "First rent should create one instance.");
            Assert(pool.ActiveCount == 1, "First rent should create one active lease.");
        }

        private static void ReturnRetainsInstance()
        {
            using var pool = CreatePool();
            var lease = pool.Rent();
            lease.Dispose();
            LogState(pool, "After return");
            Assert(pool.ActiveCount == 0, "Returned instance should no longer be active.");
            Assert(pool.AvailableCount == 1, "Returned instance should be retained.");
        }

        private static void RentReusesInstance()
        {
            using var pool = CreatePool();
            TestPoolObject first;
            using (var lease = pool.Rent()) first = lease.Instance;
            using (var lease = pool.Rent())
                Assert(ReferenceEquals(first, lease.Instance), "Second rent should reuse retained instance.");
            Assert(pool.CreatedCount == 1, "Reusing an instance should not create another object.");
        }

        private static void DoubleLeaseDisposeIsSafe()
        {
            using var pool = CreatePool();
            var lease = pool.Rent();
            lease.Dispose();
            lease.Dispose();
            Assert(pool.ActiveCount == 0, "Double lease disposal must not double-return.");
            Assert(pool.AvailableCount == 1, "Double disposal must retain only one instance.");
        }

        private static void RetentionLimitIsEnforced()
        {
            using var pool = new ObjectPool<TestPoolObject>(() => new TestPoolObject(), 1, 2);
            var first = pool.Rent();
            var second = pool.Rent();
            first.Dispose();
            second.Dispose();
            LogState(pool, "After retention test");
            Assert(pool.AvailableCount == 1, "Pool should retain no more than MaximumRetained.");
            Assert(pool.CreatedCount == 2, "Two simultaneous leases should require two instances.");
        }

        private static void ResetIsCalledOnReturn()
        {
            using var pool = CreatePool();
            TestPoolObject instance;
            using (var lease = pool.Rent()) { instance = lease.Instance; instance.Value = 42; }
            TestLog.State($"ResetCount = {instance.ResetCount}; Value = {instance.Value}");
            Assert(instance.ResetCount == 1, "Returning pooled object should call TryReset.");
            Assert(instance.Value == 0, "Reset should clear transient state.");
        }

        private static void FailedResetRejectsInstance()
        {
            using var pool = new ObjectPool<TestPoolObject>(() => new TestPoolObject { AllowReset = false }, 4, 4);
            using (var lease = pool.Rent()) { }
            Assert(pool.ActiveCount == 0, "Rejected instance should no longer be active.");
            Assert(pool.AvailableCount == 0, "Failed reset should prevent retention.");
        }

        private static void DisposeClearsRetainedInstances()
        {
            var pool = CreatePool();
            TestPoolObject instance;
            using (var lease = pool.Rent()) instance = lease.Instance;
            pool.Dispose();
            Assert(pool.AvailableCount == 0, "Disposal should clear retained instances.");
            Assert(instance.IsDisposed, "Retained disposable instance should be disposed.");
        }

        private static void RentAfterDisposeThrows()
        {
            var pool = CreatePool();
            pool.Dispose();
            var threw = false;
            try { pool.Rent(); }
            catch (ObjectDisposedException) { threw = true; TestLog.Expected("ObjectDisposedException"); }
            Assert(threw, "Rent after pool disposal should throw.");
        }

        private static async Task ConcurrentRentIsBoundedAsync()
        {
            using var pool = new ObjectPool<TestPoolObject>(() => new TestPoolObject(), 2, 2);
            using var first = await pool.RentAsync();
            using var second = await pool.RentAsync();
            using var cancellation = new CancellationTokenSource();
            var waiting = pool.RentAsync(cancellation.Token);
            Assert(!waiting.IsCompletedSuccessfully, "Additional asynchronous rent should wait.");
            cancellation.Cancel();
            var cancelled = false;
            try { using var ignored = await waiting; }
            catch (OperationCanceledException) { cancelled = true; TestLog.Expected("OperationCanceledException"); }
            Assert(cancelled, "Waiting rent should observe cancellation.");
            Assert(pool.ActiveCount == 2, "Existing leases should remain active.");
        }

        private static async Task WaitingRentCanBeCancelledAsync()
        {
            using var pool = new ObjectPool<TestPoolObject>(() => new TestPoolObject(), 1, 1);
            using var active = await pool.RentAsync();
            using var cancellation = new CancellationTokenSource();
            var waiting = pool.RentAsync(cancellation.Token);
            cancellation.Cancel();
            var cancelled = false;
            try { using var ignored = await waiting; }
            catch (OperationCanceledException) { cancelled = true; TestLog.Expected("OperationCanceledException"); }
            Assert(cancelled, "Cancellation should release a waiting renter.");
            Assert(pool.ActiveCount == 1, "Cancellation must not affect existing lease.");
        }

        private static ObjectPool<TestPoolObject> CreatePool() =>
            new ObjectPool<TestPoolObject>(() => new TestPoolObject(), 4, 4);

        private static void Run(string name, Action test)
        {
            TestLog.Test(name);
            try { test(); _passed++; TestLog.Pass(name); }
            catch (Exception ex) { TestLog.Fail(name, ex); throw; }
        }

        private static async Task RunAsync(string name, Func<Task> test)
        {
            TestLog.Test(name);
            try { await test().ConfigureAwait(false); _passed++; TestLog.Pass(name); }
            catch (Exception ex) { TestLog.Fail(name, ex); throw; }
        }

        private static void LogState(ObjectPool<TestPoolObject> pool, string message) =>
            TestLog.State($"{message} | Active={pool.ActiveCount}, Available={pool.AvailableCount}, Created={pool.CreatedCount}, Maximum={pool.MaximumConcurrent}");

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException($"ObjectPool test failed: {message}");
        }

        private sealed class TestPoolObject : IResettablePoolObject, IDisposable
        {
            public int Value { get; set; }
            public int ResetCount { get; private set; }
            public bool AllowReset { get; set; } = true;
            public bool IsDisposed { get; private set; }
            public bool TryReset() { ResetCount++; if (!AllowReset) return false; Value = 0; return true; }
            public void Dispose() => IsDisposed = true;
        }
    }
}
