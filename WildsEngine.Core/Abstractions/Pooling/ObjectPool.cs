using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Pooling
{
    /// <summary>
    /// Thread-safe object pool with bounded retention
    /// and bounded concurrent leasing.
    /// </summary>
    public class ObjectPool<T> :
        IObjectPool<T>,
        IDisposable
        where T : class
    {
        private readonly ConcurrentQueue<T> _available =
            new ConcurrentQueue<T>();

        private readonly Func<T> _factory;

        private readonly SemaphoreSlim _concurrencyGate;

        private int _availableCount;
        private int _createdCount;
        private int _activeCount;
        private int _disposed;

        public ObjectPool(
            Func<T> factory,
            int maximumRetained,
            int maximumConcurrent)
        {
            _factory = factory ??
                       throw new ArgumentNullException(nameof(factory));

            if (maximumRetained < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumRetained));
            }

            if (maximumConcurrent <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumConcurrent));
            }

            MaximumRetained = maximumRetained;
            MaximumConcurrent = maximumConcurrent;

            _concurrencyGate = new SemaphoreSlim(
                maximumConcurrent,
                maximumConcurrent);
        }

        public int AvailableCount =>
            Volatile.Read(ref _availableCount);

        public int CreatedCount =>
            Volatile.Read(ref _createdCount);

        public int ActiveCount =>
            Volatile.Read(ref _activeCount);

        public int MaximumRetained { get; }

        public int MaximumConcurrent { get; }

        public ObjectLease<T> Rent()
        {
            ThrowIfDisposed();

            _concurrencyGate.Wait();

            try
            {
                // The pool may have been disposed while waiting.
                ThrowIfDisposed();

                return CreateLease();
            }
            catch
            {
                _concurrencyGate.Release();

                throw;
            }
        }

        public async ValueTask<ObjectLease<T>> RentAsync(
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            await _concurrencyGate
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                // The pool may have been disposed while waiting.
                ThrowIfDisposed();

                return CreateLease();
            }
            catch
            {
                _concurrencyGate.Release();

                throw;
            }
        }

        public bool Return(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var remainingActive =
                Interlocked.Decrement(ref _activeCount);

            if (remainingActive < 0)
            {
                Interlocked.Increment(ref _activeCount);

                throw new InvalidOperationException(
                    "An object was returned without an active lease.");
            }

            try
            {
                if (Volatile.Read(ref _disposed) != 0)
                {
                    DisposeInstance(instance);

                    return false;
                }

                try
                {
                    if (instance is
                        IResettablePoolObject
                        resettable)
                    {
                        if (!resettable.TryReset())
                        {
                            DisposeInstance(instance);

                            return false;
                        }
                    }
                }
                catch
                {
                    DisposeInstance(instance);

                    throw;
                }

                var retained =
                    TryRetain(instance);

                if (!retained)
                    DisposeInstance(instance);

                return retained;
            }
            finally
            {
                _concurrencyGate.Release();
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(
                    ref _disposed,
                    1) != 0)
            {
                return;
            }

            while (_available.TryDequeue(out var instance))
            {
                Interlocked.Decrement(
                    ref _availableCount);

                DisposeInstance(instance);
            }

            // Intentionally do not dispose the semaphore here.
            // Active leases and waiting renters may still need
            // to interact with it while observing pool disposal.
        }

        private ObjectLease<T>
            CreateLease()
        {
            T instance;

            if (_available.TryDequeue(out var pooled))
            {
                Interlocked.Decrement(
                    ref _availableCount);

                instance = pooled;
            }
            else
            {
                instance = _factory();

                if (instance == null)
                {
                    throw new InvalidOperationException(
                        "The object pool factory returned null.");
                }

                Interlocked.Increment(
                    ref _createdCount);
            }

            Interlocked.Increment(
                ref _activeCount);

            return new ObjectLease<T>(
                this,
                instance);
        }

        private bool TryRetain(T instance)
        {
            while (true)
            {
                if (Volatile.Read(ref _disposed) != 0)
                    return false;

                var available =
                    Volatile.Read(ref _availableCount);

                if (available >= MaximumRetained)
                    return false;

                if (Interlocked.CompareExchange(
                        ref _availableCount,
                        available + 1,
                        available) != available)
                {
                    continue;
                }

                if (Volatile.Read(ref _disposed) != 0)
                {
                    Interlocked.Decrement(
                        ref _availableCount);

                    return false;
                }

                _available.Enqueue(instance);

                return true;
            }
        }

        private static void DisposeInstance(
            T instance)
        {
            if (instance is IDisposable disposable)
                disposable.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) != 0)
            {
                throw new ObjectDisposedException(
                    nameof(ObjectPool<T>));
            }
        }
    }
}