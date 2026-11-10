using System;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Tests.Entities;

public sealed class TestDiscovery :
    DiscoveryBase
{
    public TestDiscovery(
        string slug,
        IFact fact)
        : base(
            slug,
            fact)
    {
    }

    public TestDiscovery(
        Guid id,
        string slug,
        IFact fact)
        : base(
            id,
            slug,
            fact)
    {
    }
}