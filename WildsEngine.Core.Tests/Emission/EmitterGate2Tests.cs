using System.Collections.Generic;
using WildsEngine.Core.Neighbors.Operators;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Emission
{
    /// <summary>
    /// Gate 2 certification for topology-derived emission input.
    ///
    /// This gate proves:
    ///
    /// consumer data
    ///     -> base emission scalar
    ///
    /// current Neighbor topology
    ///     -> membership selection
    ///     -> scalar operator
    ///     -> topology scalar
    ///
    /// Composition between those scalars is intentionally
    /// outside this gate.
    /// </summary>
    public sealed class EmitterGate2Tests
    {
        [Fact]
        public void Membership_topology_resolves_to_scalar()
        {
            var component =
                new TestComponent(
                    "gate2.component");

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
                    "gate2.alpha.a");

            var alphaB =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.alpha.b");

            var beta =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.beta");

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

            Assert.True(
                emitter.TryEmit(
                    "signal",
                    out var baseEmission));

            var topologyScalar =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborCount(),
                    count => count);

            Assert.Equal(
                1.6,
                baseEmission,
                10);

            Assert.Equal(
                2.0,
                topologyScalar,
                10);
        }

        [Fact]
        public void Different_memberships_resolve_independently()
        {
            var component =
                new TestComponent(
                    "gate2.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var first =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.first");

            var second =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.second");

            var third =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.third");

            emitter.Neighbors.Add(
                first,
                "alpha",
                1.0);

            emitter.Neighbors.Add(
                second,
                "alpha",
                2.0);

            emitter.Neighbors.Add(
                third,
                "beta",
                3.0);

            var alphaScalar =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborCount(),
                    count => count);

            var betaScalar =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "beta"),
                    new NeighborCount(),
                    count => count);

            Assert.Equal(
                2.0,
                alphaScalar,
                10);

            Assert.Equal(
                1.0,
                betaScalar,
                10);
        }

        [Fact]
        public void Missing_membership_resolves_to_zero_count()
        {
            var component =
                new TestComponent(
                    "gate2.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var neighbor =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.neighbor");

            emitter.Neighbors.Add(
                neighbor,
                "alpha",
                1.0);

            var topologyScalar =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "unknown"),
                    new NeighborCount(),
                    count => count);

            Assert.Equal(
                0.0,
                topologyScalar,
                10);
        }

        [Fact]
        public void Membership_change_changes_topology_scalar()
        {
            var component =
                new TestComponent(
                    "gate2.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var objectA =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.object.a");

            var objectB =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.object.b");

            emitter.Neighbors.Add(
                objectA,
                "stock",
                1.0);

            emitter.Neighbors.Add(
                objectB,
                "stock",
                1.0);

            var before =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "stock"),
                    new NeighborCount(),
                    count => count);

            Assert.True(
                emitter.Neighbors.Remove(
                    objectB.Id,
                    "stock"));

            Assert.True(
                emitter.Neighbors.Add(
                    objectB,
                    "reduced",
                    1.0));

            var stockAfter =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "stock"),
                    new NeighborCount(),
                    count => count);

            var reducedAfter =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "reduced"),
                    new NeighborCount(),
                    count => count);

            Assert.Equal(
                2.0,
                before,
                10);

            Assert.Equal(
                1.0,
                stockAfter,
                10);

            Assert.Equal(
                1.0,
                reducedAfter,
                10);
        }

        [Fact]
        public void Same_object_can_contribute_through_multiple_memberships()
        {
            var component =
                new TestComponent(
                    "gate2.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var objectA =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.object.a");

            emitter.Neighbors.Add(
                objectA,
                "stock",
                1.0);

            emitter.Neighbors.Add(
                objectA,
                "sale",
                2.0);

            emitter.Neighbors.Add(
                objectA,
                "reduced",
                3.0);

            var stock =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "stock"),
                    new NeighborCount(),
                    count => count);

            var sale =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "sale"),
                    new NeighborCount(),
                    count => count);

            var reduced =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "reduced"),
                    new NeighborCount(),
                    count => count);

            Assert.Equal(
                1.0,
                stock,
                10);

            Assert.Equal(
                1.0,
                sale,
                10);

            Assert.Equal(
                1.0,
                reduced,
                10);
        }

        [Fact]
        public void Topology_scalar_resolution_does_not_mutate_neighbors()
        {
            var component =
                new TestComponent(
                    "gate2.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var alpha =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.alpha");

            var beta =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2.beta");

            emitter.Neighbors.Add(
                alpha,
                "alpha",
                1.0);

            emitter.Neighbors.Add(
                beta,
                "beta",
                2.0);

            var before =
                emitter.Neighbors.Relationships;

            var topologyScalar =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "alpha"),
                    new NeighborCount(),
                    count => count);

            var after =
                emitter.Neighbors.Relationships;

            Assert.Equal(
                1.0,
                topologyScalar,
                10);

            Assert.Equal(
                2,
                before.Count);

            Assert.Equal(
                2,
                after.Count);

            Assert.True(
                emitter.Neighbors.Contains(
                    alpha.Id,
                    "alpha"));

            Assert.True(
                emitter.Neighbors.Contains(
                    beta.Id,
                    "beta"));
        }
    }
}