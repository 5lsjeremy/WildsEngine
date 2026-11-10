using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Abstractions.Neighbors.Propagation;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Evaluates one propagated emission using the existing
    /// Neighbor propagation-eligibility contract.
    ///
    /// Eligibility consumes the received propagated Value.
    /// </summary>
    public sealed class EmissionEligibility
    {
        private readonly INeighborPropagationEligibility
            _eligibility;

        public EmissionEligibility(
            INeighborPropagationEligibility eligibility)
        {
            _eligibility =
                eligibility ??
                throw new ArgumentNullException(
                    nameof(eligibility));
        }

        public IEmissionEligibilityResult Evaluate(
            IPropagatedEmission emission)
        {
            if (emission == null)
            {
                throw new ArgumentNullException(
                    nameof(emission));
            }

            var evaluatedValue =
                emission.Value;

            var eligible =
                _eligibility.IsEligible(
                    evaluatedValue);

            return new EmissionEligibilityResult(
                emission,
                evaluatedValue,
                eligible);
        }
    }
}