using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Base
{
    public abstract class WildsFactBase :
        WildsProjectionBase<IFact>,
        IWildsFact
    {
        protected WildsFactBase(
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

        public IFact Fact =>
            Source;
    }
}
