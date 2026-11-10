namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents a Wilds state instruction resolved from one
    /// exact eligible propagated-emission decision.
    ///
    /// The source decision remains authoritative provenance.
    /// Resolving the instruction does not process it or mutate
    /// receiver state.
    /// </summary>
    public interface IEmissionStateInstruction :
        IStateInstruction
    {
        IEmissionEligibilityResult Source { get; }
    }
}