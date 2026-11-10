using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Emission;
using WildsEngine.Core.Neighbors.Propagation;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;
using Xunit.Abstractions;

namespace WildsEngine.Core.Tests.Emission
{
    /// <summary>
    /// Gate 5 certification for first-class emission-vector
    /// carriers.
    ///
    /// This gate proves continuity across:
    ///
    /// resolved emissions
    ///     -> immutable resolved vector
    ///     -> immutable normalized vector
    ///     -> existing Neighbor propagation
    ///     -> immutable propagated vector
    ///
    /// The complete route preserves exact:
    ///
    /// source emitter identity
    /// vector identity
    /// emission identity and ordering
    /// consumer-owned property names
    /// resolved magnitudes
    /// normalized magnitudes
    /// Neighbor relationship identity
    /// frozen propagation distance
    /// received magnitudes
    ///
    /// Consumer interpretation, eligibility, and receiver
    /// mutation remain outside this gate.
    /// </summary>
    public sealed class EmitterGate5VectorCarrierTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public EmitterGate5VectorCarrierTests(
            ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper =
                testOutputHelper;
        }

        [Fact]
        public void Vector_carriers_preserve_complete_gate5_provenance()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 5 - VECTOR CARRIER PROVENANCE");

            TestLog.Test(
                "Complete resolved, normalized, propagated, source, and relationship provenance survives the vector route.");

            var component =
                new TestComponent(
                    "gate5.carrier.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var receiver =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate5.carrier.receiver");

            Assert.True(
                emitter.Neighbors.Add(
                    receiver,
                    "conversation",
                    2.0));

            Assert.True(
                emitter.Neighbors.TryGet(
                    receiver.Id,
                    "conversation",
                    out var relationship));

            Assert.NotNull(
                relationship);

            var resolvedComponents =
                new IResolvedEmission[]
                {
                    new ResolvedEmission(
                        emitter,
                        "panic",
                        8.4),

                    new ResolvedEmission(
                        emitter,
                        "urgency",
                        5.7),

                    new ResolvedEmission(
                        emitter,
                        "confusion",
                        2.1),

                    new ResolvedEmission(
                        emitter,
                        "hesitation",
                        1.3),

                    new ResolvedEmission(
                        emitter,
                        "trust",
                        0.6)
                };

            var resolvedVector =
                new ResolvedEmissionVector(
                    resolvedComponents);

            var propagation =
                new EmissionVectorPropagation(
                    new ProportionalEmissionNormalization(),
                    new ExponentialDistancePropagation(
                        0.25));

            TestLog.Task(
                "Normalizing and propagating the resolved vector.");

            var propagatedVector =
                propagation.Propagate(
                    resolvedVector,
                    relationship!);

            var normalizedVector =
                propagatedVector.Source;

            var expectedAttenuation =
                Math.Exp(
                    -0.25 *
                    relationship.Distance);

            TestLog.State(
                $"Emitter Id = {emitter.Id}");

            TestLog.State(
                $"Resolved vector exact source = " +
                $"{ReferenceEquals(emitter, resolvedVector.Source)}");

            TestLog.State(
                $"Resolved vector dimensions = " +
                $"{resolvedVector.Dimension}");

            TestLog.State(
                $"Resolved vector total = " +
                $"{resolvedVector.ResolvedTotal:R}");

            TestLog.State(
                $"Normalized vector exact source = " +
                $"{ReferenceEquals(resolvedVector, normalizedVector.Source)}");

            TestLog.State(
                $"Normalized vector dimensions = " +
                $"{normalizedVector.Dimension}");

            TestLog.State(
                $"Normalized vector total = " +
                $"{normalizedVector.NormalizedTotal:R}");

            TestLog.State(
                $"Propagated vector exact source = " +
                $"{ReferenceEquals(normalizedVector, propagatedVector.Source)}");

            TestLog.State(
                $"Propagated vector dimensions = " +
                $"{propagatedVector.Dimension}");

            TestLog.State(
                $"Relationship membership = " +
                $"{propagatedVector.Relationship.Membership}");

            TestLog.State(
                $"Live relationship distance = " +
                $"{propagatedVector.Relationship.Distance:R}");

