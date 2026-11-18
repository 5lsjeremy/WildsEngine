using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Services;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class WildsServiceContextCertificationTests
    {
        [Fact]
        public async Task SuccessProducesQueryableExecutionProvenance()
        {
            var fixture = new Fixture();
            var provenance = new object();
            var pulseId = Guid.NewGuid();
            var result = await fixture.Service.ExecuteAsync("request", provenance, null, pulseId);
            Assert.Equal("request-result", result.Value);
            Assert.True(fixture.Journal.TryGet(result.ExecutionId, out var record));
            Assert.NotNull(record);
            Assert.Equal(WildsServiceExecutionOutcome.Succeeded, record!.Outcome);
            Assert.Equal(fixture.Service.Id, record.ServiceId);
            Assert.Equal(fixture.Service.Slug, record.ServiceSlug);
            Assert.Equal(pulseId, record.PulseId);
            Assert.Same(provenance, record.Provenance);
            Assert.NotNull(record.StartedAt);
            Assert.NotNull(record.CompletedAt);
            Assert.NotNull(record.Duration);
            Assert.False(fixture.Service.IsRunning);
        }

        [Fact]
        public async Task IsRunningRemainsTrueUntilEveryConcurrentExecutionCompletes()
        {
            var fixture = new Fixture(block: true);
            var first = fixture.Service.ExecuteAsync("first");
            var second = fixture.Service.ExecuteAsync("second");
            await fixture.Service.Entered.Task;
            Assert.True(fixture.Service.IsRunning);
            fixture.Service.ReleaseOne();
            await Task.WhenAny(first, second);
            Assert.True(fixture.Service.IsRunning);
            fixture.Service.ReleaseOne();
            await Task.WhenAll(first, second);
            Assert.False(fixture.Service.IsRunning);
        }

        [Fact]
        public async Task CancellationIsRecordedAndPropagated()
        {
            var fixture = new Fixture(block: true);
            using var source = new CancellationTokenSource();
            var task = fixture.Service.ExecuteAsync("cancel", cancellationToken: source.Token);
            await fixture.Service.Entered.Task;
            source.Cancel();
            fixture.Service.ReleaseOne();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
            var records = fixture.Journal.GetByService(fixture.Service.Id);
            Assert.Single(records);
            Assert.Equal(WildsServiceExecutionOutcome.Cancelled, records[0].Outcome);
            Assert.False(fixture.Service.IsRunning);
        }

        [Fact]
        public async Task FailureIsRecordedWithoutReplacingOriginalException()
        {
            var fixture = new Fixture(fail: true);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.ExecuteAsync("fail"));
            Assert.Equal("controlled failure", exception.Message);
            var records = fixture.Journal.GetByService(fixture.Service.Id);
            Assert.Single(records);
            Assert.Equal(WildsServiceExecutionOutcome.Failed, records[0].Outcome);
            Assert.Contains("InvalidOperationException", records[0].FailureType);
            Assert.False(fixture.Service.IsRunning);
        }

        [Fact]
        public async Task ParentExecutionAndPulseArePreserved()
        {
            var fixture = new Fixture();
            var parent = Guid.NewGuid();
            var pulse = Guid.NewGuid();
            var result = await fixture.Service.ExecuteAsync("child", parentExecutionId: parent, pulseId: pulse);
            Assert.True(fixture.Journal.TryGet(result.ExecutionId, out var record));
            Assert.Equal(parent, record!.ParentExecutionId);
            Assert.Equal(pulse, record.PulseId);
            Assert.Single(fixture.Journal.GetByPulse(pulse));
        }

        [Fact]
        public void CatalogRejectsDuplicateIdentityAndSlug()
        {
            var fixture = new Fixture();
            var catalog = new WildsServiceCatalog();
            catalog.Register(fixture.Service);
            Assert.True(catalog.TryGet(fixture.Service.Id, out var byId));
            Assert.Same(fixture.Service, byId);
            Assert.True(catalog.TryGet(fixture.Service.Slug, out var bySlug));
            Assert.Same(fixture.Service, bySlug);
            Assert.Throws<InvalidOperationException>(() => catalog.Register(fixture.Service));
        }

        private sealed class Fixture
        {
            public Fixture(bool block = false, bool fail = false)
            {
                Clock = new SystemWildsServiceClock();
                Journal = new WildsServiceExecutionJournal();
                var concreteFactory = new WildsServiceContextFactory(Clock);
                Service = new TestService(concreteFactory, Journal, Clock, block, fail);
            }
            public SystemWildsServiceClock Clock { get; }
            public WildsServiceExecutionJournal Journal { get; }
            public TestService Service { get; }
        }

        private sealed class TestService : WildsServiceBase<string, string>
        {
            private readonly bool _block;
            private readonly bool _fail;
            private readonly SemaphoreSlim _release = new SemaphoreSlim(0);
            private int _entered;
            public TestService(
                IWildsServiceContextFactory<IWildsServiceContext> contexts,
                IWildsServiceExecutionJournal journal,
                IWildsServiceClock clock,
                bool block,
                bool fail)
                : base("h1a-test-service", contexts, journal, clock)
            { _block = block; _fail = fail; }
            public TaskCompletionSource<bool> Entered { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public void ReleaseOne() => _release.Release();
            protected override async Task<string> ExecuteCoreAsync(
                string request, IWildsServiceContext context, CancellationToken cancellationToken)
            {
                if (Interlocked.Increment(ref _entered) >= 2 || !_block) Entered.TrySetResult(true);
                else Entered.TrySetResult(true);
                if (_block) await _release.WaitAsync(cancellationToken);
                if (_fail) throw new InvalidOperationException("controlled failure");
                return request + "-result";
            }
        }
    }
}
