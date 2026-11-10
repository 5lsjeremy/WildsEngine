using System;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Tests.Entities;

public sealed class TestIdentifier :
    IdentifierBase
{
    public TestIdentifier(
        string slug)
        : base(
            slug)
    {
    }

    public TestIdentifier(
        Guid id,
        string slug)
        : base(
            id,
            slug)
    {
    }
}