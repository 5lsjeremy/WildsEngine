using System;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Tests.Entities;

public sealed class TestEntity :
    EntityBase
{
    public TestEntity(
        string slug)
        : base(
            slug)
    {
    }

    public TestEntity(
        Guid id,
        string slug)
        : base(
            id,
            slug)
    {
    }

    public bool RegisterFact(
        IFact fact)
    {
        return AddFact(
            fact);
    }

    public bool UnregisterFact(
        IFact fact)
    {
        return RemoveFact(
            fact);
    }
}