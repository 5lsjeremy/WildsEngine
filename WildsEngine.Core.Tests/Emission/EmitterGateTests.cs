using System;
using System.Collections.Generic;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Emission
{
    /// <summary>
    /// Gate 1 certification for the Wilds emitter surface.
    ///
    /// This gate proves:
    ///
    /// consumer data
    ///     -> emitter
    ///     -> scalar emission
    ///
    /// while preserving ordinary Wilds identity and topology.
    ///
    /// Propagation, topology-derived scalar influence,
    /// receiver eligibility, and state emission are intentionally
    /// outside this gate.
    /// </summary>
    public sealed class EmitterGate1Tests
    {
        [Fact]
        public void Single_numeric_property_resolves_to_scalar()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["appetite"] = 1.6
                        });

            var emitted =
                emitter.TryEmit(
                    "appetite",
                    out var value);

            Assert.True(
                emitted);

            Assert.Equal(
                1.6,
                value,
                10);
        }

        [Fact]
        public void Named_group_resolves_to_scalar_dictionary()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    groups:
                        new Dictionary<
                            string,
                            IReadOnlyDictionary<
                                string,
                                object?>>
                        {
                            ["food"] =
                                new Dictionary<
                                    string,
                                    object?>
                                {
                                    ["appetite"] = 1.6,
                                    ["hunger-reducer"] = 1.4
                                }
                        });

            var emitted =
                emitter.TryEmitGroup(
                    "food",
                    out var values);

            Assert.True(
                emitted);

            Assert.Equal(
                2,
                values.Count);

            Assert.Equal(
                1.6,
                values["appetite"],
                10);

            Assert.Equal(
                1.4,
                values["hunger-reducer"],
                10);
        }

        [Fact]
        public void Missing_property_does_not_emit()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var emitted =
                emitter.TryEmit(
                    "unknown",
                    out var value);

            Assert.False(
                emitted);

            Assert.Equal(
                0.0,
                value);
        }

        [Fact]
        public void Missing_group_does_not_emit()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var emitted =
                emitter.TryEmitGroup(
                    "unknown",
                    out var values);

            Assert.False(
                emitted);

            Assert.Empty(
                values);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Non_finite_scalar_is_rejected(
            double rawValue)
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["value"] = rawValue
                        });

            Assert.Throws<
                ArgumentOutOfRangeException>(
                () =>
                {
                    emitter.TryEmit(
                        "value",
                        out _);
                });
        }

        [Fact]
        public void Non_numeric_property_is_not_silently_converted()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["value"] = "not-a-number"
                        });

            Assert.Throws<
                ArgumentException>(
                () =>
                {
                    emitter.TryEmit(
                        "value",
                        out _);
                });
        }

        [Fact]
        public void Emitter_has_wilds_identity()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            Assert.NotEqual(
                Guid.Empty,
                emitter.Id);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    emitter.Slug));

            Assert.Same(
                component,
                emitter.Component);
        }

        [Fact]
        public void Emitter_owns_ordinary_neighbor_topology()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var neighbor =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate1.neighbor");

            var added =
                emitter.Neighbors.Add(
                    neighbor,
                    "alpha",
                    1.0);

            Assert.True(
                added);

            Assert.Equal(
                1,
                emitter.Neighbors.Count);

            Assert.True(
                emitter.Neighbors.Contains(
                    neighbor.Id,
                    "alpha"));
        }

        [Fact]
        public void Same_neighbor_can_participate_in_multiple_memberships()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var neighbor =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate1.neighbor");

            Assert.True(
                emitter.Neighbors.Add(
                    neighbor,
                    "alpha",
                    1.0));

            Assert.True(
                emitter.Neighbors.Add(
                    neighbor,
                    "beta",
                    2.0));

            Assert.True(
                emitter.Neighbors.Add(
                    neighbor,
                    "gamma",
                    3.0));

            Assert.Equal(
                3,
                emitter.Neighbors.Count);

            Assert.True(
                emitter.Neighbors.Contains(
                    neighbor.Id,
                    "alpha"));

            Assert.True(
                emitter.Neighbors.Contains(
                    neighbor.Id,
                    "beta"));

            Assert.True(
                emitter.Neighbors.Contains(
                    neighbor.Id,
                    "gamma"));
        }

        [Fact]
        public void Membership_can_be_removed_without_affecting_others()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var neighbor =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate1.neighbor");

            emitter.Neighbors.Add(
                neighbor,
                "alpha",
                1.0);

            emitter.Neighbors.Add(
                neighbor,
                "beta",
                2.0);

            emitter.Neighbors.Add(
                neighbor,
                "gamma",
                3.0);

            var removed =
                emitter.Neighbors.Remove(
                    neighbor.Id,
                    "beta");

            Assert.True(
                removed);

            Assert.Equal(
                2,
                emitter.Neighbors.Count);

            Assert.True(
                emitter.Neighbors.Contains(
                    neighbor.Id,
                    "alpha"));

            Assert.False(
                emitter.Neighbors.Contains(
                    neighbor.Id,
                    "beta"));

            Assert.True(
                emitter.Neighbors.Contains(
                    neighbor.Id,
                    "gamma"));
        }

        [Fact]
        public void Relationship_snapshot_matches_current_topology()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var neighbor =
                WildsFixtureFactory.CreateTopologyObject(
                    component,
                    "gate1.neighbor");

            emitter.Neighbors.Add(
                neighbor,
                "alpha",
                1.0);

            var snapshot =
                emitter.Neighbors.Relationships;

            Assert.Single(
                snapshot);

            Assert.Equal(
                "alpha",
                snapshot[0].Membership);

            Assert.Same(
                neighbor,
                snapshot[0].Neighbor);

            Assert.Equal(
                1.0,
                snapshot[0].Distance,
                10);
        }

        [Fact]
        public void Reading_emission_does_not_change_consumer_data()
        {
            var properties =
                new Dictionary<string, object?>
                {
                    ["appetite"] = 1.6
                };

            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        properties);

            Assert.True(
                emitter.TryEmit(
                    "appetite",
                    out var value));

            Assert.Equal(
                1.6,
                value,
                10);

            Assert.Equal(
                1.6,
                properties["appetite"]);
        }

        [Fact]
        public void Arbitrary_consumer_vocabulary_requires_no_wilds_types()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component,
                    properties:
                        new Dictionary<string, object?>
                        {
                            ["nocturnal-fungal-fissure"] = 7.25
                        });

            Assert.True(
                emitter.TryEmit(
                    "nocturnal-fungal-fissure",
                    out var value));

            Assert.Equal(
                7.25,
                value,
                10);
        }

        [Fact]
        public void Cheese_can_participate_in_multiple_memberships()
        {
            var component =
                new TestComponent(
                    "gate1.component");

            var emitter =
                WildsFixtureFactory.CreateEmitter(
                    component);

            var cheese =
                WildsFixtureFactory.CreateCheese(
                    component);

            Assert.True(
                emitter.Neighbors.Add(
                    cheese,
                    "delicious",
                    1.0));

            Assert.True(
                emitter.Neighbors.Add(
                    cheese,
                    "reduced",
                    2.0));

            Assert.Equal(
                2,
                emitter.Neighbors.Count);

            Assert.True(
                emitter.Neighbors.Contains(
                    cheese.Id,
                    "delicious"));

            Assert.True(
                emitter.Neighbors.Contains(
                    cheese.Id,
                    "reduced"));
        }
    }
}