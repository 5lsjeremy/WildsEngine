using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Immutable eligibility decision for one propagated
    /// emission.
    ///
    /// EvaluatedValue snapshots the received propagated
    /// magnitude supplied to the eligibility policy.
    /// </summary>
    public sealed class EmissionEligibilityResult :
        IEmissionEligibilityResult
    {
        public EmissionEligibilityResult(
            IPropagatedEmission emission,
            double evaluatedValue,
            bool isEligible)
        {
            Emission =
                emission ??
                throw new ArgumentNullException(
                    nameof(emission));

            if (double.IsNaN(evaluatedValue) ||
                double.IsInfinity(evaluatedValue) ||
                evaluatedValue < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(evaluatedValue),
                    evaluatedValue,
                    "Eligibility evaluation value must be " +
                    "finite and non-negative.");
            }

            if (evaluatedValue !=
                emission.Value)
            {
                throw new ArgumentException(
                    "Eligibility must evaluate the exact " +
                    "received propagated value.",
                    nameof(evaluatedValue));
            }

            EvaluatedValue =
                evaluatedValue;

            IsEligible =
                isEligible;
        }

        public IPropagatedEmission Emission { get; }

        public double EvaluatedValue { get; }

        public bool IsEligible { get; }
    }
}