using System;
using System.Threading;

namespace WildsEngine.Discoveries.Abstractions.Services
{
    public interface IWildsServiceContext : IIdentifier
    {
        Guid ServiceId { get; }
        string ServiceSlug { get; }
        Guid? ParentExecutionId { get; }
        Guid? PulseId { get; }
        DateTimeOffset RequestedAt { get; }
        object? Provenance { get; }
        CancellationToken CancellationToken { get; }
    }
}
