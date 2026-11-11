using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Tests.Entities.Fixtures
{
    public sealed class TestProof :
        ProofBase
    {
        private readonly double _believability;
        private readonly bool _isTrue;
        private readonly string? _reason;

        public TestProof(
            double believability,
            bool isTrue,
            string? reason = null)
        {
            _believability = believability;
            _isTrue = isTrue;
            _reason = reason;
        }

        protected override void ResolveEvidence(
            IDiscoveryEvidence evidence,
            out double believability,
            out bool isTrue,
            out string? reason)
        {
            believability = _believability;
            isTrue = _isTrue;
            reason = _reason;
        }
    }
}
