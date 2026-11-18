using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Services
{
    public abstract class WildsServiceBase<TRequest, TResult> :
        IdentifierBase,
        IWildsService<TRequest, TResult>
        where TRequest : class
        where TResult : class
    {
        private readonly IWildsServiceContextFactory<IWildsServiceContext> _contexts;
        private readonly IWildsServiceExecutionJournal _journal;
        private readonly IWildsServiceClock _clock;
        private int _activeExecutions;

        protected WildsServiceBase(
            string slug,
            IWildsServiceContextFactory<IWildsServiceContext> contexts,
            IWildsServiceExecutionJournal journal,
            IWildsServiceClock clock)
            : base(slug)
        {
            _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));
            _journal = journal ?? throw new ArgumentNullException(nameof(journal));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        protected WildsServiceBase(
            Guid id,
            string slug,
            IWildsServiceContextFactory<IWildsServiceContext> contexts,
            IWildsServiceExecutionJournal journal,
            IWildsServiceClock clock)
            : base(id, slug)
        {
            _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));
            _journal = journal ?? throw new ArgumentNullException(nameof(journal));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public bool IsRunning => Volatile.Read(ref _activeExecutions) > 0;

        public async Task<IWildsServiceResult<TResult>> ExecuteAsync(
            TRequest request,
            object? provenance = null,
            Guid? parentExecutionId = null,
            Guid? pulseId = null,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var context = _contexts.Create(this, provenance, parentExecutionId, pulseId, cancellationToken);
            _journal.Record(new WildsServiceExecutionRecord(
                context, null, null, null, WildsServiceExecutionOutcome.Requested));

            Interlocked.Increment(ref _activeExecutions);
            var startedAt = _clock.GetUtcNow();
            var startedStamp = _clock.GetTimestamp();
            _journal.Record(new WildsServiceExecutionRecord(
                context, startedAt, null, null, WildsServiceExecutionOutcome.Running));

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var value = await ExecuteCoreAsync(request, context, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                if (value == null) throw new InvalidOperationException("A Wilds service cannot return null.");
                var completedAt = _clock.GetUtcNow();
                var duration = _clock.GetElapsedTime(startedStamp, _clock.GetTimestamp());
                _journal.Record(new WildsServiceExecutionRecord(
                    context, startedAt, completedAt, duration, WildsServiceExecutionOutcome.Succeeded));
                return new WildsServiceResult<TResult>(context.Id, value);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                var completedAt = _clock.GetUtcNow();
                var duration = _clock.GetElapsedTime(startedStamp, _clock.GetTimestamp());
                _journal.Record(new WildsServiceExecutionRecord(
                    context, startedAt, completedAt, duration, WildsServiceExecutionOutcome.Cancelled));
                throw;
            }
            catch (Exception exception)
            {
                var completedAt = _clock.GetUtcNow();
                var duration = _clock.GetElapsedTime(startedStamp, _clock.GetTimestamp());
                _journal.Record(new WildsServiceExecutionRecord(
                    context, startedAt, completedAt, duration, WildsServiceExecutionOutcome.Failed,
                    exception.GetType().FullName, exception.Message));
                throw;
            }
            finally
            {
                Interlocked.Decrement(ref _activeExecutions);
            }
        }

        protected abstract Task<TResult> ExecuteCoreAsync(
            TRequest request,
            IWildsServiceContext context,
            CancellationToken cancellationToken);
    }
}
