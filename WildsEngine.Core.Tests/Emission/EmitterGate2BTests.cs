using System;
using System.Collections.Generic;
using WildsEngine.Core.Neighbors.Operators;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Emission
{
    /// <summary>
    /// Gate 2B certification for emission composition.
    ///
    /// This gate proves:
    ///
    /// consumer data
    ///     -> base emission scalar
    ///
    /// topology
    ///     -> membership selection
    ///     -> topology scalar
    ///
    /// base scalar + topology scalar
    ///     -> supplied composition policy
    ///     -> resolved emission scalar
    ///
    /// The emitter does not own the mathematical semantics
    /// of composition.
    /// </summary>
    public sealed class EmitterGate2BTests
    {
        [Fact]
        public void Base_and_topology_scalars_are_composed_by_supplied_policy()
        {
            var component =
                new TestComponent(
                    "gate2b.component");

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
                    "gate2b.alpha.a");

            var alphaB =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2b.alpha.b");

            var beta =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2b.beta");

            emitter.Neighbors.Add(
                alphaA,
                "alpha",
                1.0);

            emitter.Neighbors.Add(
                alphaB,
                "alpha",
                2.0);

            emitter.Neighbors.Add(
                beta,
                "beta",
                1.0);

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

            var resolved =
                emitter.ComposeTestEmission(
                    baseEmission,
                    topologyScalar,
                    new TestAdditiveEmissionComposition());

            Assert.Equal(
                1.6,
                baseEmission,
                10);

            Assert.Equal(
                2.0,
                topologyScalar,
                10);

            Assert.Equal(
                3.6,
                resolved,
                10);
        }

        [Fact]
        public void Membership_change_changes_composed_emission()
        {
            var component =
                new TestComponent(
                    "gate2b.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["signal"] = 1.6
                        });

            var objectA =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2b.object.a");

            var objectB =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate2b.object.b");

            emitter.Neighbors.Add(
                objectA,
                "active",
                1.0);

            emitter.Neighbors.Add(
                objectB,
                "active",
                1.0);

            Assert.True(
                emitter.TryEmit(
                    "signal",
                    out var baseEmission));

            var composition =
                new TestAdditiveEmissionComposition();

            var topologyBefore =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "active"),
                    new NeighborCount(),
                    count => count);

            var resolvedBefore =
                emitter.ComposeTestEmission(
                    baseEmission,
                    topologyBefore,
                    composition);

            Assert.True(
                emitter.Neighbors.Remove(
                    objectB.Id,
                    "active"));

            Assert.True(
                emitter.Neighbors.Add(
                    objectB,
                    "inactive",
                    1.0));

            var topologyAfter =
                emitter.ResolveTestTopologyScalar(
                    new MembershipNeighborFilter(
                        "active"),
                    new NeighborCount(),
                    count => count);

            var resolvedAfter =
                emitter.ComposeTestEmission(
                    baseEmission,
                    topologyAfter,
                    composition);

            Assert.Equal(
                2.0,
                topologyBefore,
                10);

            Assert.Equal(
                1.0,
                topologyAfter,
                10);

            Assert.Equal(
                3.6,
                resolvedBefore,
                10);

            Assert.Equal(
                2.6,
                resolvedAfter,
                10);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Non_finite_base_emission_is_rejected(
            double emission)
        {
            var component =
                new TestComponent(
                    "gate2b.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    emitter.ComposeTestEmission(
                        emission,
                        1.0,
                        new TestAdditiveEmissionComposition());
                });
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Non_finite_topology_scalar_is_rejected(
            double topology)
        {
            var component =
                new TestComponent(
                    "gate2b.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                {
                    emitter.ComposeTestEmission(
                        1.0,
                        topology,
                        new TestAdditiveEmissionComposition());
                });
        }

        [Fact]
        public void Non_finite_composition_result_is_rejected()
        {
            var component =
                new TestComponent(
                    "gate2b.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            Assert.Throws<ArithmeticException>(
                () =>
                {
                    emitter.ComposeTestEmission(
                        double.MaxValue,
                        double.MaxValue,
                        new TestAdditiveEmissionComposition());
                });
        }
    }
}