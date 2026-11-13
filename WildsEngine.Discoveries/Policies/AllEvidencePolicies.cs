using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Policies
{
    /// <summary>
    /// Counts Evidence only when every supplied policy accepts it.
    /// </summary>
    public sealed class AllEvidencePolicies :
        EvidenceContributionPolicyBase
    {
        private readonly IReadOnlyList<IEvidenceContributionPolicy> _policies;

        public AllEvidencePolicies(
            IReadOnlyList<IEvidenceContributionPolicy> policies)
        {
            if (policies == null)
            {
                throw new ArgumentNullException(
                    nameof(policies));
            }

            if (policies.Count == 0)
            {
                throw new ArgumentException(
                    "At least one policy is required.",
                    nameof(policies));
            }

            var snapshot =
                new List<IEvidenceContributionPolicy>(
                    policies.Count);

            for (var index = 0;
                 index < policies.Count;
                 index++)
            {
                snapshot.Add(
                    policies[index] ??
                    throw new ArgumentException(
                        "Policies cannot contain null.",
                        nameof(policies)));
            }

            _policies =
                new ReadOnlyCollection<IEvidenceContributionPolicy>(
                    snapshot);
        }

        public IReadOnlyList<IEvidenceContributionPolicy> Policies =>
            _policies;

        protected override bool Evaluate(
            IDiscoveryEvidence evidence)
        {
            for (var index = 0;
                 index < _policies.Count;
                 index++)
            {
                if (!_policies[index].Contributes(evidence))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
