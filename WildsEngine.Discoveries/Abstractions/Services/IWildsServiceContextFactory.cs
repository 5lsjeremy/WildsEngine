using System.Threading;

namespace WildsEngine.Discoveries.Abstractions.Services
{
    public interface IWildsServiceContextFactory<out TContext>
        where TContext : IWildsServiceContext
    {
        TContext Create(
            IIdentifier service,
            object? provenance,
            System.Guid? parentExecutionId,
            System.Guid? pulseId,
            CancellationToken cancellationToken);
    }
}
