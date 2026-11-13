using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Concrete
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
