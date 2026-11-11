using System;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Policies
{
    public sealed class ConditionEqualsPolicy :
        EvidenceContributionPolicyBase
    {
        private readonly object? _expectedValue;

        public ConditionEqualsPolicy(
            string conditionName,
            object? expectedValue)
        {
            if (string.IsNullOrWhiteSpace(conditionName))
            {
                throw new ArgumentException(
                    "A condition name is required.",
                    nameof(conditionName));
            }

            ConditionName =
                conditionName.Trim();

            _expectedValue =
                expectedValue;
        }

        public string ConditionName { get; }
        public object? ExpectedValue => _expectedValue;

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return evidence.Conditions.TryGetValue(
                       ConditionName,
                       out var actualValue) &&
                   Equals(
                       actualValue,
                       _expectedValue);
        }
    }
}
