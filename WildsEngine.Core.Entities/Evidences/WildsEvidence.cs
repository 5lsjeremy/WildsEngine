using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Entities.Concrete
{
    public sealed class WildsEvidence :
        WildsEvidenceBase
    {
        public WildsEvidence(
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
    }
}
