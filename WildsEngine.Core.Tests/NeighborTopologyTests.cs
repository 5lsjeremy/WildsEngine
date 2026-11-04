using System.Linq;
using WildsEngine.Core.Neighbors.Operators;
using WildsEngine.Core.Processing;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Logging;

namespace WildsEngine.Core.Tests;

public class NeighborTopologyTests
{
    [Fact]
    public void Topology_CanMutateThroughState_AndBeQueriedThroughOperators()
    {
        TestLog.Section(
            "NEIGHBOR TOPOLOGY - STATE MUTATION AND OPERATOR QUERY");

        TestLog.Test(
            "Creating independent topology objects A, B, C, and D.");

        var topology =
            new NeighborTopologyFixture();

        TestLog.State(
            $"A = {topology.A.Slug}");

        TestLog.State(
            $"B = {topology.B.Slug}");

        TestLog.State(
            $"C = {topology.C.Slug}");

        TestLog.State(
            $"D = {topology.D.Slug}");

        TestLog.Task(
            "Building topology through A.State.Process().");

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    6.0)));

        TestLog.State(
            "A -> B | membership=local | distance=6");

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.C,
                    NeighborTopologyFixture.LocalMembership,
                    2.0)));

        TestLog.State(
            "A -> C | membership=local | distance=2");

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.D,
                    NeighborTopologyFixture.RemoteMembership,
                    1.0)));

        TestLog.State(
            "A -> D | membership=remote | distance=1");

        TestLog.Expected(
            "A should now contain exactly three Neighbor relationships.");

        Assert.Equal(
            3,
            topology.A.Neighbors.Count);

        Assert.True(
            topology.A.Neighbors.Contains(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership));

        Assert.True(
            topology.A.Neighbors.Contains(
                topology.C.Id,
                NeighborTopologyFixture.LocalMembership));

        Assert.True(
            topology.A.Neighbors.Contains(
                topology.D.Id,
                NeighborTopologyFixture.RemoteMembership));

        TestLog.Pass(
            "Topology established through the state machine.");

        var originalOrder =
            topology.A.Neighbors
                .Relationships
                .Select(
                    relationship =>
                        relationship.Neighbor.Id)
                .ToArray();

        TestLog.Task(
            "Querying topology through ApplyNeighborOperator().");

        var nearest =
            topology.A.ApplyNeighborOperator(
                new NearestNeighbor());

        var localNeighbors =
            topology.A.ApplyNeighborOperator(
                new MembershipNeighborFilter(
                    NeighborTopologyFixture.LocalMembership));

        var orderedNeighbors =
            topology.A.ApplyNeighborOperator(
                new NearestNeighborOrder());

        TestLog.Expected(
            "D should be the nearest Neighbor at distance 1.");

        Assert.NotNull(
            nearest);

        Assert.Equal(
            topology.D.Id,
            nearest!.Neighbor.Id);

        Assert.Equal(
            1.0,
            nearest.Distance);

        TestLog.State(
            $"Nearest Neighbor = {nearest.Neighbor.Slug} | distance={nearest.Distance}");

        TestLog.Expected(
            "The local membership should contain exactly B and C.");

        Assert.Equal(
            2,
            localNeighbors.Count);

        Assert.All(
            localNeighbors,
            relationship =>
                Assert.Equal(
                    NeighborTopologyFixture.LocalMembership,
                    relationship.Membership));

        TestLog.State(
            $"Local Neighbor count = {localNeighbors.Count}");

        Assert.Equal(
            topology.D.Id,
            orderedNeighbors[0].Neighbor.Id);

        TestLog.Pass(
            "Topology operators returned the expected initial results.");

        TestLog.Task(
            "Verifying query operators did not mutate source topology.");

        Assert.Equal(
            originalOrder,
            topology.A.Neighbors
                .Relationships
                .Select(
                    relationship =>
                        relationship.Neighbor.Id)
                .ToArray());

        Assert.Equal(
            3,
            topology.A.Neighbors.Count);

        TestLog.Pass(
            "Operator queries preserved source topology.");

        TestLog.Task(
            "Moving B from distance 6 to distance 0.5 through the state machine.");

        Assert.True(
            topology.A.State.Process(
                new SetNeighborDistanceInstruction(
                    topology.B.Id,
                    NeighborTopologyFixture.LocalMembership,
                    0.5)));

        var nearestAfterDistanceChange =
            topology.A.ApplyNeighborOperator(
                new NearestNeighbor());

        TestLog.Expected(
            "B should now replace D as the nearest Neighbor.");

        Assert.NotNull(
            nearestAfterDistanceChange);

        Assert.Equal(
            topology.B.Id,
            nearestAfterDistanceChange!.Neighbor.Id);

        Assert.Equal(
            0.5,
            nearestAfterDistanceChange.Distance);

        TestLog.State(
            $"Nearest Neighbor = {nearestAfterDistanceChange.Neighbor.Slug} | distance={nearestAfterDistanceChange.Distance}");

        TestLog.Pass(
            "Topology operators immediately observed the state-driven distance change.");

        TestLog.Task(
            "Removing B through the state machine.");

        Assert.True(
            topology.A.State.Process(
                new RemoveNeighborInstruction(
                    topology.B.Id,
                    NeighborTopologyFixture.LocalMembership)));

        Assert.False(
            topology.A.Neighbors.Contains(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership));

        Assert.Equal(
            2,
            topology.A.Neighbors.Count);

        TestLog.State(
            $"Neighbor count after B removal = {topology.A.Neighbors.Count}");

        var nearestAfterRemoval =
            topology.A.ApplyNeighborOperator(
                new NearestNeighbor());

        TestLog.Expected(
            "D should become the nearest Neighbor again after B is removed.");

        Assert.NotNull(
            nearestAfterRemoval);

        Assert.Equal(
            topology.D.Id,
            nearestAfterRemoval!.Neighbor.Id);

        TestLog.State(
            $"Nearest Neighbor = {nearestAfterRemoval.Neighbor.Slug} | distance={nearestAfterRemoval.Distance}");

        TestLog.Pass(
            "Removal was immediately reflected by subsequent topology computation.");

        TestLog.Task(
            "Running repeated topology queries and checking source integrity.");

        var countBeforeFinalQueries =
            topology.A.Neighbors.Count;

        _ =
            topology.A.ApplyNeighborOperator(
                new NearestNeighborOrder());

        _ =
            topology.A.ApplyNeighborOperator(
                new MembershipNeighborFilter(
                    NeighborTopologyFixture.LocalMembership));

        _ =
            topology.A.ApplyNeighborOperator(
                new NeighborCount());

        Assert.Equal(
            countBeforeFinalQueries,
            topology.A.Neighbors.Count);

        TestLog.Pass(
            "Repeated topology queries did not mutate the Neighbor collection.");

        TestLog.Pass(
            "Neighbor topology vertical-slice certification completed successfully.");
    }
    
    [Fact]
    public void Topology_DuplicateRelationship_IsRejectedDeterministically()
    {
        TestLog.Section(
            "NEIGHBOR TOPOLOGY - DUPLICATE RELATIONSHIP IDENTITY");

        TestLog.Test(
            "Proving Neighbor.Id + Membership defines relationship identity.");

        var topology =
            new NeighborTopologyFixture();

        TestLog.Task(
            "Adding A -> B as a local Neighbor at distance 6.");

        var firstAdd =
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    6.0));

        Assert.True(
            firstAdd);

        Assert.Equal(
            1,
            topology.A.Neighbors.Count);

        TestLog.Pass(
            "Initial relationship was accepted.");

        TestLog.Task(
            "Attempting to add the same Neighbor and membership again.");

        var duplicateAdd =
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    2.0));

        TestLog.Expected(
            "The duplicate relationship should be rejected and the original relationship preserved.");

        Assert.False(
            duplicateAdd);

        Assert.Equal(
            1,
            topology.A.Neighbors.Count);

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership,
                out var relationship));

        Assert.NotNull(
            relationship);

        Assert.Equal(
            6.0,
            relationship!.Distance);

        TestLog.State(
            $"Relationship count = {topology.A.Neighbors.Count}");

        TestLog.State(
            $"Preserved distance = {relationship.Distance}");

        TestLog.Pass(
            "Duplicate relationship was rejected without changing existing relationship state.");
    }

    [Fact]
    public void Topology_SameNeighbor_CanParticipateInDistinctMemberships()
    {
        TestLog.Section(
            "NEIGHBOR TOPOLOGY - DISTINCT MEMBERSHIPS");

        TestLog.Test(
            "Proving one Neighbor may participate through more than one membership.");

        var topology =
            new NeighborTopologyFixture();

        TestLog.Task(
            "Adding B to A through the local membership.");

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    6.0)));

        TestLog.Task(
            "Adding the same B to A through the remote membership.");

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.RemoteMembership,
                    12.0)));

        TestLog.Expected(
            "A should contain two relationships to B distinguished by membership.");

        Assert.Equal(
            2,
            topology.A.Neighbors.Count);

        Assert.True(
            topology.A.Neighbors.Contains(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership));

        Assert.True(
            topology.A.Neighbors.Contains(
                topology.B.Id,
                NeighborTopologyFixture.RemoteMembership));

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership,
                out var localRelationship));

        Assert.True(
            topology.A.Neighbors.TryGet(
                topology.B.Id,
                NeighborTopologyFixture.RemoteMembership,
                out var remoteRelationship));

        Assert.NotNull(
            localRelationship);

        Assert.NotNull(
            remoteRelationship);

        Assert.Equal(
            6.0,
            localRelationship!.Distance);

        Assert.Equal(
            12.0,
            remoteRelationship!.Distance);

        TestLog.State(
            $"B local distance = {localRelationship.Distance}");

        TestLog.State(
            $"B remote distance = {remoteRelationship.Distance}");

        TestLog.Task(
            "Removing only the local relationship.");

        Assert.True(
            topology.A.State.Process(
                new RemoveNeighborInstruction(
                    topology.B.Id,
                    NeighborTopologyFixture.LocalMembership)));

        TestLog.Expected(
            "Removing one membership must not remove another membership to the same Neighbor.");

        Assert.False(
            topology.A.Neighbors.Contains(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership));

        Assert.True(
            topology.A.Neighbors.Contains(
                topology.B.Id,
                NeighborTopologyFixture.RemoteMembership));

        Assert.Equal(
            1,
            topology.A.Neighbors.Count);

        TestLog.Pass(
            "Membership identities remained independent.");
    }

    [Fact]
    public void Topology_OperatorPipeline_IsDeterministic_AndPreservesSource()
    {
        TestLog.Section(
            "NEIGHBOR TOPOLOGY - OPERATOR PIPELINE DETERMINISM");

        TestLog.Test(
            "Proving composed topology operations are deterministic and source-safe.");

        var topology =
            new NeighborTopologyFixture();

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.B,
                    NeighborTopologyFixture.LocalMembership,
                    6.0)));

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.C,
                    NeighborTopologyFixture.LocalMembership,
                    2.0)));

        Assert.True(
            topology.A.State.Process(
                new AddNeighborInstruction(
                    topology.D,
                    NeighborTopologyFixture.RemoteMembership,
                    1.0)));

        var sourceBefore =
            topology.A.Neighbors
                .Relationships
                .Select(
                    relationship =>
                        (
                            relationship.Neighbor.Id,
                            relationship.Membership,
                            relationship.Distance))
                .ToArray();

        TestLog.State(
            $"Source relationship count = {sourceBefore.Length}");

        TestLog.Task(
            "Building local -> nearest-first topology pipeline.");

        var pipeline =
            new NeighborPipeline(
                new MembershipNeighborFilter(
                    NeighborTopologyFixture.LocalMembership),
                new NearestNeighborOrder());

        var first =
            topology.A.ApplyNeighborOperator(
                pipeline);

        var second =
            topology.A.ApplyNeighborOperator(
                pipeline);

        TestLog.Expected(
            "Both executions should produce C then B.");

        Assert.Equal(
            2,
            first.Count);

        Assert.Equal(
            2,
            second.Count);

        Assert.Equal(
            topology.C.Id,
            first[0].Neighbor.Id);

        Assert.Equal(
            topology.B.Id,
            first[1].Neighbor.Id);

        Assert.Equal(
            first.Select(
                relationship =>
                    relationship.Neighbor.Id),
            second.Select(
                relationship =>
                    relationship.Neighbor.Id));

        TestLog.State(
            $"Pipeline result 1 = {first[0].Neighbor.Slug} -> {first[1].Neighbor.Slug}");

        TestLog.State(
            $"Pipeline result 2 = {second[0].Neighbor.Slug} -> {second[1].Neighbor.Slug}");

        TestLog.Task(
            "Comparing source topology before and after pipeline execution.");

        var sourceAfter =
            topology.A.Neighbors
                .Relationships
                .Select(
                    relationship =>
                        (
                            relationship.Neighbor.Id,
                            relationship.Membership,
                            relationship.Distance))
                .ToArray();

        Assert.Equal(
            sourceBefore,
            sourceAfter);

        Assert.Equal(
            3,
            topology.A.Neighbors.Count);

        TestLog.Pass(
            "Operator pipeline was deterministic.");

        TestLog.Pass(
            "Operator pipeline preserved the source Neighbor topology.");
    }
    
    [Fact]
