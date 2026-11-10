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
    /// Gate 6 certification for propagated-emission
    /// eligibility.
    ///
    /// This gate proves:
    ///
    /// scalar propagated emission
    ///     -> received propagated Value
    ///     -> existing Neighbor eligibility policy
    ///     -> immutable eligibility result
    ///
    /// propagated emission vector
    ///     -> every received component evaluated
    ///     -> immutable eligibility vector
    ///     -> eligible and ineligible channels retained
    ///     -> any/all/count summaries derived honestly
    ///
    /// Eligibility must evaluate the received propagated
    /// value. It must not substitute the resolved magnitude
    /// or normalized magnitude.
    ///
    /// Exact Gate 5 source, vector, component, relationship,
    /// distance, and magnitude provenance must remain intact.
    ///
    /// Consumer interpretation, receiver-state mutation,
    /// cross-channel transformation, and QSL remain outside
    /// this gate.
    /// </summary>
    public sealed class EmitterGate6EligibilityTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public EmitterGate6EligibilityTests(
            ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper =
                testOutputHelper;
        }

        [Fact]
        public void Scalar_eligibility_evaluates_received_propagated_value()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - SCALAR ELIGIBILITY");

            TestLog.Test(
                "Eligibility evaluates received intensity rather than resolved source magnitude.");

            var propagated =
                CreateScalarPropagatedEmission(
                    resolvedValue: 0.80,
                    distance: 2.0,
                    decay: 0.50);

            const double threshold =
                0.30;

            var eligibility =
                new EmissionEligibility(
                    new MinimumIntensityEligibility(
                        threshold));

            var result =
                eligibility.Evaluate(
                    propagated);

            TestLog.State(
                $"Resolved value = " +
                $"{propagated.ResolvedValue:R}");

            TestLog.State(
                $"Received value = " +
                $"{propagated.Value:R}");

            TestLog.State(
                $"Eligibility threshold = " +
                $"{threshold:R}");

            TestLog.State(
                $"Eligibility evaluated value = " +
                $"{result.EvaluatedValue:R}");

            TestLog.State(
                $"Eligible = {result.IsEligible}");

            Assert.True(
                propagated.ResolvedValue >
                threshold);

            Assert.True(
                propagated.Value <
                threshold);

            Assert.Same(
                propagated,
                result.Emission);

            Assert.Equal(
                propagated.Value,
                result.EvaluatedValue,
                12);

            Assert.False(
                result.IsEligible);

            TestLog.Pass(
                "Eligibility ignored the stronger resolved magnitude and evaluated the received intensity.");

            TestLog.Pass(
                "A source emission above threshold remained ineligible after propagation attenuated it below threshold.");

            PublishLatestReport();

            WriteFinished(
                "SCALAR ELIGIBILITY");
        }

        [Fact]
        public void Eligibility_remains_inclusive_at_exact_received_threshold()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - EXACT RECEIVED THRESHOLD");

            TestLog.Test(
                "Received intensity remains eligible when it exactly equals the supplied threshold.");

            var propagated =
                CreateScalarPropagatedEmission(
                    resolvedValue: 1.0,
                    distance: 2.0,
                    decay: 0.50);

            var eligibility =
                new EmissionEligibility(
                    new MinimumIntensityEligibility(
                        propagated.Value));

            var result =
                eligibility.Evaluate(
                    propagated);

            TestLog.State(
                $"Resolved value = " +
                $"{propagated.ResolvedValue:R}");

            TestLog.State(
                $"Received value = " +
                $"{propagated.Value:R}");

            TestLog.State(
                $"Exact threshold = " +
                $"{propagated.Value:R}");

            Assert.Equal(
                propagated.Value,
                result.EvaluatedValue,
                12);

            Assert.True(
                result.IsEligible);

            TestLog.Pass(
                "Exact received intensity remained eligible at the inclusive threshold.");

            WriteFinished(
                "EXACT RECEIVED THRESHOLD");
        }

        [Fact]
        public void Vector_eligibility_evaluates_every_received_channel()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - VECTOR ELIGIBILITY");

            TestLog.Test(
                "Every propagated vector channel receives an independent eligibility decision.");

            const double threshold =
                0.10;

            var propagated =
                CreateFiveChannelPropagatedVector();

            var eligibility =
                new EmissionVectorEligibility(
                    new MinimumIntensityEligibility(
                        threshold));

            var result =
                eligibility.Evaluate(
                    propagated);

            TestLog.State(
                $"Dimensions = {result.Dimension}");

            TestLog.State(
                $"Threshold = {threshold:R}");

            TestLog.State(
                $"Eligible components = " +
                $"{result.EligibleCount}");

            TestLog.State(
                $"Any eligible = {result.AnyEligible}");

            TestLog.State(
                $"All eligible = {result.AllEligible}");

            Assert.Same(
                propagated,
                result.Source);

            Assert.Equal(
                propagated.Dimension,
                result.Dimension);

            Assert.Equal(
                5,
                result.Dimension);

            Assert.Equal(
                2,
                result.EligibleCount);

            Assert.True(
                result.AnyEligible);

            Assert.False(
                result.AllEligible);

            for (var index = 0;
                 index < result.Dimension;
                 index++)
            {
                var propagatedComponent =
                    propagated.Emissions[index];

                var decision =
                    result.Emissions[index];

                TestLog.State(
                    $"{propagatedComponent.Emission.Name} | " +
                    $"resolved = " +
                    $"{propagatedComponent.ResolvedValue:R} | " +
                    $"normalized = " +
                    $"{propagatedComponent.NormalizedValue!.Value:R} | " +
                    $"received = " +
                    $"{decision.EvaluatedValue:R} | " +
                    $"eligible = " +
                    $"{decision.IsEligible}");

                Assert.Same(
                    propagatedComponent,
                    decision.Emission);

                Assert.Equal(
                    propagatedComponent.Value,
                    decision.EvaluatedValue,
                    12);
            }

            Assert.Equal(
                "panic",
                result.Emissions[0].Emission.Emission.Name);

            Assert.True(
                result.Emissions[0].IsEligible);

            Assert.Equal(
                "urgency",
                result.Emissions[1].Emission.Emission.Name);

            Assert.True(
                result.Emissions[1].IsEligible);

            Assert.Equal(
                "confusion",
                result.Emissions[2].Emission.Emission.Name);

            Assert.False(
                result.Emissions[2].IsEligible);

            Assert.Equal(
                "hesitation",
                result.Emissions[3].Emission.Emission.Name);

            Assert.False(
                result.Emissions[3].IsEligible);

            Assert.Equal(
                "trust",
                result.Emissions[4].Emission.Emission.Name);

            Assert.False(
                result.Emissions[4].IsEligible);

            TestLog.Pass(
                "Vector eligibility evaluated every received channel independently.");

            TestLog.Pass(
                "Eligible and ineligible channels remained present in the result.");

            WriteFinished(
                "VECTOR ELIGIBILITY");
        }

        [Fact]
        public void Vector_can_have_no_eligible_components_without_losing_provenance()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - NO ELIGIBLE CHANNELS");

            TestLog.Test(
                "A vector with no qualifying channels preserves every propagated fact.");

            var propagated =
                CreateFiveChannelPropagatedVector();

            var eligibility =
                new EmissionVectorEligibility(
                    new MinimumIntensityEligibility(
                        1.0));

            var result =
                eligibility.Evaluate(
                    propagated);

            TestLog.State(
                $"Dimensions = {result.Dimension}");

            TestLog.State(
                $"Eligible components = " +
                $"{result.EligibleCount}");

            TestLog.State(
                $"Any eligible = {result.AnyEligible}");

            TestLog.State(
                $"All eligible = {result.AllEligible}");

            Assert.Same(
                propagated,
                result.Source);

            Assert.Equal(
                propagated.Dimension,
                result.Dimension);

            Assert.Equal(
                0,
                result.EligibleCount);

            Assert.False(
                result.AnyEligible);

            Assert.False(
                result.AllEligible);

            Assert.Equal(
                propagated.Dimension,
                result.Emissions.Count);

            for (var index = 0;
                 index < result.Dimension;
                 index++)
            {
                Assert.False(
                    result.Emissions[index].IsEligible);

                Assert.Same(
                    propagated.Emissions[index],
                    result.Emissions[index].Emission);

                Assert.Equal(
                    propagated.Emissions[index].Value,
                    result.Emissions[index].EvaluatedValue,
                    12);
            }

            TestLog.Pass(
                "No channels qualified, but no propagated provenance was discarded.");

            WriteFinished(
                "NO ELIGIBLE CHANNELS");
        }

        [Fact]
        public void Vector_can_have_all_components_eligible()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - ALL CHANNELS ELIGIBLE");

            TestLog.Test(
                "A zero eligibility threshold admits every finite non-negative received channel.");

            var propagated =
                CreateFiveChannelPropagatedVector();

            var eligibility =
                new EmissionVectorEligibility(
                    new MinimumIntensityEligibility(
                        0.0));

            var result =
                eligibility.Evaluate(
                    propagated);

            TestLog.State(
                $"Dimensions = {result.Dimension}");

            TestLog.State(
                $"Eligible components = " +
                $"{result.EligibleCount}");

            TestLog.State(
                $"Any eligible = {result.AnyEligible}");

            TestLog.State(
                $"All eligible = {result.AllEligible}");

            Assert.Equal(
                result.Dimension,
                result.EligibleCount);

            Assert.True(
                result.AnyEligible);

            Assert.True(
                result.AllEligible);

            for (var index = 0;
                 index < result.Dimension;
                 index++)
            {
                Assert.True(
                    result.Emissions[index].IsEligible);
            }

            TestLog.Pass(
                "Every received vector channel qualified at the inclusive zero threshold.");

            WriteFinished(
                "ALL CHANNELS ELIGIBLE");
        }

        [Fact]
        public void Eligibility_preserves_complete_gate5_provenance_chain()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - PROVENANCE CONTINUITY");

            TestLog.Test(
                "Eligibility preserves the complete propagated-to-emitter provenance chain.");

            var component =
                new TestComponent(
                    "gate6.provenance.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var receiver =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate6.provenance.receiver");

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
                    });

            var propagation =
                new EmissionVectorPropagation(
                    new ProportionalEmissionNormalization(),
                    new ExponentialDistancePropagation(
                        0.25));

            var propagatedVector =
                propagation.Propagate(
                    resolvedVector,
                    relationship!);

            var normalizedVector =
                propagatedVector.Source;

            var eligibility =
                new EmissionVectorEligibility(
                    new MinimumIntensityEligibility(
                        0.10));

            var eligibilityVector =
                eligibility.Evaluate(
                    propagatedVector);

            TestLog.State(
                $"Eligibility exact propagated source = " +
                $"{ReferenceEquals(
                    propagatedVector,
                    eligibilityVector.Source)}");

            TestLog.State(
                $"Propagated exact normalized source = " +
                $"{ReferenceEquals(
                    normalizedVector,
                    eligibilityVector.Source.Source)}");

            TestLog.State(
                $"Normalized exact resolved source = " +
                $"{ReferenceEquals(
                    resolvedVector,
                    eligibilityVector.Source.Source.Source)}");

            TestLog.State(
                $"Resolved exact emitter source = " +
                $"{ReferenceEquals(
                    emitter,
                    eligibilityVector.Source.Source.Source.Source)}");

            TestLog.State(
                $"Relationship membership = " +
                $"{eligibilityVector.Source.Relationship.Membership}");

            TestLog.State(
                $"Frozen propagation distance = " +
                $"{eligibilityVector.Source.PropagationDistance:R}");

            TestLog.State(
                $"Eligibility dimensions = " +
                $"{eligibilityVector.Dimension}");

            TestLog.State(
                $"Eligible components = " +
                $"{eligibilityVector.EligibleCount}");

            Assert.Same(
                propagatedVector,
                eligibilityVector.Source);

            Assert.Same(
                normalizedVector,
                eligibilityVector.Source.Source);

            Assert.Same(
                resolvedVector,
                eligibilityVector.Source.Source.Source);

            Assert.Same(
                emitter,
                eligibilityVector.Source.Source.Source.Source);

            Assert.Same(
                relationship,
                eligibilityVector.Source.Relationship);

            Assert.Equal(
                relationship.Distance,
                eligibilityVector.Source.PropagationDistance,
                12);

            Assert.Equal(
                resolvedVector.Dimension,
                eligibilityVector.Dimension);

            for (var index = 0;
                 index < eligibilityVector.Dimension;
                 index++)
            {
                var resolved =
                    resolvedVector.Emissions[index];

                var normalized =
                    normalizedVector.Emissions[index];

                var propagated =
                    propagatedVector.Emissions[index];

                var decision =
                    eligibilityVector.Emissions[index];

                TestLog.State(
                    $"{resolved.Name} | " +
                    $"resolved = {resolved.Value:R} | " +
                    $"normalized = " +
                    $"{normalized.NormalizedValue:R} | " +
                    $"received = {propagated.Value:R} | " +
                    $"evaluated = {decision.EvaluatedValue:R} | " +
                    $"eligible = {decision.IsEligible}");

                Assert.Same(
                    propagated,
                    decision.Emission);

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
                    propagated.PropagationDistance,
                    eligibilityVector.Source.PropagationDistance,
                    12);

                Assert.Equal(
                    resolved.Value,
                    propagated.ResolvedValue,
                    12);

                Assert.Equal(
                    normalized.NormalizedValue,
                    propagated.NormalizedValue!.Value,
                    12);

                Assert.Equal(
                    propagated.Value,
                    decision.EvaluatedValue,
                    12);
            }

            TestLog.Pass(
                "Eligibility retained exact propagated, normalized, resolved, emitter, and relationship provenance.");

            TestLog.Pass(
                "Gate 6 added decisions without replacing or rewriting Gate 5 facts.");

            PublishLatestReport();

            WriteFinished(
                "PROVENANCE CONTINUITY");
        }

        [Fact]
        public void Eligibility_vector_snapshots_component_decisions()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - ELIGIBILITY IMMUTABILITY");

            TestLog.Test(
                "Eligibility vector remains stable after caller-owned decision collection mutation.");

            var propagated =
                CreateFiveChannelPropagatedVector();

            var scalarEligibility =
                new EmissionEligibility(
                    new MinimumIntensityEligibility(
                        0.10));

            var decisions =
                new List<IEmissionEligibilityResult>(
                    propagated.Dimension);

            for (var index = 0;
                 index < propagated.Dimension;
                 index++)
            {
                decisions.Add(
                    scalarEligibility.Evaluate(
                        propagated.Emissions[index]));
            }

            var vector =
                new EmissionEligibilityVector(
                    propagated,
                    decisions);

            var originalDimension =
                vector.Dimension;

            var originalEligibleCount =
                vector.EligibleCount;

            decisions.Clear();

            TestLog.State(
                $"Caller decision count after mutation = " +
                $"{decisions.Count}");

            TestLog.State(
                $"Carrier dimension after mutation = " +
                $"{vector.Dimension}");

            TestLog.State(
                $"Carrier eligible count after mutation = " +
                $"{vector.EligibleCount}");

            Assert.Empty(
                decisions);

            Assert.Equal(
                originalDimension,
                vector.Dimension);

            Assert.Equal(
                originalEligibleCount,
                vector.EligibleCount);

            Assert.Equal(
                propagated.Dimension,
                vector.Emissions.Count);

            Assert.False(
                vector.Emissions is
                    IEmissionEligibilityResult[]);

            TestLog.Pass(
                "Eligibility vector retained its immutable decision snapshot.");

            WriteFinished(
                "ELIGIBILITY IMMUTABILITY");
        }

        [Fact]
        public void Eligibility_vector_preserves_arbitrary_n_dimensions()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - ARBITRARY VECTOR DIMENSION");

            TestLog.Test(
                "Eligibility preserves arbitrary N-dimensional propagated vectors.");

            const int dimension =
                17;

            var propagated =
                CreatePropagatedVector(
                    dimension);

            var eligibility =
                new EmissionVectorEligibility(
                    new MinimumIntensityEligibility(
                        0.0));

            var result =
                eligibility.Evaluate(
                    propagated);

            TestLog.State(
                $"Propagated dimensions = " +
                $"{propagated.Dimension}");

            TestLog.State(
                $"Eligibility dimensions = " +
                $"{result.Dimension}");

            TestLog.State(
                $"Eligible components = " +
                $"{result.EligibleCount}");

            Assert.Equal(
                dimension,
                propagated.Dimension);

            Assert.Equal(
                dimension,
                result.Dimension);

            Assert.Equal(
                dimension,
                result.EligibleCount);

            Assert.True(
                result.AllEligible);

            for (var index = 0;
                 index < dimension;
                 index++)
            {
                var expectedName =
                    $"consumer-property-{index}";

                Assert.Equal(
                    expectedName,
                    result.Emissions[index]
                        .Emission
                        .Emission
                        .Name);

                Assert.Same(
                    propagated.Emissions[index],
                    result.Emissions[index].Emission);
            }

            TestLog.Pass(
                "Arbitrary N dimensions survived Gate 6 eligibility evaluation.");

            WriteFinished(
                "ARBITRARY VECTOR DIMENSION");
        }

        [Fact]
        public void Eligibility_result_rejects_mismatched_evaluated_value()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - EVALUATED VALUE INVARIANT");

            TestLog.Test(
                "Eligibility result cannot claim that it evaluated a value other than received intensity.");

            var propagated =
                CreateScalarPropagatedEmission(
                    resolvedValue: 1.0,
                    distance: 2.0,
                    decay: 0.50);

            var incorrectValue =
                propagated.ResolvedValue;

            Assert.NotEqual(
                propagated.Value,
                incorrectValue);

            var exception =
                Assert.Throws<ArgumentException>(
                    () =>
                        new EmissionEligibilityResult(
                            propagated,
                            incorrectValue,
                            isEligible: true));

            TestLog.State(
                $"Resolved value = " +
                $"{propagated.ResolvedValue:R}");

            TestLog.State(
                $"Received value = " +
                $"{propagated.Value:R}");

            TestLog.State(
                $"Rejected evaluated value = " +
                $"{incorrectValue:R}");

            TestLog.Expected(
                exception.Message);

            TestLog.Pass(
                "Eligibility result could not substitute resolved magnitude for received intensity.");

            WriteFinished(
                "EVALUATED VALUE INVARIANT");
        }

        [Fact]
        public void Eligibility_vector_rejects_dimension_mismatch()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - DIMENSION VALIDATION");

            TestLog.Test(
                "Eligibility vector rejects a decision set with incompatible dimensions.");

            var propagated =
                CreateFiveChannelPropagatedVector();

            var scalarEligibility =
                new EmissionEligibility(
                    new MinimumIntensityEligibility(
                        0.10));

            var incomplete =
                new List<IEmissionEligibilityResult>
                {
                    scalarEligibility.Evaluate(
                        propagated.Emissions[0]),

                    scalarEligibility.Evaluate(
                        propagated.Emissions[1])
                };

            var exception =
                Assert.Throws<ArgumentException>(
                    () =>
                        new EmissionEligibilityVector(
                            propagated,
                            incomplete));

            TestLog.State(
                $"Propagated dimensions = " +
                $"{propagated.Dimension}");

            TestLog.State(
                $"Eligibility dimensions = " +
                $"{incomplete.Count}");

            TestLog.Expected(
                exception.Message);

            TestLog.Pass(
                "Eligibility dimension mismatch was rejected explicitly.");

            WriteFinished(
                "DIMENSION VALIDATION");
        }

        [Fact]
        public void Eligibility_vector_rejects_reordered_components()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 6 - COMPONENT ORDER VALIDATION");

            TestLog.Test(
                "Eligibility vector rejects decisions that do not preserve propagated component ordering.");

            var propagated =
                CreateFiveChannelPropagatedVector();

            var scalarEligibility =
                new EmissionEligibility(
                    new MinimumIntensityEligibility(
                        0.10));

            var reordered =
                new List<IEmissionEligibilityResult>(
                    propagated.Dimension)
                {
                    scalarEligibility.Evaluate(
                        propagated.Emissions[1]),

                    scalarEligibility.Evaluate(
                        propagated.Emissions[0]),

                    scalarEligibility.Evaluate(
                        propagated.Emissions[2]),

                    scalarEligibility.Evaluate(
                        propagated.Emissions[3]),

                    scalarEligibility.Evaluate(
                        propagated.Emissions[4])
                };

            var exception =
                Assert.Throws<ArgumentException>(
                    () =>
                        new EmissionEligibilityVector(
                            propagated,
                            reordered));

            TestLog.Expected(
                exception.Message);

            TestLog.Pass(
                "Eligibility could not reorder propagated vector channels.");

            WriteFinished(
                "COMPONENT ORDER VALIDATION");
        }
        
        [Fact]
