using System.Threading;
using System.Threading.Tasks;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Provides bounded synchronous and asynchronous leasing
    /// of reusable objects.
    /// </summary>
    public interface IObjectPool<T>
        where T : class
    {
        int AvailableCount { get; }

        int CreatedCount { get; }

        int ActiveCount { get; }

        int MaximumRetained { get; }

        int MaximumConcurrent { get; }

        ObjectLease<T> Rent();

        ValueTask<ObjectLease<T>> RentAsync(
            CancellationToken cancellationToken = default);

        bool Return(T instance);
    }
}