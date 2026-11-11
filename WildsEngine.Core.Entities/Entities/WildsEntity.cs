using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Entities
{
    public sealed class WildsEntity :
        WildsEntityBase
    {
        public WildsEntity(
            IEntity entity,
            IComponent component,
            Guid id,
            string layer)
            : base(
                entity,
                component,
                id,
                layer)
        {
        }
    }
}