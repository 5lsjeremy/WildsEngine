using System;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Tests.Entities.Fixtures
{
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
    }
}
