using System;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Proofs;

namespace WildsEngine.Core.Entities.Base
{
    /// <summary>
    /// Provides the reusable Evidence validation lifecycle.
    ///
    /// Concrete Proof implementations determine:
    ///
    /// - how believable the Evidence is,
    /// - whether the Evidence resolves as true,
    /// - and any consumer-facing reason for that conclusion.
    ///
    /// Proof does not decide which Fact or Discovery
    /// will consume the Evidence.
    /// </summary>
    public abstract class ProofBase :
        IProof
    {
        public IProofResult Validate(
            IDiscoveryEvidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException(
                    nameof(evidence));
            }

            ResolveEvidence(
                evidence,
                out var believability,
                out var isTrue,
                out var reason);

            if (double.IsNaN(
                    believability) ||
                double.IsInfinity(
                    believability) ||
                believability < 0.0 ||
                believability > 1.0)
            {
                throw new InvalidOperationException(
                    "Proof resolution produced an invalid " +
                    "Believability value.");
            }

            return new ProofResult(
                evidence,
                this,
                believability,
                isTrue,
                reason);
        }

        protected abstract void ResolveEvidence(
            IDiscoveryEvidence evidence,
            out double believability,
            out bool isTrue,
            out string? reason);
    }
}