using System;
using WildsEngine.Discoveries.Abstractions.Services;

namespace WildsEngine.Discoveries.Services
{
    public sealed class WildsServiceResult<TResult> : IWildsServiceResult<TResult>
        where TResult : class
    {
        public WildsServiceResult(Guid executionId, TResult value)
        {
            if (executionId == Guid.Empty) throw new ArgumentException("Service result requires an execution ID.", nameof(executionId));
            ExecutionId = executionId;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
        public Guid ExecutionId { get; }
        public TResult Value { get; }
    }
}
