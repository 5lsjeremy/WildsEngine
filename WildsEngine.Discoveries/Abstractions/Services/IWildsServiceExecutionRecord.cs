using System;

namespace WildsEngine.Discoveries.Abstractions.Services
{
    public interface IWildsServiceExecutionRecord : IIdentifier
    {
        Guid ServiceId { get; }
        string ServiceSlug { get; }
        Guid? ParentExecutionId { get; }
        Guid? PulseId { get; }
        DateTimeOffset RequestedAt { get; }
        DateTimeOffset? StartedAt { get; }
        DateTimeOffset? CompletedAt { get; }
        TimeSpan? Duration { get; }
        WildsServiceExecutionOutcome Outcome { get; }
        object? Provenance { get; }
        string? FailureType { get; }
        string? FailureMessage { get; }
    }
}
