using System;
using System.Threading;

namespace WildsEngine.Discoveries.Services
{
    public sealed class WildsServiceContext : WildsServiceContextBase
    {
        public WildsServiceContext(
            Guid id, string slug, Guid serviceId, string serviceSlug,
            Guid? parentExecutionId, Guid? pulseId, DateTimeOffset requestedAt,
            object? provenance, CancellationToken cancellationToken)
            : base(id, slug, serviceId, serviceSlug, parentExecutionId, pulseId,
                   requestedAt, provenance, cancellationToken)
        {
        }
    }
}
