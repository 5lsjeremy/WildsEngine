using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    /// <summary>
    /// Adapts an arbitrary consumer predicate into an Evidence policy.
    /// Useful at integration boundaries and in certification fixtures.
    /// </summary>
    public sealed class DelegateEvidencePolicy :
        EvidenceContributionPolicyBase
    {
        private readonly Func<IDiscoveryEvidence, bool> _predicate;

        public DelegateEvidencePolicy(
            Func<IDiscoveryEvidence, bool> predicate)
        {
            _predicate =
                predicate ??
                throw new ArgumentNullException(
                    nameof(predicate));
        }

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return _predicate(
                evidence);
        }
    }
}
