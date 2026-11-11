using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Base
{
    public abstract class WildsEvidenceBase :
        WildsProjectionBase<IDiscoveryEvidence>,
        IWildsEvidence
    {
        protected WildsEvidenceBase(
            IDiscoveryEvidence evidence,
            IComponent component,
            Guid id,
            string layer)
            : base(
                evidence,
                component,
                id,
                layer)
        {
        }

        public IDiscoveryEvidence Evidence =>
            Source;
    }
}
