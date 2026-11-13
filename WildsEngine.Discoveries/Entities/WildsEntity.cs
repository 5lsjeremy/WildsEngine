using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries
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