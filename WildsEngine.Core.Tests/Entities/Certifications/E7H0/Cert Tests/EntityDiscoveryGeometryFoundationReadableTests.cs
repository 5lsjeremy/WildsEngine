using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Evidences;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Vectors;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityDiscoveryGeometryFoundationReadableTests
    {
        [Fact]
        public void TwoOfThreeProvenFactsCreateGeometryWithoutCreatingResolution()
        {
            TestLog.Section("E7-H0 - PARTIAL DISCOVERY GEOMETRY");

            var entity =
                new TestEntity("soup-gobbo");

            var discovery =
                new TestDiscovery(
                    entity,
                    3,
                    "Soup Risk",
                    "soup-risk");

            var first =
                CreateProvenFact(
                    entity,
                    "FirstWarning",
                    true,
                    "first-warning");

            var second =
                CreateProvenFact(
                    entity,
                    "SecondWarning",
                    true,
                    "second-warning");

            Assert.True(discovery.AddFactItem(first));
            Assert.True(discovery.AddFactItem(second));

            Assert.Equal(3, discovery.RequiredFactCount);
            Assert.Equal(2, discovery.CurrentFactCount);
            Assert.Equal(2.0 / 3.0, discovery.FactPercentage, 10);
            Assert.False(discovery.IsAvailable);

            var input =
                new DiscoveryVectorInput(
                    discovery,
                    "soup-line-sample");

            var vector =
                new DiscoveryVector(
                    new List<IDiscoveryVectorComponent>
                    {
                        new DiscoveryVectorComponent(
                            "consumption-risk",
                            discovery.FactPercentage)
                    },
                    new List<IDiscoveryVectorContribution>
                    {
                        new DiscoveryVectorContribution(
                            input,
                            "consumption-risk",
                            1.0,
                            1.0,
                            first),
                        new DiscoveryVectorContribution(
                            input,
                            "consumption-risk",
                            1.0,
                            1.0,
                            second)
                    },
                    new List<IDiscoveryVectorMotion>
                    {
                        new DiscoveryVectorMotion(
                            "consumption-risk",
                            0.25,
                            0.05)
                    },
                    new AbsoluteMagnitudeMetric(),
                    DateTimeOffset.UtcNow);

            TestLog.State($"Required Facts = {discovery.RequiredFactCount}");
            TestLog.State($"Current contributing Facts = {discovery.CurrentFactCount}");
            TestLog.State($"Discovery percentage = {discovery.FactPercentage:R}");
            TestLog.State($"Discovery resolved = {discovery.IsAvailable}");
            TestLog.State($"Risk position = {vector.Components[0].Value:R}");
            TestLog.State($"Risk velocity = {vector.Motion[0].Velocity:R}");
            TestLog.State($"Risk acceleration = {vector.Motion[0].Acceleration:R}");

            Assert.Equal(2, discovery.Facts.Count);
            Assert.Equal(2, discovery.CurrentFactCount);
            Assert.False(discovery.IsAvailable);
            Assert.Equal(discovery.FactPercentage, vector.Components[0].Value);
            Assert.Same(discovery, vector.Contributions[0].Input.Discovery);
            Assert.Same(first, vector.Contributions[0].Fact);
            Assert.Same(second, vector.Contributions[1].Fact);

            TestLog.Pass(
                "Two proven contributing Facts out of three required produced attributable geometry and momentum without falsely resolving the Discovery.");
        }

        [Fact]
        public void SamePositionCanCarryDifferentMotion()
        {
            TestLog.Section("E7-H0 - POSITION IS NOT TRAJECTORY");

            var components =
                new List<IDiscoveryVectorComponent>
                {
                    new DiscoveryVectorComponent(
                        "narrative-support",
                        0.67)
                };

            var rising =
                new DiscoveryVector(
                    components,
                    new List<IDiscoveryVectorContribution>(),
                    new List<IDiscoveryVectorMotion>
                    {
                        new DiscoveryVectorMotion(
                            "narrative-support",
                            0.20,
                            0.04)
                    },
                    new AbsoluteMagnitudeMetric(),
                    DateTimeOffset.UtcNow);

            var falling =
                new DiscoveryVector(
                    components,
                    new List<IDiscoveryVectorContribution>(),
                    new List<IDiscoveryVectorMotion>
                    {
                        new DiscoveryVectorMotion(
                            "narrative-support",
                            -0.20,
                            -0.04)
                    },
                    new AbsoluteMagnitudeMetric(),
                    DateTimeOffset.UtcNow.AddSeconds(1));

            TestLog.State($"Shared position = {rising.Components[0].Value:R}");
            TestLog.State($"Rising velocity = {rising.Motion[0].Velocity:R}");
            TestLog.State($"Falling velocity = {falling.Motion[0].Velocity:R}");

            Assert.Equal(
                rising.Components[0].Value,
                falling.Components[0].Value);

            Assert.NotEqual(
                rising.Motion[0].Velocity,
                falling.Motion[0].Velocity);

            TestLog.Pass(
                "Equal positions retained different trajectories.");
        }

        private static TestFact CreateProvenFact(
            IEntity entity,
            string name,
            object value,
            string externalId)
        {
            var fact =
                new TestFact(
                    entity,
                    name,
                    value,
                    1,
                    new MinimumBelievabilityPolicy(0.80),
                    externalId);

            var evidence =
                new DiscoveryEvidence(
                    entity,
                    new Dictionary<string, object?>
                    {
                        { "FactName", name }
                    },
                    value,
                    1.0,
                    1.0,
                    new List<IBaseObject>(),
                    null,
                    externalId + "-evidence",
                    name + " evidence");

            var proof =
                new TestProof(
                    0.90,
                    true,
                    "Controlled E7-H0 proof.");

            Assert.True(
                evidence.ApplyProof(
                    proof.Validate(evidence)));

            Assert.True(
                fact.AddEvidenceItem(evidence));

            Assert.True(fact.IsProven);

            return fact;
        }

        private sealed class AbsoluteMagnitudeMetric :
            IDiscoveryVectorMetric
        {
            public double GetMagnitude(
                IDiscoveryVectorPoint point)
            {
                var total = 0.0;

                for (var index = 0;
                     index < point.Components.Count;
                     index++)
                {
                    total +=
                        Math.Abs(
                            point.Components[index].Value);
                }

                return total;
            }
        }
    }
}
