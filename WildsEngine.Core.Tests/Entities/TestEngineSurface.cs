using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Tests.Entities;

public sealed class TestEngineSurface :
    EntitySurfaceBase
{
    public TestEngineSurface(
        IEntity entity,
        object target,
        string alias,
        string consumerType)
        : base(
            entity,
            target,
            alias,
            consumerType)
    {
    }
}