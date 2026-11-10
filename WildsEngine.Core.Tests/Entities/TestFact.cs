using System;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Tests.Entities;

public sealed class TestFact :
    FactBase
{
    public TestFact(
        string slug,
        IEntity entity,
        string path,
        object? value)
        : base(
            slug,
            entity,
            path,
            value)
    {
    }

    public TestFact(
        Guid id,
        string slug,
        IEntity entity,
        string path,
        object? value)
        : base(
            id,
            slug,
            entity,
            path,
            value)
    {
    }

    public void UpdateValue(
        object? value)
    {
        SetValue(
            value);
    }

    public bool RegisterDiscovery(
        IDiscovery discovery)
    {
        return AddDiscovery(
            discovery);
    }

    public bool UnregisterDiscovery(
        IDiscovery discovery)
    {
        return RemoveDiscovery(
            discovery);
    }
}