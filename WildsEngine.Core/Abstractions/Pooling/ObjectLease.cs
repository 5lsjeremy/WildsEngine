using System;
using System.Threading;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents temporary ownership of an object rented from a pool.
    /// Disposing the lease returns the object to its originating pool.
    /// </summary>
    public sealed class ObjectLease<T> : IDisposable
        where T : class
    {
        private IObjectPool<T>? _pool;
        private T? _instance;

        internal ObjectLease(
            IObjectPool<T> pool,
            T instance)
        {
            _pool = pool ??
                    throw new ArgumentNullException(nameof(pool));

            _instance = instance ??
                        throw new ArgumentNullException(nameof(instance));
        }

        public T Instance
        {
            get
            {
                return _instance ??
                       throw new ObjectDisposedException(
                           nameof(ObjectLease<T>));
            }
        }

        public void Dispose()
        {
            var instance =
                Interlocked.Exchange(
                    ref _instance,
                    null);

            var pool =
                Interlocked.Exchange(
                    ref _pool,
                    null);

            if (instance == null || pool == null)
                return;

            pool.Return(instance);
        }
    }
}