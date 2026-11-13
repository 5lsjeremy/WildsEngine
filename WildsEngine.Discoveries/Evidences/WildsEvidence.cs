using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Concrete
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
