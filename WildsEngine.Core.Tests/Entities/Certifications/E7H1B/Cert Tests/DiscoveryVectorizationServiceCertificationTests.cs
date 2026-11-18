using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Vectorization;
using WildsEngine.Discoveries.Services;
using WildsEngine.Discoveries.Vectorization;
using WildsEngine.Discoveries.Vectors;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryVectorizationServiceCertificationTests
    {
        [Fact]
        public async Task FactoryCreatedVectorizerExecutesServiceAndReturnsExecutionProvenance()
        {
            var fixture = new Fixture();
            var entity = new TestEntity("h1b-owner");
            var discovery = CreateDiscovery(entity);
            var input = new DiscoveryVectorInput(discovery, new object());

            fixture.Service.Handler = request =>
                new DiscoveryVectorizationServiceResult(
                    new List<IDiscoveryVectorContribution>
                    {
                        new DiscoveryVectorContribution(request.Input, "trust", 0.5, 2.0),
                        new DiscoveryVectorContribution(request.Input, "trust", -0.25, 1.0),
                        new DiscoveryVectorContribution(request.Input, "risk", 0.75, 1.0)
                    });

            var vectorizer = new DiscoveryVectorizerFactory()
                .Create(fixture.Service, new AbsoluteMetric());

            var result = await vectorizer.VectorizeAsync(
                input,
                pulseId: Guid.NewGuid());

            Assert.NotEqual(Guid.Empty, result.ServiceExecutionId);
            Assert.True(fixture.Journal.TryGet(result.ServiceExecutionId, out var execution));
            Assert.Equal(WildsServiceExecutionOutcome.Succeeded, execution!.Outcome);
            Assert.Equal(2, result.Vector.Components.Count);
            Assert.Equal("risk", result.Vector.Components[0].Name);
            Assert.Equal(0.75, result.Vector.Components[0].Value);
            Assert.Equal("trust", result.Vector.Components[1].Name);
            Assert.Equal(0.75, result.Vector.Components[1].Value);
            Assert.Equal(3, result.Vector.Contributions.Count);
            Assert.Equal(1, fixture.Service.CallCount);
        }

        [Fact]
        public async Task ContributionOrderDoesNotChangeAggregatedGeometry()
        {
            var fixture = new Fixture();
            var input = new DiscoveryVectorInput(CreateDiscovery(new TestEntity("order")));
            var first = new DiscoveryVectorContribution(input, "signal", 2.0, 0.5);
            var second = new DiscoveryVectorContribution(input, "signal", -1.0, 0.25);

            fixture.Service.Handler = _ =>
                new DiscoveryVectorizationServiceResult(
                    new List<IDiscoveryVectorContribution> { first, second });

            var one = await new DiscoveryVectorizerFactory()
                .Create(fixture.Service, new AbsoluteMetric())
                .VectorizeAsync(input);

            fixture.Service.Handler = _ =>
                new DiscoveryVectorizationServiceResult(
                    new List<IDiscoveryVectorContribution> { second, first });

            var two = await new DiscoveryVectorizerFactory()
                .Create(fixture.Service, new AbsoluteMetric())
                .VectorizeAsync(input);

            Assert.Equal(one.Vector.Components[0].Value, two.Vector.Components[0].Value);
        }

        [Fact]
        public async Task ForeignInputContributionFailsAfterServiceExecutionIsJournaled()
        {
            var fixture = new Fixture();
            var requested = new DiscoveryVectorInput(CreateDiscovery(new TestEntity("requested")));
            var foreign = new DiscoveryVectorInput(CreateDiscovery(new TestEntity("foreign")));

            fixture.Service.Handler = _ =>
                new DiscoveryVectorizationServiceResult(
                    new List<IDiscoveryVectorContribution>
                    {
                        new DiscoveryVectorContribution(foreign, "signal", 1.0, 1.0)
                    });

            var vectorizer = new DiscoveryVectorizerFactory()
                .Create(fixture.Service, new AbsoluteMetric());

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => vectorizer.VectorizeAsync(requested));

            var records = fixture.Journal.GetByService(fixture.Service.Id);
            Assert.Single(records);
            Assert.Equal(WildsServiceExecutionOutcome.Succeeded, records[0].Outcome);
        }

        [Fact]
        public async Task CancellationPropagatesThroughVectorizerAndServiceJournal()
        {
            var fixture = new Fixture(block: true);
            var input = new DiscoveryVectorInput(CreateDiscovery(new TestEntity("cancel")));
            var vectorizer = new DiscoveryVectorizerFactory()
                .Create(fixture.Service, new AbsoluteMetric());

            using var source = new CancellationTokenSource();
            var task = vectorizer.VectorizeAsync(input, cancellationToken: source.Token);

            await fixture.Service.Entered.Task;
            source.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);

            var records = fixture.Journal.GetByService(fixture.Service.Id);
            Assert.Single(records);
            Assert.Equal(WildsServiceExecutionOutcome.Cancelled, records[0].Outcome);
            Assert.False(fixture.Service.IsRunning);
        }

        [Fact]
        public void FactoryRejectsMissingDependencies()
        {
            var fixture = new Fixture();
            var factory = new DiscoveryVectorizerFactory();

            Assert.Throws<ArgumentNullException>(
                () => factory.Create(null!, new AbsoluteMetric()));

            Assert.Throws<ArgumentNullException>(
                () => factory.Create(fixture.Service, null!));
        }

        [Fact]
        public async Task VectorizationDoesNotResolveOrMutateSourceDiscovery()
        {
            var fixture = new Fixture();
            var entity = new TestEntity("partial");
            var discovery = CreateDiscovery(entity);
            var input = new DiscoveryVectorInput(discovery);

            fixture.Service.Handler = request =>
                new DiscoveryVectorizationServiceResult(
                    new List<IDiscoveryVectorContribution>
                    {
                        new DiscoveryVectorContribution(
                            request.Input,
                            "support",
                            2.0 / 3.0,
                            1.0)
                    });

            var before = discovery.CurrentFactCount;

            var result = await new DiscoveryVectorizerFactory()
                .Create(fixture.Service, new AbsoluteMetric())
                .VectorizeAsync(input);

            Assert.Equal(before, discovery.CurrentFactCount);
            Assert.False(discovery.IsAvailable);
            Assert.Equal(2.0 / 3.0, result.Vector.Components[0].Value, 10);
        }

        private static TestDiscovery CreateDiscovery(IEntity entity) =>
            new TestDiscovery(
                entity,
                3,
                "H1B Discovery",
                Guid.NewGuid().ToString("N"));

        private sealed class AbsoluteMetric : IDiscoveryVectorMetric
        {
            public double GetMagnitude(IDiscoveryVectorPoint point)
            {
                var total = 0.0;
                for (var index = 0; index < point.Components.Count; index++)
                    total += Math.Abs(point.Components[index].Value);
                return total;
            }
        }

        private sealed class Fixture
        {
            public Fixture(bool block = false)
            {
                var clock = new SystemWildsServiceClock();
                Journal = new WildsServiceExecutionJournal();
                Service = new TestVectorizationService(
                    new WildsServiceContextFactory(clock),
                    Journal,
                    clock,
                    block);
            }

            public WildsServiceExecutionJournal Journal { get; }
            public TestVectorizationService Service { get; }
        }

        private sealed class TestVectorizationService : DiscoveryVectorizationServiceBase
        {
            private readonly bool _block;
            private readonly TaskCompletionSource<bool> _release =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            public TestVectorizationService(
                IWildsServiceContextFactory<IWildsServiceContext> contexts,
                IWildsServiceExecutionJournal journal,
                IWildsServiceClock clock,
                bool block)
                : base("h1b-vectorization", contexts, journal, clock)
            {
                _block = block;
            }

            public Func<DiscoveryVectorizationRequest, DiscoveryVectorizationServiceResult>? Handler { get; set; }
            public int CallCount { get; private set; }
            public TaskCompletionSource<bool> Entered { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            protected override async Task<DiscoveryVectorizationServiceResult> ExecuteCoreAsync(
                DiscoveryVectorizationRequest request,
                IWildsServiceContext context,
                CancellationToken cancellationToken)
            {
                CallCount++;
                Entered.TrySetResult(true);

                if (_block)
                {
                    var cancellation = new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);

                    using (cancellationToken.Register(() => cancellation.TrySetCanceled(cancellationToken)))
                    {
                        await Task.WhenAny(_release.Task, cancellation.Task);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                return Handler?.Invoke(request) ??
                    new DiscoveryVectorizationServiceResult(
                        new List<IDiscoveryVectorContribution>
                        {
                            new DiscoveryVectorContribution(request.Input, "default", 1.0, 1.0)
                        });
            }
        }
    }
}
