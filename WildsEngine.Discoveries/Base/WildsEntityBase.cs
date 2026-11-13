using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    public abstract class WildsEntityBase :
        WildsProjectionBase<IEntity>,
        IWildsEntity
    {
        protected WildsEntityBase(
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

        public IEntity Entity =>
            Source;
    }
}