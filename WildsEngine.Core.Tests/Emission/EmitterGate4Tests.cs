using System;
using System.Collections.Generic;
using WildsEngine.Core.Emission;
using WildsEngine.Core.Neighbors.Operators;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;
using Xunit.Abstractions;

namespace WildsEngine.Core.Tests.Emission
{
    /// <summary>
    /// Gate 4 certification for immutable resolved-emission
    /// carriers.
    ///
    /// This gate proves:
    ///
    /// certified Gate 3 resolution
    ///     -> exact source emitter
    ///     -> consumer-owned emission identity
    ///     -> finite resolved scalar
    ///     -> immutable resolved-emission carrier
    ///
    /// Propagation, eligibility, receiver selection, and
    /// state operations remain outside this gate.
    /// </summary>
    public sealed class EmitterGate4Tests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public EmitterGate4Tests(
            ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper =
                testOutputHelper;
        }

        [Fact]
        public void Gate3_result_resolves_into_immutable_carrier()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 4 - RESOLVED EMISSION CARRIER");

            TestLog.Test(
                "Gate 3 output survives resolved-emission carrier construction.");

            var component =
                new TestComponent(
                    "gate4.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["abyssal-lure-resonance"] = 1.6
                        });

            var alphaA =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate4.alpha.a");

            var alphaB =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate4.alpha.b");

            var beta =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate4.beta");

            TestLog.Task(
                "Building topology for resolved emission.");

            Assert.True(
                emitter.Neighbors.Add(
                    alphaA,
                    "alpha",
                    1.0));

            Assert.True(
                emitter.Neighbors.Add(
                    alphaB,
                    "alpha",
                    2.0));

            Assert.True(
                emitter.Neighbors.Add(
                    beta,
                    "beta",
                    1.0));

            var resolved =
                emitter.TryResolveTestEmissionResult(
                    "abyssal-lure-resonance",
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var result);

            Assert.True(
                resolved);

            Assert.NotNull(
                result);

            TestLog.State(
                $"Source Id = {result!.Source.Id}");

            TestLog.State(
                $"Exact source reference = {ReferenceEquals(emitter, result.Source)}");

            TestLog.State(
                $"Emission name = {result.Name}");

            TestLog.State(
                $"Resolved value = {result.Value}");

            Assert.Same(
                emitter,
                result.Source);

            Assert.Equal(
                emitter.Id,
                result.Source.Id);

            Assert.Equal(
                "abyssal-lure-resonance",
                result.Name);

            Assert.Equal(
                3.6,
                result.Value,
                10);

            TestLog.Pass(
                "Resolved emission preserved exact source, vocabulary, and value.");

            PublishLatestReport();

            WriteFinished(
                "RESOLVED EMISSION CARRIER");
        }

        [Fact]
        public void Missing_consumer_emission_creates_no_carrier()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 4 - MISSING EMISSION");

            TestLog.Test(
                "Missing consumer vocabulary cannot produce a resolved-emission carrier.");

            var component =
                new TestComponent(
                    "gate4.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var neighbor =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate4.neighbor");

            Assert.True(
                emitter.Neighbors.Add(
                    neighbor,
                    "alpha",
                    1.0));

            var resolved =
                emitter.TryResolveTestEmissionResult(
                    "abyssal-lure-resonance",
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var result);

            TestLog.State(
                $"Resolution succeeded = {resolved}");

            TestLog.State(
                $"Carrier created = {result != null}");

            Assert.False(
                resolved);

            Assert.Null(
                result);

            TestLog.Pass(
                "Missing consumer emission produced no resolved carrier.");

            WriteFinished(
                "MISSING EMISSION");
        }

        [Fact]
        public void Carrier_preserves_exact_source_reference()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 4 - SOURCE IDENTITY");

            TestLog.Test(
                "Resolved emission preserves the exact source emitter reference.");

            var component =
                new TestComponent(
                    "gate4.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var result =
                new ResolvedEmission(
                    emitter,
                    "signal",
                    4.25);

            TestLog.State(
                $"Emitter Id = {emitter.Id}");

            TestLog.State(
                $"Carrier source Id = {result.Source.Id}");

            TestLog.State(
                $"Exact reference = {ReferenceEquals(emitter, result.Source)}");

            Assert.Same(
                emitter,
                result.Source);

            Assert.Equal(
                emitter.Id,
                result.Source.Id);

            TestLog.Pass(
                "Exact emitter reference survived carrier construction.");

            WriteFinished(
                "SOURCE IDENTITY");
        }

        [Fact]
        public void Carrier_preserves_arbitrary_consumer_vocabulary()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 4 - CONSUMER VOCABULARY");

            TestLog.Test(
                "Resolved emission preserves arbitrary consumer-owned vocabulary.");

            var component =
                new TestComponent(
                    "gate4.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            const string emissionName =
                "nocturnal-abyssal-fungal-lure-resonance";

            var result =
                new ResolvedEmission(
                    emitter,
                    emissionName,
                    7.25);

            TestLog.State(
                $"Emission name = {result.Name}");

            Assert.Equal(
                emissionName,
                result.Name);

            Assert.Equal(
                7.25,
                result.Value,
                10);

            TestLog.Pass(
                "Resolved carrier preserved unfamiliar consumer vocabulary.");

            WriteFinished(
                "CONSUMER VOCABULARY");
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Carrier_rejects_non_finite_values(
            double value)
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 4 - FINITE VALUE INVARIANT");

            TestLog.Test(
                "Resolved-emission carrier rejects non-finite scalar values.");

            var component =
                new TestComponent(
                    "gate4.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var exception =
                Assert.Throws<ArgumentOutOfRangeException>(
                    () =>
                    {
                        new ResolvedEmission(
                            emitter,
                            "signal",
                            value);
                    });

            TestLog.Expected(
                $"{exception.GetType().Name} for non-finite value {value}");

            TestLog.Pass(
                "Non-finite value could not enter a resolved-emission carrier.");

            WriteFinished(
                "FINITE VALUE INVARIANT");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void Carrier_rejects_empty_emission_identity(
            string emissionName)
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 4 - EMISSION IDENTITY INVARIANT");

            TestLog.Test(
                "Resolved-emission carrier requires consumer-owned emission identity.");

            var component =
                new TestComponent(
                    "gate4.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var exception =
                Assert.Throws<ArgumentException>(
                    () =>
                    {
                        new ResolvedEmission(
                            emitter,
                            emissionName,
                            1.0);
                    });

            TestLog.Expected(
                exception.GetType().Name);

            TestLog.Pass(
                "Empty emission identity could not enter the carrier.");

            WriteFinished(
                "EMISSION IDENTITY INVARIANT");
        }

        [Fact]
        public void Carrier_normalizes_outer_whitespace_without_changing_vocabulary()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 4 - EMISSION NAME NORMALIZATION");

            TestLog.Test(
                "Resolved emission trims only outer whitespace from consumer vocabulary.");

            var component =
                new TestComponent(
                    "gate4.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var result =
                new ResolvedEmission(
                    emitter,
                    "  abyssal-lure-resonance  ",
                    2.5);

            TestLog.State(
                $"Stored emission name = {result.Name}");

            Assert.Equal(
                "abyssal-lure-resonance",
                result.Name);

            Assert.Equal(
                2.5,
                result.Value,
                10);

            TestLog.Pass(
                "Resolved emission preserved normalized consumer vocabulary.");

            WriteFinished(
                "EMISSION NAME NORMALIZATION");
        }

        [Fact]
        public void Carrier_rejects_null_source()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 4 - SOURCE INVARIANT");

            TestLog.Test(
                "Resolved-emission carrier cannot exist without a source emitter.");

            var exception =
                Assert.Throws<ArgumentNullException>(
                    () =>
                    {
                        new ResolvedEmission(
                            null!,
                            "signal",
                            1.0);
                    });

            TestLog.Expected(
                exception.GetType().Name);

            TestLog.Pass(
                "Resolved emission could not be constructed without its source.");

            WriteFinished(
                "SOURCE INVARIANT");
        }

        [Fact]
        public void Carrier_does_not_change_when_source_topology_changes()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 4 - SNAPSHOT STABILITY");

            TestLog.Test(
                "Resolved emission remains stable after source topology changes.");

            var component =
                new TestComponent(
                    "gate4.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["signal"] = 5.0
                        });

            var first =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate4.first");

            var second =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate4.second");

            emitter.Neighbors.Add(
                first,
                "active",
                1.0);

            emitter.Neighbors.Add(
                second,
                "active",
                1.0);

            Assert.True(
                emitter.TryResolveTestEmissionResult(
                    "signal",
                    new MembershipNeighborFilter(
                        "active"),
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var result));

            Assert.NotNull(
                result);

            var originalValue =
                result!.Value;

            TestLog.State(
                $"Carrier value before topology change = {originalValue}");

            TestLog.Task(
                "Changing source topology after carrier construction.");

            Assert.True(
                emitter.Neighbors.Remove(
                    second.Id,
                    "active"));

            Assert.True(
                emitter.Neighbors.Add(
                    second,
                    "inactive",
                    1.0));

            TestLog.State(
                $"Carrier value after topology change = {result.Value}");

            Assert.Equal(
                7.0,
                originalValue,
                10);

            Assert.Equal(
                originalValue,
                result.Value,
                10);

            Assert.Same(
                emitter,
                result.Source);

            Assert.Equal(
                "signal",
                result.Name);

            TestLog.Pass(
                "Resolved-emission carrier remained stable after topology changed.");

            WriteFinished(
                "SNAPSHOT STABILITY");
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
                $" EMITTER GATE 4 FINISHED - {certification}");

            _testOutputHelper.WriteLine(
                "============================================");

            _testOutputHelper.WriteLine(
                $" Report: {TestLog.Path}");

            _testOutputHelper.WriteLine("");
        }
    }
}
