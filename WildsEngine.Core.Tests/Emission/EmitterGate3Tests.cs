using WildsEngine.Core.Neighbors.Operators;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit.Abstractions;

namespace WildsEngine.Core.Tests.Emission
{
    /// <summary>
    /// Gate 3 certification for complete scalar emission
    /// resolution.
    ///
    /// This gate proves the complete internal route:
    ///
    /// consumer value
    ///     -> base scalar
    ///     -> current topology
    ///     -> topology selection
    ///     -> topology scalar
    ///     -> supplied composition
    ///     -> resolved emission
    ///
    /// Propagation, receiver eligibility, and state operations
    /// remain outside this gate.
    /// </summary>
    public sealed class EmitterGate3Tests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public EmitterGate3Tests(
            ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper =
                testOutputHelper;
        }

        [Fact]
        public void Complete_emission_route_resolves_in_one_operation()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 3 - COMPLETE EMISSION RESOLUTION");

            TestLog.Test(
                "Consumer value and current topology resolve into one emission.");

            var component =
                new TestComponent(
                    "gate3.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["signal"] = 1.6
                        });

            var alphaA =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.alpha.a");

            var alphaB =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.alpha.b");

            var beta =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.beta");

            TestLog.Task(
                "Building emitter topology.");

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

            TestLog.State(
                $"Neighbor relationships = {emitter.Neighbors.Count}");

            TestLog.Task(
                "Resolving base emission through alpha topology.");

            var alphaRelationships =
                emitter.ApplyNeighborOperator(
                    new MembershipNeighborFilter(
                        "alpha"));

            var topologyScalar =
                new NeighborCount().Apply(
                    emitter,
                    alphaRelationships);

            var resolved =
                emitter.TryResolveTestEmission(
                    "signal",
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var value);

            TestLog.State(
                "Base emission = 1.6");

            TestLog.State(
                "Selected membership = alpha");

            TestLog.State(
                $"Topology scalar = {topologyScalar}");

            TestLog.State(
                $"Resolved emission = {value}");

            Assert.True(
                resolved);

            Assert.Equal(
                2,
                topologyScalar);

            Assert.Equal(
                3.6,
                value,
                10);

            TestLog.Pass(
                "Complete emission route resolved successfully.");

            PublishLatestReport();

            WriteFinished(
                "COMPLETE EMISSION RESOLUTION");
        }

        [Fact]
        public void Unrelated_memberships_do_not_contribute()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 3 - MEMBERSHIP ISOLATION");

            TestLog.Test(
                "Unrelated memberships do not contribute to the selected topology.");

            var component =
                new TestComponent(
                    "gate3.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["signal"] = 10.0
                        });

            var alpha =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.alpha");

            var betaA =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.beta.a");

            var betaB =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.beta.b");

            emitter.Neighbors.Add(
                alpha,
                "alpha",
                1.0);

            emitter.Neighbors.Add(
                betaA,
                "beta",
                1.0);

            emitter.Neighbors.Add(
                betaB,
                "beta",
                1.0);

            TestLog.State(
                $"Total relationships = {emitter.Neighbors.Count}");

            var resolved =
                emitter.TryResolveTestEmission(
                    "signal",
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var value);

            TestLog.State(
                $"Resolved alpha emission = {value}");

            Assert.True(
                resolved);

            Assert.Equal(
                11.0,
                value,
                10);

            TestLog.Pass(
                "Unrelated beta memberships did not contribute.");

            WriteFinished(
                "MEMBERSHIP ISOLATION");
        }

        [Fact]
        public void Membership_changes_immediately_change_resolved_emission()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 3 - DYNAMIC MEMBERSHIP");

            TestLog.Test(
                "Changing topology membership changes the next resolved emission.");

            var component =
                new TestComponent(
                    "gate3.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["signal"] = 5.0
                        });

            var objectA =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.object.a");

            var objectB =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.object.b");

            emitter.Neighbors.Add(
                objectA,
                "active",
                1.0);

            emitter.Neighbors.Add(
                objectB,
                "active",
                1.0);

            Assert.True(
                emitter.TryResolveTestEmission(
                    "signal",
                    new MembershipNeighborFilter(
                        "active"),
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var before));

            TestLog.State(
                $"Resolved active emission before topology change = {before}");

            TestLog.Task(
                "Moving one object from active to inactive membership.");

            Assert.True(
                emitter.Neighbors.Remove(
                    objectB.Id,
                    "active"));

            Assert.True(
                emitter.Neighbors.Add(
                    objectB,
                    "inactive",
                    1.0));

            Assert.True(
                emitter.TryResolveTestEmission(
                    "signal",
                    new MembershipNeighborFilter(
                        "active"),
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var after));

            TestLog.State(
                $"Resolved active emission after topology change = {after}");

            Assert.Equal(
                7.0,
                before,
                10);

            Assert.Equal(
                6.0,
                after,
                10);

            TestLog.Pass(
                "Dynamic membership change immediately affected emission resolution.");

            WriteFinished(
                "DYNAMIC MEMBERSHIP");
        }

        [Fact]
        public void Missing_consumer_property_stops_resolution()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 3 - MISSING PROPERTY");

            TestLog.Test(
                "Missing consumer vocabulary stops emission resolution cleanly.");

            var component =
                new TestComponent(
                    "gate3.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var neighbor =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.neighbor");

            emitter.Neighbors.Add(
                neighbor,
                "alpha",
                1.0);

            var resolved =
                emitter.TryResolveTestEmission(
                    "missing",
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var value);

            TestLog.State(
                $"Resolution succeeded = {resolved}");

            TestLog.State(
                $"Returned value = {value}");

            Assert.False(
                resolved);

            Assert.Equal(
                0.0,
                value);

            TestLog.Pass(
                "Missing consumer property produced no resolved emission.");

            WriteFinished(
                "MISSING PROPERTY");
        }

        [Fact]
        public void Pipeline_can_define_topology_before_scalar_resolution()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 3 - TOPOLOGY PIPELINE");

            TestLog.Test(
                "Existing Neighbor pipeline stages define topology before scalar reduction.");

            var component =
                new TestComponent(
                    "gate3.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["signal"] = 1.0
                        });

            var near =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.near");

            var middle =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.middle");

            var far =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.far");

            var unrelated =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.unrelated");

            emitter.Neighbors.Add(
                near,
                "alpha",
                1.0);

            emitter.Neighbors.Add(
                middle,
                "alpha",
                2.0);

            emitter.Neighbors.Add(
                far,
                "alpha",
                10.0);

            emitter.Neighbors.Add(
                unrelated,
                "beta",
                1.0);

            var pipeline =
                new NeighborPipeline(
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborWithinDistanceFilter(
                        2.0));

            var selected =
                emitter.ApplyNeighborOperator(
                    pipeline);

            TestLog.State(
                $"Source relationships = {emitter.Neighbors.Count}");

            TestLog.State(
                $"Pipeline relationships = {selected.Count}");

            Assert.True(
                emitter.TryResolveTestEmission(
                    "signal",
                    pipeline,
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var value));

            TestLog.State(
                $"Resolved emission = {value}");

            Assert.Equal(
                2,
                selected.Count);

            Assert.Equal(
                3.0,
                value,
                10);

            TestLog.Pass(
                "Neighbor pipeline successfully defined emission topology.");

            WriteFinished(
                "TOPOLOGY PIPELINE");
        }

        [Fact]
        public void Complete_resolution_does_not_mutate_source_topology()
        {
            InitializeEmissionReport();

            TestLog.Section(
                "EMITTER GATE 3 - TOPOLOGY PRESERVATION");

            TestLog.Test(
                "Complete emission resolution preserves source Neighbor topology.");

            var component =
                new TestComponent(
                    "gate3.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["signal"] = 2.0
                        });

            var alpha =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.alpha");

            var beta =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate3.beta");

            emitter.Neighbors.Add(
                alpha,
                "alpha",
                1.0);

            emitter.Neighbors.Add(
                beta,
                "beta",
                2.0);

            var countBefore =
                emitter.Neighbors.Count;

            Assert.True(
                emitter.TryResolveTestEmission(
                    "signal",
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborCount(),
                    count => count,
                    new TestAdditiveEmissionComposition(),
                    out var value));

            var countAfter =
                emitter.Neighbors.Count;

            TestLog.State(
                $"Relationships before resolution = {countBefore}");

            TestLog.State(
                $"Relationships after resolution = {countAfter}");

            TestLog.State(
                $"Resolved emission = {value}");

            Assert.Equal(
                3.0,
                value,
                10);

            Assert.Equal(
                2,
                countAfter);

            Assert.True(
                emitter.Neighbors.Contains(
                    alpha.Id,
                    "alpha"));

            Assert.True(
                emitter.Neighbors.Contains(
                    beta.Id,
                    "beta"));

            TestLog.Pass(
                "Complete resolution preserved source topology.");

            WriteFinished(
                "TOPOLOGY PRESERVATION");
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
                $" EMITTER GATE 3 FINISHED - {certification}");

            _testOutputHelper.WriteLine(
                "============================================");

            _testOutputHelper.WriteLine(
                $" Report: {TestLog.Path}");

            _testOutputHelper.WriteLine("");
        }
    }
}