            TestLog.State(
                $"Frozen propagation distance = " +
                $"{propagatedVector.PropagationDistance:R}");

            TestLog.State(
                $"Expected attenuation = " +
                $"{expectedAttenuation:R}");

            TestLog.State(
                $"Received vector total = " +
                $"{propagatedVector.ReceivedTotal:R}");

            Assert.Same(
                emitter,
                resolvedVector.Source);

            Assert.Same(
                resolvedVector,
                normalizedVector.Source);

            Assert.Same(
                normalizedVector,
                propagatedVector.Source);

            Assert.Same(
                relationship,
                propagatedVector.Relationship);

            Assert.Equal(
                relationship.Distance,
                propagatedVector.PropagationDistance,
                12);

            Assert.Equal(
                resolvedVector.Dimension,
                normalizedVector.Dimension);

            Assert.Equal(
                resolvedVector.Dimension,
                propagatedVector.Dimension);

            Assert.Equal(
                18.1,
                resolvedVector.ResolvedTotal,
                12);

            Assert.Equal(
                1.0,
                normalizedVector.NormalizedTotal,
                12);

            Assert.Equal(
                expectedAttenuation,
                propagatedVector.ReceivedTotal,
                12);

            for (var index = 0;
                 index < resolvedVector.Dimension;
                 index++)
            {
                var resolved =
                    resolvedVector.Emissions[index];

                var normalized =
                    normalizedVector.Emissions[index];

                var propagated =
                    propagatedVector.Emissions[index];

                var expectedNormalized =
                    resolved.Value /
                    resolvedVector.ResolvedTotal;

                var expectedReceived =
                    expectedNormalized *
                    expectedAttenuation;

                TestLog.State(
                    $"{resolved.Name} | " +
                    $"resolved = {resolved.Value:R} | " +
                    $"normalized = {normalized.NormalizedValue:R} | " +
                    $"received = {propagated.Value:R} | " +
                    $"distance = {propagated.PropagationDistance:R}");

                Assert.Same(
                    resolved,
                    normalized.Emission);

                Assert.Same(
                    resolved,
                    propagated.Emission);

                Assert.Same(
                    relationship,
                    propagated.Relationship);

                Assert.Equal(
                    propagatedVector.PropagationDistance,
                    propagated.PropagationDistance,
                    12);

                Assert.Equal(
                    resolved.Value,
                    normalized.ResolvedValue,
                    12);

                Assert.Equal(
                    resolved.Value,
                    propagated.ResolvedValue,
                    12);

                Assert.True(
                    propagated.NormalizedValue.HasValue);

                Assert.Equal(
                    expectedNormalized,
                    normalized.NormalizedValue,
                    12);

                Assert.Equal(
                    normalized.NormalizedValue,
                    propagated.NormalizedValue!.Value,
                    12);

                Assert.Equal(
                    expectedReceived,
                    propagated.Value,
                    12);
            }

            TestLog.Pass(
                "Exact emitter, vector, component, relationship, and value provenance survived Gate 5.");

            TestLog.Pass(
                "Resolved, normalized, and received magnitudes remained independently observable.");

            TestLog.Pass(
                "Frozen propagation distance remained available beside the exact relationship.");

            PublishLatestReport();

