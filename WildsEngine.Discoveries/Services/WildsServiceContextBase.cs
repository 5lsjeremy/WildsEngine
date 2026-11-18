using System;
using System.Threading;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Services
{
    public abstract class WildsServiceContextBase : IdentifierBase, IWildsServiceContext
    {
        protected WildsServiceContextBase(
            Guid id,
            string slug,
            Guid serviceId,
            string serviceSlug,
            Guid? parentExecutionId,
            Guid? pulseId,
            DateTimeOffset requestedAt,
            object? provenance,
            CancellationToken cancellationToken)
            : base(id, slug)
        {
            if (serviceId == Guid.Empty) throw new ArgumentException("Service context requires a service ID.", nameof(serviceId));
            if (string.IsNullOrWhiteSpace(serviceSlug)) throw new ArgumentException("Service context requires a service slug.", nameof(serviceSlug));
            ServiceId = serviceId;
            ServiceSlug = serviceSlug.Trim();
            ParentExecutionId = parentExecutionId;
            PulseId = pulseId;
            RequestedAt = requestedAt;
            Provenance = provenance;
            CancellationToken = cancellationToken;
        }

        public Guid ServiceId { get; }
        public string ServiceSlug { get; }
        public Guid? ParentExecutionId { get; }
        public Guid? PulseId { get; }
        public DateTimeOffset RequestedAt { get; }
        public object? Provenance { get; }
        public CancellationToken CancellationToken { get; }
    }
}
