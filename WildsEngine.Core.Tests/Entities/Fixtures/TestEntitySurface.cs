using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Core.Tests.Entities.Fixtures
{
    public sealed class TestEntitySurface :
        EntitySurfaceBase
    {
        public TestEntitySurface(
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
}
