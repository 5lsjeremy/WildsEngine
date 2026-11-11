using System;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Proofs
{
    public sealed class ProofResult :
        IProofResult
    {
        public ProofResult(
            IDiscoveryEvidence evidence,
            IProof proof,
            double believability,
            bool isTrue,
            string? reason = null)
        {
            Evidence =
                evidence ??
                throw new ArgumentNullException(
                    nameof(evidence));

            Proof =
                proof ??
                throw new ArgumentNullException(
                    nameof(proof));

            if (double.IsNaN(
                    believability) ||
                double.IsInfinity(
                    believability) ||
                believability < 0.0 ||
                believability > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(believability),
                    "Believability must be between zero and one.");
            }

            Believability =
                believability;

            IsTrue =
                isTrue;

            Reason =
                reason;
        }

        public IDiscoveryEvidence Evidence { get; }

        public IProof Proof { get; }

        public double Believability { get; }

        public bool IsTrue { get; }

        public string? Reason { get; }
    }
}