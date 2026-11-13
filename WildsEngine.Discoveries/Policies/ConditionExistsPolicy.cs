using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    public sealed class ConditionExistsPolicy :
        EvidenceContributionPolicyBase
    {
        public ConditionExistsPolicy(
            string conditionName)
        {
            if (string.IsNullOrWhiteSpace(conditionName))
            {
                throw new ArgumentException(
                    "A condition name is required.",
                    nameof(conditionName));
            }

            ConditionName =
                conditionName.Trim();
        }

        public string ConditionName { get; }

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            return evidence.Conditions.ContainsKey(
                ConditionName);
        }
    }
}
