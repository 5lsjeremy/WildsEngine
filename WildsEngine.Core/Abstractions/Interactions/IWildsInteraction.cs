using System;
using System.Threading;
using System.Threading.Tasks;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents the public interaction boundary into Wilds Core.
    ///
    /// Callers provide opaque runtime input.
    /// Wilds owns the processing and temporal lifecycle.
    ///
    /// Higher-level systems do not need to know which internal
    /// processor ultimately handles the interaction.
    /// </summary>
    public interface IWildsInteraction
    {
        ValueTask<IMoment> ExecuteAsync(
            object? request,
            Func<object?, CancellationToken, ValueTask<object?>> processor,
            CancellationToken cancellationToken = default);
    }
}