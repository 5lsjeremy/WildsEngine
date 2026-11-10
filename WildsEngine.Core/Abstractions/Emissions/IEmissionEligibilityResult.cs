namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents the immutable eligibility decision for one
    /// propagated emission.
    ///
    /// Eligibility is evaluated against the received
    /// propagated Value.
    ///
    /// No receiver-state mutation or consumer interpretation
    /// occurs here.
    /// </summary>
    public interface IEmissionEligibilityResult
    {
        IPropagatedEmission Emission { get; }

        double EvaluatedValue { get; }

        bool IsEligible { get; }
    }
}