public void Scalar_eligibility_remains_stable_after_topology_changes()
{
    InitializeEmissionReport();

    TestLog.Section(
        "EMITTER GATE 6 - SCALAR TOPOLOGY SNAPSHOT");

    TestLog.Test(
        "Eligibility remains an immutable decision about the propagated fact after live topology changes.");

    var component =
        new TestComponent(
            "gate6.scalar.snapshot.component");

    var emitter =
        WildsFixtureFactory.CreateEmitter(
            component);

    var receiver =
        WildsFixtureFactory.CreateTopologyObject(
            component,
            "gate6.scalar.snapshot.receiver");

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

    var resolved =
        new ResolvedEmission(
            emitter,
            "hesitation",
            1.0);

    var propagation =
        new EmissionPropagation(
            new ExponentialDistancePropagation(
                0.5));

    var propagated =
        propagation.Propagate(
            resolved,
            relationship!);

    var eligibility =
        new EmissionEligibility(
            new MinimumIntensityEligibility(
                0.30));

    var decision =
        eligibility.Evaluate(
            propagated);

    var originalValue =
        decision.EvaluatedValue;

    var originalEligible =
        decision.IsEligible;

    var originalDistance =
        propagated.PropagationDistance;

    TestLog.State(
        $"Original relationship distance = " +
        $"{relationship.Distance:R}");

    TestLog.State(
        $"Frozen propagation distance = " +
        $"{originalDistance:R}");

    TestLog.State(
        $"Evaluated value = " +
        $"{originalValue:R}");

    TestLog.State(
        $"Eligible before topology change = " +
        $"{originalEligible}");

    TestLog.Task(
        "Replacing the live relationship after eligibility evaluation.");

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
        $"Eligibility evaluated value after topology change = " +
        $"{decision.EvaluatedValue:R}");

    TestLog.State(
        $"Eligibility result after topology change = " +
        $"{decision.IsEligible}");

    TestLog.State(
        $"Historical propagation distance after topology change = " +
        $"{propagated.PropagationDistance:R}");

    Assert.NotSame(
        relationship,
        replacement);

    Assert.Equal(
        5.0,
        replacement.Distance,
        12);

    Assert.Same(
        propagated,
        decision.Emission);

    Assert.Same(
        relationship,
        decision.Emission.Relationship);

    Assert.Equal(
        originalDistance,
        decision.Emission.PropagationDistance,
        12);

    Assert.Equal(
        originalValue,
        decision.EvaluatedValue,
        12);

    Assert.Equal(
        originalEligible,
        decision.IsEligible);

    TestLog.Pass(
        "Scalar eligibility remained bound to the original propagated fact after live topology changed.");

    TestLog.Pass(
        "Eligibility value, decision, exact relationship, and frozen propagation distance remained historical facts.");

    WriteFinished(
        "SCALAR TOPOLOGY SNAPSHOT");
}

