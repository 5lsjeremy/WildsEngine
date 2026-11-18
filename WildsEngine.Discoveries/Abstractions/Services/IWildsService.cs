using System.Threading;
using System.Threading.Tasks;

namespace WildsEngine.Discoveries.Abstractions.Services
{
    public interface IWildsService<in TRequest, TResult> : IIdentifier
        where TRequest : class
        where TResult : class
    {
        bool IsRunning { get; }

        Task<IWildsServiceResult<TResult>> ExecuteAsync(
            TRequest request,
            object? provenance = null,
            System.Guid? parentExecutionId = null,
            System.Guid? pulseId = null,
            CancellationToken cancellationToken = default);
    }
}
