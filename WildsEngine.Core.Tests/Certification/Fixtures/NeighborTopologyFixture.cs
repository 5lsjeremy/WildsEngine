using WildsEngine.Tests.Registry;

namespace WildsEngine.Core.Tests.Fixtures;

public sealed class NeighborTopologyFixture
{
    public const string LocalMembership =
        "local";

    public const string RemoteMembership =
        "remote";

    public NeighborTopologyFixture()
    {
        A = new TestComponent(
            "topology.a");

        B = new TestComponent(
            "topology.b");

        C = new TestComponent(
            "topology.c");

        D = new TestComponent(
            "topology.d");
    }

    public TestComponent A { get; }

    public TestComponent B { get; }

    public TestComponent C { get; }

    public TestComponent D { get; }
}