public void Topology_Relationships_AreDirectional_AndDoNotImplyReciprocity()
{
    TestLog.Section(
        "NEIGHBOR TOPOLOGY - DIRECTIONAL INDEPENDENCE");

    TestLog.Test(
        "Proving A -> B does not implicitly create B -> A.");

    var topology =
        new NeighborTopologyFixture();

    TestLog.Task(
        "Creating A -> B through the local membership.");

    Assert.True(
        topology.A.State.Process(
            new AddNeighborInstruction(
                topology.B,
                NeighborTopologyFixture.LocalMembership,
                4.0)));

    TestLog.Expected(
        "A should contain B, while B should remain topologically independent.");

    Assert.True(
        topology.A.Neighbors.Contains(
            topology.B.Id,
            NeighborTopologyFixture.LocalMembership));

    Assert.False(
        topology.B.Neighbors.Contains(
            topology.A.Id,
            NeighborTopologyFixture.LocalMembership));

    Assert.Equal(
        1,
        topology.A.Neighbors.Count);

    Assert.Equal(
        0,
        topology.B.Neighbors.Count);

    TestLog.State(
        $"A Neighbor count = {topology.A.Neighbors.Count}");

    TestLog.State(
        $"B Neighbor count = {topology.B.Neighbors.Count}");

    TestLog.Pass(
        "A -> B did not implicitly create B -> A.");

    TestLog.Task(
        "Creating an explicit B -> A relationship with independent distance.");

    Assert.True(
        topology.B.State.Process(
            new AddNeighborInstruction(
                topology.A,
                NeighborTopologyFixture.LocalMembership,
                9.0)));

    TestLog.Expected(
        "A -> B and B -> A should now exist as independent relationships.");

    Assert.True(
        topology.A.Neighbors.TryGet(
            topology.B.Id,
            NeighborTopologyFixture.LocalMembership,
            out var aToB));

    Assert.True(
        topology.B.Neighbors.TryGet(
            topology.A.Id,
            NeighborTopologyFixture.LocalMembership,
            out var bToA));

    Assert.NotNull(
        aToB);

    Assert.NotNull(
        bToA);

    Assert.Equal(
        4.0,
        aToB!.Distance);

    Assert.Equal(
        9.0,
        bToA!.Distance);

    TestLog.State(
        $"A -> B distance = {aToB.Distance}");

    TestLog.State(
        $"B -> A distance = {bToA.Distance}");

    TestLog.Pass(
        "Reciprocal topology can exist without sharing relationship state.");

    TestLog.Task(
        "Changing A -> B without modifying B -> A.");

    Assert.True(
        topology.A.State.Process(
            new SetNeighborDistanceInstruction(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership,
                1.5)));

    Assert.True(
        topology.A.Neighbors.TryGet(
            topology.B.Id,
            NeighborTopologyFixture.LocalMembership,
            out aToB));

    Assert.True(
        topology.B.Neighbors.TryGet(
            topology.A.Id,
            NeighborTopologyFixture.LocalMembership,
            out bToA));

    Assert.NotNull(
        aToB);

    Assert.NotNull(
        bToA);

    Assert.Equal(
        1.5,
        aToB!.Distance);

    Assert.Equal(
        9.0,
        bToA!.Distance);

    TestLog.State(
        $"Updated A -> B distance = {aToB.Distance}");

    TestLog.State(
        $"Preserved B -> A distance = {bToA.Distance}");

    TestLog.Pass(
        "Directional relationship state remained independent.");

    TestLog.Task(
        "Removing A -> B while preserving B -> A.");

    Assert.True(
        topology.A.State.Process(
            new RemoveNeighborInstruction(
                topology.B.Id,
                NeighborTopologyFixture.LocalMembership)));

    Assert.False(
        topology.A.Neighbors.Contains(
            topology.B.Id,
            NeighborTopologyFixture.LocalMembership));

    Assert.True(
        topology.B.Neighbors.Contains(
            topology.A.Id,
            NeighborTopologyFixture.LocalMembership));

    Assert.Equal(
        0,
        topology.A.Neighbors.Count);

    Assert.Equal(
        1,
        topology.B.Neighbors.Count);

    TestLog.Pass(
        "Removing one direction did not alter the reciprocal relationship.");

    TestLog.Pass(
        "Neighbor topology directional-independence certification completed successfully.");
}
}