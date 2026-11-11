using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Entities.Concrete
{
    public sealed class WildsFact :
        WildsFactBase
    {
        public WildsFact(
            IFact fact,
            IComponent component,
            Guid id,
            string layer)
            : base(
                fact,
                component,
                id,
                layer)
        {
        }
    }
}
