using System;
using System.Collections.Generic;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Vectors;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityDiscoveryGeometryFoundationCertificationTests
    {
        [Fact]
        public void ComponentsPreserveConsumerDimensionsAndSignedValues()
        {
            var component = new DiscoveryVectorComponent(" risk-direction ", -2.5);
            Assert.Equal("risk-direction", component.Name);
            Assert.Equal(-2.5, component.Value);
            Assert.Throws<ArgumentException>(() => new DiscoveryVectorComponent(" ", 1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DiscoveryVectorComponent("risk", double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DiscoveryVectorComponent("risk", double.PositiveInfinity));
        }

        [Fact]
        public void InputPreservesExactDiscoveryAndOpaqueProvenance()
        {
            var entity = new TestEntity("h0-input-owner");
            var discovery = CreateDiscovery(entity, 3);
            var provenance = new object();
            var input = new DiscoveryVectorInput(discovery, provenance);
            Assert.Same(discovery, input.Discovery);
            Assert.Same(provenance, input.Provenance);
        }

        [Fact]
        public void ContributionPreservesExactInputAndFactAndRejectsForeignFact()
        {
            var owner = new TestEntity("h0-owner");
            var foreign = new TestEntity("h0-foreign");
            var discovery = CreateDiscovery(owner, 3);
            var input = new DiscoveryVectorInput(discovery, new object());
            var fact = CreateFact(owner, "support");
            var foreignFact = CreateFact(foreign, "foreign");
            var contribution = new DiscoveryVectorContribution(input, "momentum", -0.5, 0.75, fact);
            Assert.Same(input, contribution.Input);
            Assert.Same(fact, contribution.Fact);
            Assert.Equal("momentum", contribution.Dimension);
            Assert.Equal(-0.5, contribution.Value);
            Assert.Equal(0.75, contribution.Weight);
            Assert.Throws<ArgumentException>(() =>
                new DiscoveryVectorContribution(input, "momentum", 1.0, 1.0, foreignFact));
        }

        [Fact]
        public void ContributionRejectsNegativeAndNonFiniteWeight()
        {
            var input = new DiscoveryVectorInput(CreateDiscovery(new TestEntity("h0-weight"), 1));
            var negative = new DiscoveryVectorContribution(input, "signal", 1.0, -0.01);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DiscoveryVector(Components(("signal", 1.0)),
                    new List<IDiscoveryVectorContribution> { negative },
                    new List<IDiscoveryVectorMotion>(), new SumMagnitudeMetric(), DateTimeOffset.UtcNow));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DiscoveryVectorContribution(input, "signal", 1.0, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DiscoveryVectorContribution(input, "signal", 1.0, double.PositiveInfinity));
        }

        [Fact]
        public void VectorRejectsDuplicateAndUnknownDimensions()
        {
            var input = new DiscoveryVectorInput(CreateDiscovery(new TestEntity("h0-schema"), 1));
            var duplicate = new List<IDiscoveryVectorComponent>
            {
                new DiscoveryVectorComponent("signal", 1.0),
                new DiscoveryVectorComponent("signal", 2.0)
            };
            Assert.Throws<ArgumentException>(() => CreateVector(duplicate));

            var components = Components(("signal", 1.0));
            var unknownContribution = new List<IDiscoveryVectorContribution>
            {
                new DiscoveryVectorContribution(input, "other", 1.0, 1.0)
            };
            Assert.Throws<ArgumentException>(() =>
                new DiscoveryVector(components, unknownContribution,
                    new List<IDiscoveryVectorMotion>(), new SumMagnitudeMetric(), DateTimeOffset.UtcNow));
        }

        [Fact]
        public void VectorRejectsUnknownAndDuplicateMotionDimensions()
        {
            var components = Components(("signal", 1.0));
            Assert.Throws<ArgumentException>(() =>
                new DiscoveryVector(components, new List<IDiscoveryVectorContribution>(),
                    new List<IDiscoveryVectorMotion> { new DiscoveryVectorMotion("other", 1.0, 0.0) },
                    new SumMagnitudeMetric(), DateTimeOffset.UtcNow));
            Assert.Throws<ArgumentException>(() =>
                new DiscoveryVector(components, new List<IDiscoveryVectorContribution>(),
                    new List<IDiscoveryVectorMotion>
                    {
                        new DiscoveryVectorMotion("signal", 1.0, 0.0),
                        new DiscoveryVectorMotion("signal", 2.0, 1.0)
                    }, new SumMagnitudeMetric(), DateTimeOffset.UtcNow));
        }

        [Fact]
        public void MotionAllowsSignedFiniteVelocityAndAcceleration()
        {
            var motion = new DiscoveryVectorMotion("signal", -3.0, 1.25);
            Assert.Equal(-3.0, motion.Velocity);
            Assert.Equal(1.25, motion.Acceleration);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DiscoveryVectorMotion("signal", double.NaN, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DiscoveryVectorMotion("signal", 0.0, double.NegativeInfinity));
        }

        [Fact]
        public void MetricDerivesMagnitudeFromExactVectorPoint()
        {
            var metric = new CapturingMetric(7.25);
            var vector = new DiscoveryVector(Components(("x", 3.0), ("y", 4.0)),
                new List<IDiscoveryVectorContribution>(), new List<IDiscoveryVectorMotion>(),
                metric, DateTimeOffset.UtcNow);
            Assert.Same(vector, metric.Point);
            Assert.Equal(7.25, vector.Magnitude);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DiscoveryVector(Components(("x", 1.0)),
                    new List<IDiscoveryVectorContribution>(), new List<IDiscoveryVectorMotion>(),
                    new CapturingMetric(-1.0), DateTimeOffset.UtcNow));
        }

        [Fact]
        public void VectorSnapshotsCallerCollections()
        {
            var components = Components(("x", 3.0));
            var contributions = new List<IDiscoveryVectorContribution>();
            var motion = new List<IDiscoveryVectorMotion>();
            var vector = new DiscoveryVector(components, contributions, motion,
                new SumMagnitudeMetric(), DateTimeOffset.UtcNow);
            components.Clear();
            components.Add(new DiscoveryVectorComponent("replacement", 99.0));
            contributions.Clear();
            motion.Add(new DiscoveryVectorMotion("x", 1.0, 0.0));
            Assert.Single(vector.Components);
            Assert.Equal("x", vector.Components[0].Name);
            Assert.Empty(vector.Contributions);
            Assert.Empty(vector.Motion);
        }

        [Fact]
        public void ClusterSnapshotsMembersAndValidatesGeometryBounds()
        {
            var vector = CreateVector(Components(("x", 1.0)));
            var members = new List<IDiscoveryVector> { vector };
            var cluster = new TestCluster(members, new TestPoint(Components(("x", 1.0))), 0.8, 0.2, 0.9);
            members.Clear();
            Assert.Single(cluster.Members);
            Assert.Same(vector, cluster.Members[0]);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TestCluster(new List<IDiscoveryVector> { vector }, new TestPoint(Components(("x", 1.0))), 1.01, 0.0, 1.0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TestCluster(new List<IDiscoveryVector> { vector }, new TestPoint(Components(("x", 1.0))), 1.0, -0.01, 1.0));
        }

        [Fact]
        public void TruthRetainsExactSeriesClustersAndDiscoveries()
        {
            var entity = new TestEntity("h0-truth");
            var discovery = CreateDiscovery(entity, 3);
            var input = new DiscoveryVectorInput(discovery, new object());
            var vector = CreateVector(Components(("support", 2.0 / 3.0)));
            var series = new TestSeries("truth-scope", new List<IDiscoveryVectorInput> { input },
                new List<IDiscoveryVector> { vector });
            var cluster = new TestCluster(new List<IDiscoveryVector> { vector },
                new TestPoint(Components(("support", 2.0 / 3.0))), 1.0, 0.0, 1.0);
            var truth = new TestTruth("truth.h0", "H0 Truth", series,
                new List<IDiscoveryCluster> { cluster }, new List<IDiscovery> { discovery }, 0.8, 0.7);
            Assert.Same(series, truth.Series);
            Assert.Same(cluster, truth.Clusters[0]);
            Assert.Same(discovery, truth.Discoveries[0]);
            Assert.Throws<ArgumentException>(() =>
                new TestTruth("truth.foreign", "Foreign", series,
                    new List<IDiscoveryCluster> { cluster },
                    new List<IDiscovery> { CreateDiscovery(new TestEntity("foreign-truth"), 1) }, 0.8, 0.7));
        }

        [Fact]
        public void ConsequenceRetainsExactTruthAndHasNoActionSurface()
        {
            var entity = new TestEntity("h0-consequence");
            var discovery = CreateDiscovery(entity, 1);
            var input = new DiscoveryVectorInput(discovery);
            var vector = CreateVector(Components(("risk", 0.5)));
            var series = new TestSeries("consequence-scope", new List<IDiscoveryVectorInput> { input },
                new List<IDiscoveryVector> { vector });
            var cluster = new TestCluster(new List<IDiscoveryVector> { vector },
                new TestPoint(Components(("risk", 0.5))), 1.0, 0.0, 1.0);
            var truth = new TestTruth("truth.risk", "Risk", series,
                new List<IDiscoveryCluster> { cluster }, new List<IDiscovery> { discovery }, 0.8, 0.7);
            var consequence = new TestConsequence("consequence.risk", "Projected Risk",
                new List<ITruth> { truth }, 0.6, 0.75);
            Assert.Same(truth, consequence.Truths[0]);
            Assert.Null(typeof(IConsequence).GetProperty("Action"));
            Assert.Null(typeof(IConsequence).GetMethod("Execute"));
        }

        [Fact]
        public void PartialDiscoveryCanBeRepresentedWithoutChangingResolution()
        {
            var entity = new TestEntity("h0-partial");
            var discovery = CreateDiscovery(entity, 3);
            var first = CreateFact(entity, "first");
            var second = CreateFact(entity, "second");
            Assert.True(discovery.AddFactItem(first));
            Assert.True(discovery.AddFactItem(second));
            var beforeCount = discovery.CurrentFactCount;
            var beforeResolved = discovery.IsAvailable;
            var input = new DiscoveryVectorInput(discovery, "partial-resolution");
            var vector = new DiscoveryVector(
                Components(("support", 2.0 / 3.0)),
                new List<IDiscoveryVectorContribution>
                {
                    new DiscoveryVectorContribution(input, "support", 1.0, 1.0, first),
                    new DiscoveryVectorContribution(input, "support", 1.0, 1.0, second)
                },
                new List<IDiscoveryVectorMotion> { new DiscoveryVectorMotion("support", 0.25, 0.05) },
                new SumMagnitudeMetric(), DateTimeOffset.UtcNow);
            Assert.Equal(2.0 / 3.0, vector.Components[0].Value, 10);
            Assert.Equal(beforeCount, discovery.CurrentFactCount);
            Assert.Equal(beforeResolved, discovery.IsAvailable);
            Assert.False(discovery.IsAvailable);
            Assert.Equal(2, discovery.Facts.Count);
        }

        private static TestDiscovery CreateDiscovery(IEntity entity, int required) =>
            new TestDiscovery(entity, required, "H0 Discovery", "h0-discovery-" + Guid.NewGuid().ToString("N"));

        private static TestFact CreateFact(IEntity entity, string name) =>
            new TestFact(entity, name, true, 1, new MinimumBelievabilityPolicy(0.80));

        private static List<IDiscoveryVectorComponent> Components(params (string Name, double Value)[] values)
        {
            var result = new List<IDiscoveryVectorComponent>();
            foreach (var value in values)
                result.Add(new DiscoveryVectorComponent(value.Name, value.Value));
            return result;
        }

        private static DiscoveryVector CreateVector(IReadOnlyList<IDiscoveryVectorComponent> components) =>
            new DiscoveryVector(components, new List<IDiscoveryVectorContribution>(),
                new List<IDiscoveryVectorMotion>(), new SumMagnitudeMetric(), DateTimeOffset.UtcNow);

        private sealed class SumMagnitudeMetric : IDiscoveryVectorMetric
        {
            public double GetMagnitude(IDiscoveryVectorPoint point)
            {
                var total = 0.0;
                for (var i = 0; i < point.Components.Count; i++) total += Math.Abs(point.Components[i].Value);
                return total;
            }
        }

        private sealed class CapturingMetric : IDiscoveryVectorMetric
        {
            private readonly double _value;
            public CapturingMetric(double value) => _value = value;
            public IDiscoveryVectorPoint? Point { get; private set; }
            public double GetMagnitude(IDiscoveryVectorPoint point) { Point = point; return _value; }
        }

        private sealed class TestPoint : IDiscoveryVectorPoint
        {
            public TestPoint(IReadOnlyList<IDiscoveryVectorComponent> components) => Components = components;
            public IReadOnlyList<IDiscoveryVectorComponent> Components { get; }
            public int Dimension => Components.Count;
        }

        private sealed class TestCluster : DiscoveryClusterBase
        {
            public TestCluster(IReadOnlyList<IDiscoveryVector> members, IDiscoveryVectorPoint centroid,
                double density, double dispersion, double coherence)
                : base(members, centroid, density, dispersion, coherence) { }
        }

        private sealed class TestSeries : IDiscoveryVectorSeries
        {
            public TestSeries(string scope, IReadOnlyList<IDiscoveryVectorInput> inputs,
                IReadOnlyList<IDiscoveryVector> vectors)
            { Scope = scope; Inputs = inputs; Vectors = vectors; }
            public string Scope { get; }
            public IReadOnlyList<IDiscoveryVectorInput> Inputs { get; }
            public IReadOnlyList<IDiscoveryVector> Vectors { get; }
            public IDiscoveryVector? Current => Vectors.Count == 0 ? null : Vectors[Vectors.Count - 1];
            public IDiscoveryVector? Previous => Vectors.Count < 2 ? null : Vectors[Vectors.Count - 2];
        }

        private sealed class TestTruth : TruthBase
        {
            public TestTruth(string slug, string name, IDiscoveryVectorSeries series,
                IReadOnlyList<IDiscoveryCluster> clusters, IReadOnlyList<IDiscovery> discoveries,
                double stability, double confidence)
                : base(slug, name, null, series, clusters, discoveries, stability, confidence, DateTimeOffset.UtcNow) { }
        }

        private sealed class TestConsequence : ConsequenceBase
        {
            public TestConsequence(string slug, string name, IReadOnlyList<ITruth> truths,
                double magnitude, double confidence)
                : base(slug, name, null, truths, magnitude, confidence, DateTimeOffset.UtcNow) { }
        }
    }
}
