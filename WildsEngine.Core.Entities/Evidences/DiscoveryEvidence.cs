using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Base;

namespace WildsEngine.Core.Entities.Evidences
{
    /// <summary>
    /// Concrete consumer-facing Evidence gathered from
    /// one resolved world observation.
    /// </summary>
    public sealed class DiscoveryEvidence :
        EvidenceBase
    {
        public DiscoveryEvidence(
            IEntity entity,
            IReadOnlyDictionary<string, object?> conditions,
            object? outcome,
            double strength,
            double normalizedStrength,
            IReadOnlyList<IBaseObject> neighbors,
            IBaseUnit? source,
            string? externalId = null,
            string? name = null)
            : base(
                entity,
                conditions,
                outcome,
                strength,
                normalizedStrength,
                neighbors,
                source,
                externalId,
                name)
        {
        }

        public bool ApplyProof(
            IProofResult proofResult)
        {
            return SetProofResult(
                proofResult);
        }
    }
}