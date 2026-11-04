using System;
using WildsEngine.Core.Neighbors.Propagation;
using WildsEngine.Core.Processing;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Logging;

namespace WildsEngine.Core.Tests;

public class NeighborPropagationTests
{
    [Fact]
    public void ExponentialPropagation_AttenuatesIntensity_ByNeighborDistance()
    {
        TestLog.Section(
            "NEIGHBOR PROPAGATION - EXPONENTIAL DISTANCE ATTENUATION");

        TestLog.Test(
            "Proving propagation intensity attenuates according to Neighbor distance.");

        var topology =
            new NeighborTopologyFixture();

        const double sourceIntensity =
            1.0;

        const double decay =
            0.5;

        TestLog.Task(
            "Creating A -> B at distance 1.");

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    1.0)));

        TestLog.Task(
            "Creating A -> C at distance 3.");

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.C,
                    NeighborTopologyFixture.LocalMembership,
                    3.0)));

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership,
                out var relationshipB));

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.C.Id,
                NeighborTopologyFixture.LocalMembership,
                out var relationshipC));

        Assert.NotNull(
            relationshipB);

        Assert.NotNull(
            relationshipC);

        var propagation =
            new ExponentialDistancePropagation(
                decay);

        TestLog.State(
            $"Source intensity = {sourceIntensity}");

        TestLog.State(
            $"Decay = {decay}");

        TestLog.State(
            $"B distance = {relationshipB!.Distance}");

        TestLog.State(
            $"C distance = {relationshipC!.Distance}");

        TestLog.Task(
            "Propagating source intensity independently to B and C.");

        var intensityB =
            propagation.Propagate(
                sourceIntensity,
                relationshipB);

        var intensityC =
            propagation.Propagate(
                sourceIntensity,
                relationshipC);

        var expectedB =
            sourceIntensity *
            Math.Exp(
                -decay *
                relationshipB.Distance);

        var expectedC =
            sourceIntensity *
            Math.Exp(
                -decay *
                relationshipC.Distance);

        TestLog.Expected(
            $"B intensity = {expectedB:R}");

        TestLog.Expected(
            $"C intensity = {expectedC:R}");

        TestLog.State(
            $"B received intensity = {intensityB:R}");

        TestLog.State(
            $"C received intensity = {intensityC:R}");

        Assert.Equal(
            expectedB,
            intensityB,
            12);

        Assert.Equal(
            expectedC,
            intensityC,
            12);

        Assert.True(
            intensityB <
            sourceIntensity);

        Assert.True(
            intensityC <
            intensityB);

        Assert.True(
            intensityC >
            0.0);

        TestLog.Pass(
            "Propagation attenuated monotonically with increasing Neighbor distance.");

        TestLog.Pass(
            "Exponential Neighbor propagation certification completed successfully.");
    }

    [Fact]
    public void ExponentialPropagation_ZeroDistance_PreservesIntensity()
    {
        TestLog.Section(
            "NEIGHBOR PROPAGATION - ZERO DISTANCE");

        var topology =
            new NeighborTopologyFixture();

        const double sourceIntensity =
            0.75;

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    0.0)));

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership,
                out var relationship));

        Assert.NotNull(
            relationship);

        var propagation =
            new ExponentialDistancePropagation(
                0.5);

        var result =
            propagation.Propagate(
                sourceIntensity,
                relationship!);

        TestLog.Expected(
            "Distance zero should preserve the complete source intensity.");

        TestLog.State(
            $"Source intensity = {sourceIntensity:R}");

        TestLog.State(
            $"Received intensity = {result:R}");

        Assert.Equal(
            sourceIntensity,
            result,
            12);

        TestLog.Pass(
            "Zero-distance propagation preserved source intensity.");
    }

    [Fact]
    public void ExponentialPropagation_ZeroDecay_PreservesIntensityAcrossDistance()
    {
        TestLog.Section(
            "NEIGHBOR PROPAGATION - ZERO DECAY");

        var topology =
            new NeighborTopologyFixture();

        const double sourceIntensity =
            0.75;

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    100.0)));

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership,
                out var relationship));

        Assert.NotNull(
            relationship);

        var propagation =
            new ExponentialDistancePropagation(
                0.0);

        var result =
            propagation.Propagate(
                sourceIntensity,
                relationship!);

        TestLog.Expected(
            "Zero decay should preserve intensity regardless of distance.");

        Assert.Equal(
            sourceIntensity,
            result,
            12);

        TestLog.State(
            $"Distance = {relationship.Distance}");

        TestLog.State(
            $"Received intensity = {result:R}");

        TestLog.Pass(
            "Zero-decay propagation preserved source intensity.");
    }

    [Fact]
    public void ExponentialPropagation_RejectsNegativeDecayAndIntensity()
    {
        TestLog.Section(
            "NEIGHBOR PROPAGATION - INVALID INPUT");

        TestLog.Test(
            "Proving propagation refuses invalid negative values.");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ExponentialDistancePropagation(
                    -0.1));

        TestLog.Pass(
            "Negative decay was rejected.");

        var topology =
            new NeighborTopologyFixture();

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    1.0)));

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership,
                out var relationship));

        Assert.NotNull(
            relationship);

        var propagation =
            new ExponentialDistancePropagation(
                0.5);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                propagation.Propagate(
                    -1.0,
                    relationship!));

        TestLog.Pass(
            "Negative source intensity was rejected.");

        TestLog.Pass(
            "Propagation input validation completed successfully.");
    }
    
    [Fact]
    public void MinimumIntensityEligibility_AcceptsIntensityAtOrAboveThreshold()
    {
        TestLog.Section(
            "NEIGHBOR PROPAGATION - ELIGIBILITY THRESHOLD");

        TestLog.Test(
            "Proving the minimum-intensity threshold is inclusive.");

        const double threshold =
            0.25;

        var eligibility =
            new MinimumIntensityEligibility(
                threshold);

        TestLog.Task(
            "Testing intensity above the threshold.");

        Assert.True(
            eligibility.IsEligible(
                0.50));

        TestLog.Pass(
            "Intensity above the threshold was eligible.");

        TestLog.Task(
            "Testing intensity exactly at the threshold.");

        TestLog.Expected(
            "An intensity equal to the threshold should be eligible.");

        Assert.True(
            eligibility.IsEligible(
                threshold));

        TestLog.Pass(
            "Threshold boundary was inclusive.");
    }

    [Fact]
    public void MinimumIntensityEligibility_RejectsIntensityBelowThreshold()
    {
        TestLog.Section(
            "NEIGHBOR PROPAGATION - BELOW ELIGIBILITY THRESHOLD");

        const double threshold =
            0.25;

        var eligibility =
            new MinimumIntensityEligibility(
                threshold);

        var intensity =
            Math.BitDecrement(
                threshold);

        TestLog.State(
            $"Threshold = {threshold:R}");

        TestLog.State(
            $"Test intensity = {intensity:R}");

        TestLog.Expected(
            "The nearest representable double below the threshold should not be eligible.");

        Assert.False(
            eligibility.IsEligible(
                intensity));

        TestLog.Pass(
            "Intensity immediately below the threshold was rejected.");
    }

    [Fact]
    public void MinimumIntensityEligibility_ZeroThreshold_AcceptsZeroIntensity()
    {
        TestLog.Section(
            "NEIGHBOR PROPAGATION - ZERO ELIGIBILITY THRESHOLD");

        TestLog.Test(
            "Proving zero remains a valid inclusive eligibility boundary.");

        var eligibility =
            new MinimumIntensityEligibility(
                0.0);

        TestLog.Expected(
            "Zero intensity should qualify when the minimum intensity is zero.");

        Assert.True(
            eligibility.IsEligible(
                0.0));

        Assert.True(
            eligibility.IsEligible(
                1.0));

        TestLog.Pass(
            "Zero-threshold eligibility behaved inclusively.");
    }

    [Fact]
    public void MinimumIntensityEligibility_RejectsNegativeThresholdAndIntensity()
    {
        TestLog.Section(
            "NEIGHBOR PROPAGATION - ELIGIBILITY INPUT VALIDATION");

        TestLog.Test(
            "Proving eligibility refuses invalid negative values.");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new MinimumIntensityEligibility(
                    -0.1));

        TestLog.Pass(
            "Negative eligibility threshold was rejected.");

        var eligibility =
            new MinimumIntensityEligibility(
                0.25);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                eligibility.IsEligible(
                    -0.1));

        TestLog.Pass(
            "Negative propagated intensity was rejected.");
    }

    [Fact]
    public void PropagatedIntensity_DeterminesNeighborEligibility()
    {
        TestLog.Section(
            "NEIGHBOR PROPAGATION - ATTENUATION TO ELIGIBILITY");

        TestLog.Test(
            "Proving certified topology and attenuation determine which Neighbors qualify.");

        var topology =
            new NeighborTopologyFixture();

        const double sourceIntensity =
            1.0;

        const double decay =
            0.5;

        const double threshold =
            0.25;

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    1.0)));

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.C,
                    NeighborTopologyFixture.LocalMembership,
                    3.0)));

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership,
                out var relationshipB));

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.C.Id,
                NeighborTopologyFixture.LocalMembership,
                out var relationshipC));

        Assert.NotNull(
            relationshipB);

        Assert.NotNull(
            relationshipC);

        var propagation =
            new ExponentialDistancePropagation(
                decay);

        var eligibility =
            new MinimumIntensityEligibility(
                threshold);

        var intensityB =
            propagation.Propagate(
                sourceIntensity,
                relationshipB!);

        var intensityC =
            propagation.Propagate(
                sourceIntensity,
                relationshipC!);

        TestLog.State(
            $"Source intensity = {sourceIntensity:R}");

        TestLog.State(
            $"Eligibility threshold = {threshold:R}");

        TestLog.State(
            $"B distance = {relationshipB.Distance:R}");

        TestLog.State(
            $"B received intensity = {intensityB:R}");

        TestLog.State(
            $"C distance = {relationshipC.Distance:R}");

        TestLog.State(
            $"C received intensity = {intensityC:R}");

        TestLog.Expected(
            "B should qualify and C should fall below the response threshold.");

        Assert.True(
            eligibility.IsEligible(
                intensityB));

        Assert.False(
            eligibility.IsEligible(
                intensityC));

        TestLog.Pass(
            "B was selected as an eligible propagation recipient.");

        TestLog.Pass(
            "C remained below the propagation eligibility threshold.");

        TestLog.Pass(
            "Topology -> propagation -> eligibility boundary completed successfully.");
    }
}