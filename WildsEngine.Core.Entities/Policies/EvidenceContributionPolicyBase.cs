using System;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Policies
{
    /// <summary>
    /// Provides null validation for reusable Evidence contribution policies.
    /// </summary>
    public abstract class EvidenceContributionPolicyBase :
        IEvidenceContributionPolicy
    {
        public bool Contributes(
            IDiscoveryEvidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException(
                    nameof(evidence));
            }

            return Evaluate(
                evidence);
        }

        protected abstract bool Evaluate(
            IDiscoveryEvidence evidence);

        protected static double RequireNormalized(
            double value,
            string parameterName)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < 0.0 ||
                value > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "The value must be between zero and one.");
            }

            return value;
        }
    }
}
