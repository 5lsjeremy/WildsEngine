using System;
using System.Threading;
using System.Threading.Tasks;
using QSL.Core;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Qsl
{
    /// <summary>
    /// Executes QSL requests through the complete
    /// WildsEngine request lifecycle.
    ///
    /// request
    ///     -> Pulse
    ///     -> WorkUnit
    ///     -> QSL preparation
    ///     -> QSL evaluation
    ///     -> Moment
    ///     -> Timeline
    ///     -> Epoch
    ///
    /// Wilds owns the processing and temporal lifecycle.
    /// QSL owns consumer binding, attached state,
    /// language evaluation, diagnostics, and provenance.
    /// </summary>
    public sealed class WildsQslExecutor
    {
        private readonly QslEngine _qsl;

        private readonly Core.WildsEngine _wilds;

        public WildsQslExecutor(
            QslEngine qsl,
            Core.WildsEngine wilds)
        {
            _qsl =
                qsl
                ?? throw new ArgumentNullException(
                    nameof(qsl));

            _wilds =
                wilds
                ?? throw new ArgumentNullException(
                    nameof(wilds));
        }

        /// <summary>
        /// Executes ordinary QSL source through Wilds.
        ///
        /// This overload preserves the existing
        /// source-only integration path.
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
                ExecuteSourceAsync,
                cancellationToken);
        }

        /// <summary>
        /// Executes a complete QSL request containing
        /// source, consumer bindings, and state packages
        /// through Wilds.
        /// </summary>
        public ValueTask<IMoment> ExecuteAsync(
            WildsQslRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

            return _wilds.ExecuteAsync(
                request,
                ExecuteRequestAsync,
                cancellationToken);
        }

        private ValueTask<object?> ExecuteSourceAsync(
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
                    "WildsQslExecutor expected QSL source.");
            }

            return EvaluateAsync(
                source);
        }

        private ValueTask<object?> ExecuteRequestAsync(
            object? request,
            CancellationToken cancellationToken)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            var qslRequest =
                request as WildsQslRequest;

            if (qslRequest == null)
            {
                throw new InvalidOperationException(
                    "WildsQslExecutor expected a " +
                    "WildsQslRequest.");
            }

            for (
                var index = 0;
                index < qslRequest.Consumers.Count;
                index++)
            {
                _qsl.BindConsumer(
                    qslRequest.Consumers[index]);
            }

            for (
                var index = 0;
                index < qslRequest.StatePackages.Count;
                index++)
            {
                _qsl.ReceiveState(
                    qslRequest.StatePackages[index]);
            }

            cancellationToken
                .ThrowIfCancellationRequested();

            return EvaluateAsync(
                qslRequest.Source);
        }

        private ValueTask<object?> EvaluateAsync(
            string source)
        {
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
