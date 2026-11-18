using System;
using System.Threading;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Services;

namespace WildsEngine.Discoveries.Services
{
    public sealed class WildsServiceContextFactory : WildsServiceContextFactoryBase<WildsServiceContext>
    {
        public WildsServiceContextFactory(IWildsServiceClock clock) : base(clock) { }

        protected override WildsServiceContext CreateCore(
            Guid executionId, string executionSlug, IIdentifier service,
            object? provenance, Guid? parentExecutionId, Guid? pulseId,
            DateTimeOffset requestedAt, CancellationToken cancellationToken)
        {
            return new WildsServiceContext(
                executionId, executionSlug, service.Id, service.Slug,
                parentExecutionId, pulseId, requestedAt, provenance, cancellationToken);
        }
    }
}
