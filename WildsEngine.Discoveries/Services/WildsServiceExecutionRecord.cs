using System;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Services
{
    public sealed class WildsServiceExecutionRecord : IdentifierBase, IWildsServiceExecutionRecord
    {
        public WildsServiceExecutionRecord(
            IWildsServiceContext context,
            DateTimeOffset? startedAt,
            DateTimeOffset? completedAt,
            TimeSpan? duration,
            WildsServiceExecutionOutcome outcome,
            string? failureType = null,
            string? failureMessage = null)
            : base(context?.Id ?? throw new ArgumentNullException(nameof(context)), context.Slug)
        {
            ServiceId = context.ServiceId;
            ServiceSlug = context.ServiceSlug;
            ParentExecutionId = context.ParentExecutionId;
            PulseId = context.PulseId;
            RequestedAt = context.RequestedAt;
            StartedAt = startedAt;
            CompletedAt = completedAt;
            Duration = duration;
            Outcome = outcome;
            Provenance = context.Provenance;
            FailureType = failureType;
            FailureMessage = failureMessage;
        }
        public Guid ServiceId { get; }
        public string ServiceSlug { get; }
        public Guid? ParentExecutionId { get; }
        public Guid? PulseId { get; }
        public DateTimeOffset RequestedAt { get; }
        public DateTimeOffset? StartedAt { get; }
        public DateTimeOffset? CompletedAt { get; }
        public TimeSpan? Duration { get; }
        public WildsServiceExecutionOutcome Outcome { get; }
        public object? Provenance { get; }
        public string? FailureType { get; }
        public string? FailureMessage { get; }
    }
}
