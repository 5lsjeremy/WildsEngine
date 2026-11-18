using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Services;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class WildsServiceContextReadableTests
    {
        [Fact]
        public async Task ServiceExecutionIsIdentifiableTimedAndQueryable()
        {
            TestLog.Section("E7-H1A - SERVICE CONTEXT PROVENANCE");
            var clock = new SystemWildsServiceClock();
            var journal = new WildsServiceExecutionJournal();
            var factory = new WildsServiceContextFactory(clock);
            var service = new EchoService(factory, journal, clock);
            var provenance = new object();
            var pulseId = Guid.NewGuid();
            var result = await service.ExecuteAsync("vectorize-this", provenance, pulseId: pulseId);
            Assert.True(journal.TryGet(result.ExecutionId, out var record));
            TestLog.State($"Service = {record!.ServiceSlug} ({record.ServiceId})");
            TestLog.State($"Execution = {record.Id}");
            TestLog.State($"Pulse correlation = {record.PulseId}");
            TestLog.State($"Outcome = {record.Outcome}");
            TestLog.State($"Started = {record.StartedAt:O}");
            TestLog.State($"Completed = {record.CompletedAt:O}");
            TestLog.State($"Duration = {record.Duration}");
            Assert.Equal(WildsServiceExecutionOutcome.Succeeded, record.Outcome);
            Assert.Same(provenance, record.Provenance);
            Assert.False(service.IsRunning);
            TestLog.Pass("The async service returned its execution identity and the journal preserved queryable temporal provenance.");
        }

        private sealed class EchoService : WildsServiceBase<string, string>
        {
            public EchoService(
                IWildsServiceContextFactory<IWildsServiceContext> contexts,
                IWildsServiceExecutionJournal journal,
                IWildsServiceClock clock)
                : base("readable-echo-service", contexts, journal, clock) { }
            protected override Task<string> ExecuteCoreAsync(
                string request, IWildsServiceContext context, CancellationToken cancellationToken) =>
                Task.FromResult(request + "-complete");
        }
    }
}
