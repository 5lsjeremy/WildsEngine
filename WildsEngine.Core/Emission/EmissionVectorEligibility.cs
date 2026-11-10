using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Abstractions.Neighbors.Propagation;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Evaluates every received component of one propagated
    /// emission vector through one supplied eligibility
    /// policy.
    ///
    /// The vector is not filtered or mutated.
    /// </summary>
    public sealed class EmissionVectorEligibility
    {
        private readonly INeighborPropagationEligibility
            _eligibility;

        public EmissionVectorEligibility(
            INeighborPropagationEligibility eligibility)
        {
            _eligibility =
                eligibility ??
                throw new ArgumentNullException(
                    nameof(eligibility));
        }

        public IEmissionEligibilityVector Evaluate(
            IPropagatedEmissionVector vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException(
                    nameof(vector));
            }

            var results =
                new List<IEmissionEligibilityResult>(
                    vector.Dimension);

            for (var index = 0;
                 index < vector.Dimension;
                 index++)
            {
                var emission =
                    vector.Emissions[index];

                var evaluatedValue =
                    emission.Value;

                var eligible =
                    _eligibility.IsEligible(
                        evaluatedValue);

                results.Add(
                    new EmissionEligibilityResult(
                        emission,
                        evaluatedValue,
                        eligible));
            }

            return new EmissionEligibilityVector(
                vector,
                results);
        }
    }
}