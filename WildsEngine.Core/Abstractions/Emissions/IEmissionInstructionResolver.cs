namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Maps one eligible propagated-emission decision into a
    /// consumer-defined Wilds state instruction.
    ///
    /// The resolver owns interpretation. Wilds owns validation
    /// of eligibility and exact source-provenance continuity.
    /// </summary>
    public interface IEmissionInstructionResolver
    {
        IEmissionStateInstruction Resolve(
            IEmissionEligibilityResult eligibility);
    }
}