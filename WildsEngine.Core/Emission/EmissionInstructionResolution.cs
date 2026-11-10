using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Resolves eligible propagated-emission facts into the
    /// existing Wilds state-instruction model.
    ///
    /// Resolution does not register, process, or execute the
    /// resulting instructions.
    /// </summary>
    public sealed class EmissionInstructionResolution
    {
        private readonly IEmissionInstructionResolver _resolver;

        public EmissionInstructionResolution(
            IEmissionInstructionResolver resolver)
        {
            _resolver =
                resolver ??
                throw new ArgumentNullException(
                    nameof(resolver));
        }

        public IEmissionStateInstruction Resolve(
            IEmissionEligibilityResult eligibility)
        {
            if (eligibility == null)
            {
                throw new ArgumentNullException(
                    nameof(eligibility));
            }

            if (!eligibility.IsEligible)
            {
                throw new ArgumentException(
                    "State-instruction resolution requires an " +
                    "eligible propagated-emission decision.",
                    nameof(eligibility));
            }

            var instruction =
                _resolver.Resolve(
                    eligibility);

            ValidateResolvedInstruction(
                eligibility,
                instruction);

            return instruction;
        }

        public IReadOnlyList<IEmissionStateInstruction> Resolve(
            IEmissionEligibilityVector eligibility)
        {
            if (eligibility == null)
            {
                throw new ArgumentNullException(
                    nameof(eligibility));
            }

            var resolved =
                new List<IEmissionStateInstruction>(
                    eligibility.EligibleCount);

            for (var index = 0;
                 index < eligibility.Dimension;
                 index++)
            {
                var decision =
                    eligibility.Emissions[index];

                if (!decision.IsEligible)
                {
                    continue;
                }

                resolved.Add(
                    Resolve(
                        decision));
            }

            return resolved.AsReadOnly();
        }

        private static void ValidateResolvedInstruction(
            IEmissionEligibilityResult eligibility,
            IEmissionStateInstruction instruction)
        {
            if (instruction == null)
            {
                throw new InvalidOperationException(
                    "The emission instruction resolver returned null.");
            }

            if (!ReferenceEquals(
                    eligibility,
                    instruction.Source))
            {
                throw new InvalidOperationException(
                    "The resolved instruction must preserve the " +
                    "exact eligibility-result reference.");
            }
        }
    }
}