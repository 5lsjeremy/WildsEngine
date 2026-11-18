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
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryVectorizationServiceReadableTests
    {
        [Fact]
        public async Task AsyncConsumerServiceTurnsKnowledgeIntoAttributedGeometry()
        {
            TestLog.Section("E7-H1B - ASYNC DISCOVERY VECTORIZATION");

            var clock = new SystemWildsServiceClock();
            var journal = new WildsServiceExecutionJournal();
            var service = new ReadableService(
                new WildsServiceContextFactory(clock),
                journal,
                clock);

            var entity = new TestEntity("soup-gobbo");
            var discovery = new TestDiscovery(
                entity,
                3,
                "Soup Risk",
                "soup-risk-h1b");

            var input = new DiscoveryVectorInput(
                discovery,
                "consumer-authorized-sample");

            var pulseId = Guid.NewGuid();

            var result = await new DiscoveryVectorizerFactory()
                .Create(service, new AbsoluteMetric())
                .VectorizeAsync(
                    input,
                    provenance: "H1B readable",
                    pulseId: pulseId);

            Assert.True(journal.TryGet(result.ServiceExecutionId, out var execution));

            TestLog.State($"Service = {execution!.ServiceSlug}");
            TestLog.State($"Execution = {result.ServiceExecutionId}");
            TestLog.State($"Pulse = {execution.PulseId}");
            TestLog.State($"Discovery resolved = {discovery.IsAvailable}");
            TestLog.State($"Risk = {result.Vector.Components[0].Value:R}");
            TestLog.State($"Trust = {result.Vector.Components[1].Value:R}");
            TestLog.State($"Magnitude = {result.Vector.Magnitude:R}");

            Assert.False(discovery.IsAvailable);
            Assert.Equal(WildsServiceExecutionOutcome.Succeeded, execution.Outcome);
            Assert.Equal(2, result.Vector.Components.Count);
            Assert.Equal("risk", result.Vector.Components[0].Name);
            Assert.Equal(0.80, result.Vector.Components[0].Value, 10);
            Assert.Equal("trust", result.Vector.Components[1].Name);
            Assert.Equal(0.40, result.Vector.Components[1].Value, 10);
            Assert.Equal(1.20, result.Vector.Magnitude, 10);

            TestLog.Pass(
                "The consumer-provided async service produced exact contributions; Discoveries validated and aggregated them into attributable geometry without resolving the source Discovery.");
        }

        private sealed class ReadableService : DiscoveryVectorizationServiceBase
        {
            public ReadableService(
                IWildsServiceContextFactory<IWildsServiceContext> contexts,
                IWildsServiceExecutionJournal journal,
                IWildsServiceClock clock)
                : base("soup-risk-vectorization", contexts, journal, clock)
            {
            }

            protected override Task<DiscoveryVectorizationServiceResult> ExecuteCoreAsync(
                DiscoveryVectorizationRequest request,
                IWildsServiceContext context,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(
                    new DiscoveryVectorizationServiceResult(
                        new List<IDiscoveryVectorContribution>
                        {
                            new DiscoveryVectorContribution(request.Input, "risk", 0.65, 1.0),
                            new DiscoveryVectorContribution(request.Input, "risk", 0.15, 1.0),
                            new DiscoveryVectorContribution(request.Input, "trust", 0.40, 1.0)
                        }));
            }
        }

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
    }
}
