using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    public sealed class NotEvidencePolicy :
        EvidenceContributionPolicyBase
    {
        private readonly IEvidenceContributionPolicy _policy;

        public NotEvidencePolicy(
            IEvidenceContributionPolicy policy)
        {
            _policy =
                policy ??
                throw new ArgumentNullException(
                    nameof(policy));
        }

        public IEvidenceContributionPolicy Policy =>
            _policy;

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return !_policy.Contributes(
                evidence);
        }
    }
}
