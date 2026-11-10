using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Immutable eligibility result for one exact propagated
    /// emission vector.
    ///
    /// Every propagated channel retains a corresponding
    /// eligibility result. Ineligible channels are not
    /// removed.
    /// </summary>
    public sealed class EmissionEligibilityVector :
        IEmissionEligibilityVector
    {
        private readonly IReadOnlyList<IEmissionEligibilityResult>
            _emissions;

        public EmissionEligibilityVector(
            IPropagatedEmissionVector source,
            IReadOnlyList<IEmissionEligibilityResult> emissions)
        {
            Source =
                source ??
                throw new ArgumentNullException(
                    nameof(source));

            if (emissions == null)
            {
                throw new ArgumentNullException(
                    nameof(emissions));
            }

            if (emissions.Count !=
                source.Dimension)
            {
                throw new ArgumentException(
                    "Eligibility-vector dimension must match " +
                    "the propagated-vector dimension.",
                    nameof(emissions));
            }

            var snapshot =
                new IEmissionEligibilityResult[
                    emissions.Count];

            var eligibleCount =
                0;

            for (var index = 0;
                 index < emissions.Count;
                 index++)
            {
                var result =
                    emissions[index] ??
                    throw new ArgumentException(
                        "An eligibility vector cannot contain " +
                        "a null component.",
                        nameof(emissions));

                if (!ReferenceEquals(
                        source.Emissions[index],
                        result.Emission))
                {
                    throw new ArgumentException(
                        "Eligibility must preserve exact " +
                        "propagated-emission references and " +
                        "component ordering.",
                        nameof(emissions));
                }

                if (result.EvaluatedValue !=
                    source.Emissions[index].Value)
                {
                    throw new ArgumentException(
                        "Eligibility must preserve the exact " +
                        "received value evaluated for each " +
                        "component.",
                        nameof(emissions));
                }

                snapshot[index] =
                    result;

                if (result.IsEligible)
                {
                    eligibleCount++;
                }
            }

            _emissions =
                Array.AsReadOnly(
                    snapshot);

            EligibleCount =
                eligibleCount;
        }

        public IPropagatedEmissionVector Source { get; }

        public IReadOnlyList<IEmissionEligibilityResult> Emissions =>
            _emissions;

        public int Dimension =>
            _emissions.Count;

        public int EligibleCount { get; }

        public bool AnyEligible =>
            EligibleCount > 0;

        public bool AllEligible =>
            EligibleCount == Dimension;
    }
}