[Fact]
public void Vector_eligibility_remains_stable_after_topology_changes()
{
    InitializeEmissionReport();

    TestLog.Section(
        "EMITTER GATE 6 - VECTOR TOPOLOGY SNAPSHOT");

    TestLog.Test(
        "Complete vector eligibility remains stable after live topology changes.");

    var component =
        new TestComponent(
            "gate6.vector.snapshot.component");

    var emitter =
        WildsFixtureFactory.CreateEmitter(
            component);

    var receiver =
        WildsFixtureFactory.CreateTopologyObject(
            component,
            "gate6.vector.snapshot.receiver");

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
            });

    var propagation =
        new EmissionVectorPropagation(
            new ProportionalEmissionNormalization(),
            new ExponentialDistancePropagation(
                0.25));

    var propagatedVector =
        propagation.Propagate(
            resolvedVector,
            relationship!);

    var eligibility =
        new EmissionVectorEligibility(
            new MinimumIntensityEligibility(
                0.10));

    var decisionVector =
        eligibility.Evaluate(
            propagatedVector);

    var originalEligibleCount =
        decisionVector.EligibleCount;

    var originalAnyEligible =
        decisionVector.AnyEligible;

    var originalAllEligible =
        decisionVector.AllEligible;

    var originalDistance =
        propagatedVector.PropagationDistance;

    var evaluatedValues =
        new double[
            decisionVector.Dimension];

    var eligibilityDecisions =
        new bool[
            decisionVector.Dimension];

    for (var index = 0;
         index < decisionVector.Dimension;
         index++)
    {
        evaluatedValues[index] =
            decisionVector.Emissions[index]
                .EvaluatedValue;

        eligibilityDecisions[index] =
            decisionVector.Emissions[index]
                .IsEligible;
    }

    TestLog.State(
        $"Original relationship distance = " +
        $"{relationship.Distance:R}");

    TestLog.State(
        $"Frozen propagation distance = " +
        $"{originalDistance:R}");

    TestLog.State(
        $"Dimensions = " +
        $"{decisionVector.Dimension}");

    TestLog.State(
        $"Eligible components before topology change = " +
        $"{originalEligibleCount}");

    TestLog.State(
        $"Any eligible before topology change = " +
        $"{originalAnyEligible}");

    TestLog.State(
        $"All eligible before topology change = " +
        $"{originalAllEligible}");

    TestLog.Task(
        "Replacing the live relationship after vector eligibility evaluation.");

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
        $"{decisionVector.Source.PropagationDistance:R}");

    TestLog.State(
        $"Eligible components after topology change = " +
        $"{decisionVector.EligibleCount}");

    TestLog.State(
        $"Any eligible after topology change = " +
        $"{decisionVector.AnyEligible}");

    TestLog.State(
        $"All eligible after topology change = " +
        $"{decisionVector.AllEligible}");

    Assert.NotSame(
        relationship,
        replacement);

    Assert.Equal(
        5.0,
        replacement.Distance,
        12);

    Assert.Same(
        propagatedVector,
        decisionVector.Source);

    Assert.Same(
        relationship,
        decisionVector.Source.Relationship);

    Assert.Equal(
        originalDistance,
        decisionVector.Source.PropagationDistance,
        12);

    Assert.Equal(
        resolvedVector.Dimension,
        decisionVector.Dimension);

    Assert.Equal(
        originalEligibleCount,
        decisionVector.EligibleCount);

    Assert.Equal(
        originalAnyEligible,
        decisionVector.AnyEligible);

    Assert.Equal(
        originalAllEligible,
        decisionVector.AllEligible);

    for (var index = 0;
         index < decisionVector.Dimension;
         index++)
    {
        var propagated =
            propagatedVector.Emissions[index];

        var decision =
            decisionVector.Emissions[index];

        TestLog.State(
            $"{propagated.Emission.Name} | " +
            $"evaluated = {decision.EvaluatedValue:R} | " +
            $"eligible = {decision.IsEligible} | " +
            $"distance = {propagated.PropagationDistance:R}");

        Assert.Same(
            propagated,
            decision.Emission);

        Assert.Same(
            relationship,
            propagated.Relationship);

        Assert.Equal(
            originalDistance,
            propagated.PropagationDistance,
            12);

        Assert.Equal(
            evaluatedValues[index],
            decision.EvaluatedValue,
            12);

        Assert.Equal(
            eligibilityDecisions[index],
            decision.IsEligible);
    }

    TestLog.Pass(
        "Vector eligibility remained bound to the original propagated vector after live topology changed.");

    TestLog.Pass(
        "Eligible count, any/all summaries, component decisions, received values, and propagation provenance remained stable.");

    WriteFinished(
        "VECTOR TOPOLOGY SNAPSHOT");
}

        private static IPropagatedEmission
            CreateScalarPropagatedEmission(
                double resolvedValue,
                double distance,
                double decay)
        {
            var component =
                new TestComponent(
                    "gate6.scalar.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var receiver =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate6.scalar.receiver");

            if (!emitter.Neighbors.Add(
                    receiver,
                    "eligibility",
                    distance))
            {
                throw new InvalidOperationException(
                    "Could not create the scalar eligibility " +
                    "test relationship.");
            }

            if (!emitter.Neighbors.TryGet(
                    receiver.Id,
                    "eligibility",
                    out var relationship) ||
                relationship == null)
            {
                throw new InvalidOperationException(
                    "Could not retrieve the scalar eligibility " +
                    "test relationship.");
            }

            var resolved =
                new ResolvedEmission(
                    emitter,
                    "scalar-signal",
                    resolvedValue);

            var propagation =
                new EmissionPropagation(
                    new ExponentialDistancePropagation(
                        decay));

            return propagation.Propagate(
                resolved,
                relationship);
        }

        private static IPropagatedEmissionVector
            CreateFiveChannelPropagatedVector()
        {
            var component =
                new TestComponent(
                    "gate6.vector.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var receiver =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate6.vector.receiver");

            if (!emitter.Neighbors.Add(
                    receiver,
                    "conversation",
                    2.0))
            {
                throw new InvalidOperationException(
                    "Could not create the vector eligibility " +
                    "test relationship.");
            }

            if (!emitter.Neighbors.TryGet(
                    receiver.Id,
                    "conversation",
                    out var relationship) ||
                relationship == null)
            {
                throw new InvalidOperationException(
                    "Could not retrieve the vector eligibility " +
                    "test relationship.");
            }

            var resolved =
                new ResolvedEmissionVector(
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
                    });

            var propagation =
                new EmissionVectorPropagation(
                    new ProportionalEmissionNormalization(),
                    new ExponentialDistancePropagation(
                        0.25));

            return propagation.Propagate(
                resolved,
                relationship);
        }

        private static IPropagatedEmissionVector
            CreatePropagatedVector(
                int dimension)
        {
            if (dimension <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(dimension));
            }

            var component =
                new TestComponent(
                    "gate6.dimension.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var receiver =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate6.dimension.receiver");

            if (!emitter.Neighbors.Add(
                    receiver,
                    "dimension",
                    1.0))
            {
                throw new InvalidOperationException(
                    "Could not create the dimensional " +
                    "eligibility test relationship.");
            }

            if (!emitter.Neighbors.TryGet(
                    receiver.Id,
                    "dimension",
                    out var relationship) ||
                relationship == null)
            {
                throw new InvalidOperationException(
                    "Could not retrieve the dimensional " +
                    "eligibility test relationship.");
            }

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

            var resolved =
                new ResolvedEmissionVector(
                    emissions);

            var propagation =
                new EmissionVectorPropagation(
                    new ProportionalEmissionNormalization(),
                    new ExponentialDistancePropagation(
                        0.50));

            return propagation.Propagate(
                resolved,
                relationship);
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
                $" EMITTER GATE 6 FINISHED - {certification}");

            _testOutputHelper.WriteLine(
                "============================================");

            _testOutputHelper.WriteLine(
                $" Report: {TestLog.Path}");

            _testOutputHelper.WriteLine("");
        }
    }
}