            WriteFinished(
                "VECTOR CARRIER PROVENANCE");
        }

        [Fact]
        public void Propagation_distance_remains_stable_after_relationship_changes()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 5 - DISTANCE SNAPSHOT");

            TestLog.Test(
                "Propagated results preserve the distance used during evaluation after live topology changes.");

            var component =
                new TestComponent(
                    "gate5.distance.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var receiver =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate5.distance.receiver");

            Assert.True(
                emitter.Neighbors.Add(
                    receiver,
                    "conversation",
                    2.0));

            Assert.True(
                emitter.Neighbors.TryGet(
                    receiver.Id,
                    "conversation",
                    out var relationship));

            Assert.NotNull(
                relationship);

            var resolvedVector =
                new ResolvedEmissionVector(
                    new IResolvedEmission[]
                    {
                        new ResolvedEmission(
                            emitter,
                            "approval",
                            0.8),

                        new ResolvedEmission(
                            emitter,
                            "hesitation",
                            0.2)
                    });

            var propagation =
                new EmissionVectorPropagation(
                    new ProportionalEmissionNormalization(),
                    new ExponentialDistancePropagation(
                        0.5));

            var result =
                propagation.Propagate(
                    resolvedVector,
                    relationship!);

            var frozenDistance =
                result.PropagationDistance;

            var frozenValues =
                new double[
                    result.Dimension];

            for (var index = 0;
                 index < result.Dimension;
                 index++)
            {
                frozenValues[index] =
                    result.Emissions[index].Value;
            }

            TestLog.State(
                $"Propagation distance before topology change = " +
                $"{frozenDistance:R}");

            TestLog.Task(
                "Replacing the live relationship with a different distance.");

            Assert.True(
                emitter.Neighbors.Remove(
                    receiver.Id,
                    "conversation"));

            Assert.True(
                emitter.Neighbors.Add(
                    receiver,
                    "conversation",
                    5.0));

            Assert.True(
                emitter.Neighbors.TryGet(
                    receiver.Id,
                    "conversation",
                    out var replacement));

            Assert.NotNull(
                replacement);

            TestLog.State(
                $"Replacement relationship distance = " +
                $"{replacement!.Distance:R}");

            TestLog.State(
                $"Carrier propagation distance after topology change = " +
                $"{result.PropagationDistance:R}");

            Assert.Equal(
                2.0,
                frozenDistance,
                12);

            Assert.Equal(
                frozenDistance,
                result.PropagationDistance,
                12);

            Assert.NotSame(
                result.Relationship,
                replacement);

            for (var index = 0;
                 index < result.Dimension;
                 index++)
            {
                Assert.Equal(
                    frozenDistance,
                    result.Emissions[index].PropagationDistance,
                    12);

                Assert.Equal(
                    frozenValues[index],
                    result.Emissions[index].Value,
                    12);
            }

            TestLog.Pass(
                "Historical propagation distance and received values survived live topology replacement.");

            WriteFinished(
                "DISTANCE SNAPSHOT");
        }

        [Fact]
        public void Resolved_vector_rejects_duplicate_names()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 5 - UNIQUE VECTOR CHANNELS");

            TestLog.Test(
                "Resolved vector rejects duplicate consumer-owned emission names.");

            var component =
                new TestComponent(
                    "gate5.names.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var exception =
                Assert.Throws<ArgumentException>(
                    () =>
                        new ResolvedEmissionVector(
                            new IResolvedEmission[]
                            {
                                new ResolvedEmission(
                                    emitter,
                                    "panic",
                                    8.4),

                                new ResolvedEmission(
                                    emitter,
                                    "panic",
                                    2.1)
                            }));

            TestLog.Expected(
                exception.Message);

            TestLog.Pass(
                "Duplicate named channels could not enter one emission vector.");

            WriteFinished(
                "UNIQUE VECTOR CHANNELS");
        }

        [Fact]
        public void Resolved_vector_rejects_mixed_source_emitters()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 5 - VECTOR SOURCE IDENTITY");

            TestLog.Test(
                "One emission vector cannot mix facts from different source emitters.");

            var component =
                new TestComponent(
                    "gate5.source.component");

            var sourceA =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var sourceB =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var exception =
                Assert.Throws<ArgumentException>(
                    () =>
                        new ResolvedEmissionVector(
                            new IResolvedEmission[]
                            {
                                new ResolvedEmission(
                                    sourceA,
                                    "approval",
                                    0.8),

                                new ResolvedEmission(
                                    sourceB,
                                    "hesitation",
                                    0.2)
                            }));

            TestLog.Expected(
                exception.Message);

            TestLog.Pass(
                "Mixed-source emissions could not enter one vector.");

            WriteFinished(
                "VECTOR SOURCE IDENTITY");
        }

        [Fact]
        public void Vector_carriers_snapshot_component_collections()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 5 - VECTOR IMMUTABILITY");

            TestLog.Test(
                "Resolved vector remains stable after caller-owned collection mutation.");

            var component =
                new TestComponent(
                    "gate5.immutability.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var source =
                new List<IResolvedEmission>
                {
                    new ResolvedEmission(
                        emitter,
                        "approval",
                        0.8),

                    new ResolvedEmission(
                        emitter,
                        "hesitation",
                        0.2)
                };

            var vector =
                new ResolvedEmissionVector(
                    source);

            source.Clear();

            TestLog.State(
                $"Caller collection count after mutation = " +
                $"{source.Count}");

            TestLog.State(
                $"Carrier dimension after mutation = " +
                $"{vector.Dimension}");

            Assert.Empty(
                source);

            Assert.Equal(
                2,
                vector.Dimension);

            Assert.Equal(
                2,
                vector.Emissions.Count);

            Assert.Equal(
                "approval",
                vector.Emissions[0].Name);

            Assert.Equal(
                "hesitation",
                vector.Emissions[1].Name);

            Assert.False(
                vector.Emissions is IResolvedEmission[]);

            TestLog.Pass(
                "Vector retained its immutable snapshot after caller collection mutation.");

            WriteFinished(
                "VECTOR IMMUTABILITY");
        }

        [Fact]
        public void Propagated_vector_preserves_arbitrary_n_dimensions()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 5 - ARBITRARY VECTOR DIMENSION");

            TestLog.Test(
                "First-class vector carriers preserve arbitrary N-dimensional named emissions.");

            var component =
                new TestComponent(
                    "gate5.dimension.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var receiver =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate5.dimension.receiver");

            Assert.True(
                emitter.Neighbors.Add(
                    receiver,
                    "conversation",
                    1.0));

            Assert.True(
                emitter.Neighbors.TryGet(
                    receiver.Id,
                    "conversation",
                    out var relationship));

            Assert.NotNull(
                relationship);

            const int dimension =
                17;

            var emissions =
                new IResolvedEmission[
                    dimension];

            for (var index = 0;
                 index < dimension;
                 index++)
            {
                emissions[index] =
                    new ResolvedEmission(
                        emitter,
                        $"consumer-property-{index}",
                        index + 1.0);
            }

            var resolvedVector =
                new ResolvedEmissionVector(
                    emissions);

            var propagation =
                new EmissionVectorPropagation(
                    new ProportionalEmissionNormalization(),
                    new ExponentialDistancePropagation(
                        0.5));

            var result =
                propagation.Propagate(
                    resolvedVector,
                    relationship!);

            TestLog.State(
                $"Resolved dimensions = " +
                $"{resolvedVector.Dimension}");

            TestLog.State(
                $"Normalized dimensions = " +
                $"{result.Source.Dimension}");

            TestLog.State(
                $"Propagated dimensions = " +
                $"{result.Dimension}");

            Assert.Equal(
                dimension,
                resolvedVector.Dimension);

            Assert.Equal(
                dimension,
                result.Source.Dimension);

            Assert.Equal(
                dimension,
                result.Dimension);

            for (var index = 0;
                 index < dimension;
                 index++)
            {
                var expectedName =
                    $"consumer-property-{index}";

                Assert.Equal(
                    expectedName,
                    resolvedVector.Emissions[index].Name);

                Assert.Equal(
                    expectedName,
                    result.Source.Emissions[index].Emission.Name);

                Assert.Equal(
                    expectedName,
                    result.Emissions[index].Emission.Name);
            }

            TestLog.Pass(
                "Arbitrary N dimensions survived resolution, normalization, and propagation.");

            WriteFinished(
                "ARBITRARY VECTOR DIMENSION");
        }

        private static void InitializeEmissionReport()
        {
            TestLog.Initialize(
                $"emitter-certification-{DateTime.Now:yyyyMMdd-HHmmss}.log",
                "Emission");
        }

        private static void PublishLatestReport()
        {
            TestLog.State(
                $"Emission certification report = {TestLog.Path}");

            TestLog.WriteLatest(
                "emitter-certification-latest.log");
        }

        private void WriteFinished(
            string certification)
        {
            _testOutputHelper.WriteLine("");
            _testOutputHelper.WriteLine(
                "============================================");

            _testOutputHelper.WriteLine(
                $" EMITTER GATE 5 FINISHED - {certification}");

            _testOutputHelper.WriteLine(
                "============================================");

            _testOutputHelper.WriteLine(
                $" Report: {TestLog.Path}");

            _testOutputHelper.WriteLine("");
        }
    }
}