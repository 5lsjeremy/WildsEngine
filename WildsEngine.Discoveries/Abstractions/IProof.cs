namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Validates one Evidence item without deciding which Fact or Discovery
    /// will consume it.
    /// </summary>
    public interface IProof
    {
        IProofResult Validate(IDiscoveryEvidence evidence);
    }
}
