using System;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Tests.Entities;

public sealed class TestDiscovery :
    DiscoveryBase
{
    public TestDiscovery(
        string slug,
        IFact fact,
        bool isAvailable = false)
        : base(
            slug,
            fact,
            isAvailable)
    {
    }

    public TestDiscovery(
        Guid id,
        string slug,
        IFact fact,
        bool isAvailable = false)
        : base(
            id,
            slug,
            fact,
            isAvailable)
    {
    }

    public void SetAvailable()
    {
        MakeAvailable();
    }

    public void SetUnavailable()
    {
        MakeUnavailable();
    }
}