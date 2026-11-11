namespace WildsEngine.Core.Entities.Abstractions
{
    public interface IProofResult
    {
        IDiscoveryEvidence Evidence { get; }

        IProof Proof { get; }

        double Believability { get; }

        bool IsTrue { get; }

        string? Reason { get; }
    }
}