using System;
using System.Threading;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Services;

namespace WildsEngine.Discoveries.Services
{
    public abstract class WildsServiceContextFactoryBase<TContext> : IWildsServiceContextFactory<TContext>
        where TContext : IWildsServiceContext
    {
        private readonly IWildsServiceClock _clock;

        protected WildsServiceContextFactoryBase(IWildsServiceClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public TContext Create(
            IIdentifier service,
            object? provenance,
            Guid? parentExecutionId,
            Guid? pulseId,
            CancellationToken cancellationToken)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            var executionId = Guid.NewGuid();
            return CreateCore(
                executionId,
                "service-execution-" + executionId.ToString("N"),
                service,
                provenance,
                parentExecutionId,
                pulseId,
                _clock.GetUtcNow(),
                cancellationToken);
        }

        protected abstract TContext CreateCore(
            Guid executionId,
            string executionSlug,
            IIdentifier service,
            object? provenance,
            Guid? parentExecutionId,
            Guid? pulseId,
            DateTimeOffset requestedAt,
            CancellationToken cancellationToken);
    }
}
