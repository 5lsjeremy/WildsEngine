using System;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Policies
{
    public sealed class OutcomeEqualsPolicy :
        EvidenceContributionPolicyBase
    {
        private readonly object? _expectedOutcome;

        public OutcomeEqualsPolicy(
            object? expectedOutcome)
        {
            _expectedOutcome =
                expectedOutcome;
        }

        public object? ExpectedOutcome =>
            _expectedOutcome;

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return Equals(
                evidence.Outcome,
                _expectedOutcome);
        }
    }
}
