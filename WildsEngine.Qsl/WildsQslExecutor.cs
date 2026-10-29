using System;
using System.Threading;
using System.Threading.Tasks;
using QSL.Core;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Qsl
{
    /// <summary>
    /// Executes QSL source through the complete WildsEngine
    /// request lifecycle.
    ///
    /// source
    ///     -> Pulse
    ///     -> WorkUnit
    ///     -> QSL evaluation
    ///     -> Moment
    ///     -> Timeline
    ///     -> Epoch
    /// </summary>
    public sealed class WildsQslExecutor
    {
        private readonly QslEngine _qsl;
        private readonly Core.WildsEngine _wilds;

        public WildsQslExecutor(
            QslEngine qsl,
            Core.WildsEngine wilds)
        {
            _qsl = qsl ??
                throw new ArgumentNullException(
                    nameof(qsl));

            _wilds = wilds ??
                throw new ArgumentNullException(
                    nameof(wilds));
        }

        /// <summary>
        /// Executes one complete QSL request through Wilds.
        ///
        /// Wilds remains responsible for the Pulse,
        /// WorkUnit, terminal Moment, Timeline, and Epoch.
        /// QSL remains responsible only for processing the
        /// supplied QSL source.
        /// </summary>
        public ValueTask<IMoment> ExecuteAsync(
            string source,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(
                    source))
            {
                throw new ArgumentException(
                    "QSL source cannot be empty.",
                    nameof(source));
            }

            return _wilds.ExecuteAsync(
                source,
                ExecuteQslAsync,
                cancellationToken);
        }

        private ValueTask<object?> ExecuteQslAsync(
            object? request,
            CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            var source =
                request as string;

            if (source == null)
            {
                throw new InvalidOperationException(
                    "WildsQslExecutor expected a QSL " +
                    "source string.");
            }

            var result =
                _qsl.Evaluate(
                    source);

            if (!result.Succeeded)
            {
                throw new WildsQslExecutionException(
                    result);
            }

            return new ValueTask<object?>(
                result.Value);
        }
    }
}
