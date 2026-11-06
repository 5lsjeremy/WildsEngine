using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Neighbors.Operators;
using WildsEngine.Core.Neighbors.Propagation;
using WildsEngine.Core.Processing;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests;

public sealed class ExtremeInputSafetyCertificationTests
{
    [Fact]
    public void PropagationRejectsNaN()
    {
        TestLog.Section(
            "EXTREME INPUT SAFETY - NAN REJECTION");

        TestLog.Test(
            "Propagation rejects NaN decay, intensity, distance, and eligibility values.");

        var topology =
            new NeighborTopologyFixture();

        var invalidDecay =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    new ExponentialDistancePropagation(
                        double.NaN));

        Assert.Equal(
            "decay",
            invalidDecay.ParamName);

        var propagation =
            new ExponentialDistancePropagation(
                0.5);

        var relationship =
            CreateRelationship(
                topology,
                topology.B,
                NeighborTopologyFixture.LocalMembership,
                1.0);

        var invalidIntensity =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    propagation.Propagate(
                        double.NaN,
                        relationship));

        Assert.Equal(
            "intensity",
            invalidIntensity.ParamName);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                topology.A.State.Process(
                    new AddNeighborInstruction(
                        topology.C,
                        NeighborTopologyFixture.LocalMembership,
                        double.NaN)));

        var invalidThreshold =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    new MinimumIntensityEligibility(
                        double.NaN));

        Assert.Equal(
            "minimumIntensity",
            invalidThreshold.ParamName);

        var eligibility =
            new MinimumIntensityEligibility(
                0.25);

        var invalidEligibilityIntensity =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    eligibility.IsEligible(
                        double.NaN));

        Assert.Equal(
            "intensity",
            invalidEligibilityIntensity.ParamName);

        TestLog.State(
            "NaN decay rejected = True");

        TestLog.State(
            "NaN propagation intensity rejected = True");

        TestLog.State(
            "NaN Neighbor distance rejected = True");

        TestLog.State(
            "NaN eligibility threshold rejected = True");

        TestLog.State(
            "NaN eligibility intensity rejected = True");

        TestLog.Pass(
            "NaN could not enter propagation through decay, intensity, distance, or eligibility.");
    }

    [Fact]
    public void PropagationRejectsPositiveAndNegativeInfinity()
    {
        TestLog.Section(
            "EXTREME INPUT SAFETY - INFINITY REJECTION");

        TestLog.Test(
            "Propagation rejects positive and negative infinity at every numeric input boundary.");

        var topology =
            new NeighborTopologyFixture();

        foreach (var nonFinite in
                 new[]
                 {
                     double.PositiveInfinity,
                     double.NegativeInfinity
                 })
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    new ExponentialDistancePropagation(
                        nonFinite));

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    topology.A.State.Process(
                        new AddNeighborInstruction(
                            topology.C,
                            NeighborTopologyFixture.LocalMembership,
                            nonFinite)));

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    new MinimumIntensityEligibility(
                        nonFinite));
        }

        var relationship =
            CreateRelationship(
                topology,
                topology.B,
                NeighborTopologyFixture.LocalMembership,
                1.0);

        var propagation =
            new ExponentialDistancePropagation(
                0.5);

        var eligibility =
            new MinimumIntensityEligibility(
                0.25);

        foreach (var nonFinite in
                 new[]
                 {
                     double.PositiveInfinity,
                     double.NegativeInfinity
                 })
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    propagation.Propagate(
                        nonFinite,
                        relationship));

            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    eligibility.IsEligible(
                        nonFinite));
        }

        TestLog.State(
            "Positive infinity rejected = True");

        TestLog.State(
            "Negative infinity rejected = True");

        TestLog.Pass(
            "Neither positive nor negative infinity could enter propagation.");
    }

    [Fact]
    public void PropagationRejectsNegativeDistanceWhereProhibited()
    {
        TestLog.Section(
            "EXTREME INPUT SAFETY - NEGATIVE DISTANCE");

        TestLog.Test(
            "Neighbor topology rejects negative distance before propagation.");

        var topology =
            new NeighborTopologyFixture();

        TestLog.Expected(
            "A negative Neighbor distance should be rejected rather than stored.");

        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    topology.A.State.Process(
                        new AddNeighborInstruction(
                            topology.B,
                            NeighborTopologyFixture.LocalMembership,
                            -1.0)));

        Assert.False(
            topology.A.Neighbors.TryGet(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership,
                out _));

        TestLog.State(
            $"Rejected parameter = {exception.ParamName}");

        TestLog.State(
            $"Relationship stored = {topology.A.Neighbors.Contains(topology.B.Id, NeighborTopologyFixture.LocalMembership)}");

        TestLog.Pass(
            "Negative distance was rejected before it could participate in propagation.");
    }

    [Fact]
    public void PropagationRejectsNegativeDecayWhereProhibited()
    {
        TestLog.Section(
            "EXTREME INPUT SAFETY - NEGATIVE DECAY");

        TestLog.Test(
            "Exponential propagation rejects negative decay.");

        TestLog.Expected(
            "Negative decay should not construct a propagation policy.");

        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    new ExponentialDistancePropagation(
                        -0.01));

        Assert.Equal(
            "decay",
            exception.ParamName);

        TestLog.State(
            $"Rejected parameter = {exception.ParamName}");

        TestLog.Pass(
            "Negative decay was rejected.");
    }

    [Fact]
    public void PropagationRejectsNegativeSourceIntensityWhereProhibited()
    {
        TestLog.Section(
            "EXTREME INPUT SAFETY - NEGATIVE SOURCE INTENSITY");

        TestLog.Test(
            "Exponential propagation rejects negative source intensity.");

        var topology =
            new NeighborTopologyFixture();

        var relationship =
            CreateRelationship(
                topology,
                topology.B,
                NeighborTopologyFixture.LocalMembership,
                1.0);

        var propagation =
            new ExponentialDistancePropagation(
                0.5);

        TestLog.Expected(
            "Negative source intensity should not produce propagated output.");

        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    propagation.Propagate(
                        -1.0,
                        relationship));

        Assert.Equal(
            "intensity",
            exception.ParamName);

        TestLog.State(
            $"Rejected parameter = {exception.ParamName}");

        TestLog.Pass(
            "Negative source intensity was rejected.");
    }

    [Fact]
    public void ExtremeFiniteInputsDoNotProduceNonFiniteOutput()
    {
        TestLog.Section(
            "EXTREME INPUT SAFETY - FINITE OVERFLOW CONTAINMENT");

        TestLog.Test(
            "Extreme finite propagation inputs remain finite after exponential attenuation.");

        var topology =
            new NeighborTopologyFixture();

        var extremeRelationship =
            CreateRelationship(
                topology,
                topology.B,
                NeighborTopologyFixture.LocalMembership,
                double.MaxValue);

        var extremePropagation =
            new ExponentialDistancePropagation(
                double.MaxValue);

        TestLog.Task(
            "Propagating maximum finite intensity through maximum finite decay and distance.");

        var fullyAttenuated =
            extremePropagation.Propagate(
                double.MaxValue,
                extremeRelationship);

        Assert.False(
            double.IsNaN(
                fullyAttenuated));

        Assert.False(
            double.IsInfinity(
                fullyAttenuated));

        Assert.Equal(
            0.0,
            fullyAttenuated);

        var zeroDecayTopology =
            new NeighborTopologyFixture();

        var zeroDecayRelationship =
            CreateRelationship(
                zeroDecayTopology,
                zeroDecayTopology.B,
                NeighborTopologyFixture.LocalMembership,
                double.MaxValue);

        var zeroDecayPropagation =
            new ExponentialDistancePropagation(
                0.0);

        TestLog.Task(
            "Propagating maximum finite intensity through zero decay at maximum finite distance.");

        var preserved =
            zeroDecayPropagation.Propagate(
                double.MaxValue,
                zeroDecayRelationship);

        Assert.False(
            double.IsNaN(
                preserved));

        Assert.False(
            double.IsInfinity(
                preserved));

        Assert.Equal(
            double.MaxValue,
            preserved);

        TestLog.State(
            $"Extreme attenuation result = {fullyAttenuated:R}");

        TestLog.State(
            $"Zero-decay extreme result = {preserved:R}");

        TestLog.Pass(
            "Extreme finite attenuation converged to finite zero.");

        TestLog.Pass(
            "Zero decay preserved the maximum finite source intensity without overflow.");
    }

    [Fact]
    public void EqualDistanceNeighborOrderingIsDeterministic()
    {
        TestLog.Section(
            "EXTREME INPUT SAFETY - DETERMINISTIC EQUAL-DISTANCE ORDER");

        TestLog.Test(
            "Equal-distance Neighbors are ordered by membership and then exact Neighbor identity.");

        var topology =
            new NeighborTopologyFixture();

        const double equalDistance =
            10.0;

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.D,
                    "beta",
                    equalDistance)));

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.C,
                    "alpha",
                    equalDistance)));

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    "alpha",
                    equalDistance)));

        var first =
            topology.A.ApplyNeighborOperator(
                new NearestNeighborOrder());

        var second =
            topology.A.ApplyNeighborOperator(
                new NearestNeighborOrder());

        Assert.Equal(
            3,
            first.Count);

        Assert.Equal(
            3,
            second.Count);

        Assert.Equal(
            "alpha",
            first[0].Membership);

        Assert.Equal(
            "alpha",
            first[1].Membership);

        Assert.Equal(
            "beta",
            first[2].Membership);

        var expectedFirstAlphaId =
            topology.B.Id.CompareTo(
                topology.C.Id) < 0
                ? topology.B.Id
                : topology.C.Id;

        var expectedSecondAlphaId =
            topology.B.Id.CompareTo(
                topology.C.Id) < 0
                ? topology.C.Id
                : topology.B.Id;

        Assert.Equal(
            expectedFirstAlphaId,
            first[0].Neighbor.Id);

        Assert.Equal(
            expectedSecondAlphaId,
            first[1].Neighbor.Id);

        for (var index = 0;
             index < first.Count;
             index++)
        {
            Assert.Equal(
                first[index].Membership,
                second[index].Membership);

            Assert.Equal(
                first[index].Neighbor.Id,
                second[index].Neighbor.Id);

            Assert.Equal(
                equalDistance,
                first[index].Distance,
                10);
        }

        var comparer =
            new NeighborDistanceAscendingComparer();

        Assert.True(
            comparer.Compare(
                topology.A,
                first[0],
                first[1]) < 0);

        Assert.True(
            comparer.Compare(
                topology.A,
                first[1],
                first[0]) > 0);

        Assert.True(
            comparer.Compare(
                topology.A,
                first[1],
                first[2]) < 0);

        TestLog.State(
            $"Ordered[0] = {first[0].Membership}/{first[0].Neighbor.Id}");

        TestLog.State(
            $"Ordered[1] = {first[1].Membership}/{first[1].Neighbor.Id}");

        TestLog.State(
            $"Ordered[2] = {first[2].Membership}/{first[2].Neighbor.Id}");

        TestLog.Pass(
            "Equal distance first deferred to ordinal membership.");

        TestLog.Pass(
            "Equal distance and membership then deferred to Neighbor identity.");

        TestLog.Pass(
            "Repeated ordering produced the same exact sequence.");
    }

    [Fact]
    public void ThresholdComparisonsRemainInclusiveAtExactBoundary()
    {
        TestLog.Section(
            "EXTREME INPUT SAFETY - INCLUSIVE THRESHOLD");

        TestLog.Test(
            "Minimum-intensity eligibility remains inclusive at the exact threshold.");

        const double threshold =
            0.25;

        var eligibility =
            new MinimumIntensityEligibility(
                threshold);

        var immediatelyBelow =
            Math.BitDecrement(
                threshold);

        var immediatelyAbove =
            Math.BitIncrement(
                threshold);

        TestLog.Task(
            "Testing the nearest representable values below, at, and above the threshold.");

        Assert.False(
            eligibility.IsEligible(
                immediatelyBelow));

        Assert.True(
            eligibility.IsEligible(
                threshold));

        Assert.True(
            eligibility.IsEligible(
                immediatelyAbove));

        TestLog.State(
            $"Immediately below = {immediatelyBelow:R}");

        TestLog.State(
            $"Exact threshold = {threshold:R}");

        TestLog.State(
            $"Immediately above = {immediatelyAbove:R}");

        TestLog.Pass(
            "Intensity immediately below the threshold was excluded.");

        TestLog.Pass(
            "Intensity exactly at the threshold was included.");

        TestLog.Pass(
            "Intensity immediately above the threshold was included.");
    }

    private static INeighborRelationship CreateRelationship(
        NeighborTopologyFixture topology,
        IBaseObject neighbor,
        string membership,
        double distance)
    {
        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    neighbor,
                    membership,
                    distance)));

        Assert.True(
            topology.A.Neighbors.TryGet(
                neighbor.Id,
                membership,
                out var relationship));

        Assert.NotNull(
            relationship);

        return relationship!;
    }
}
