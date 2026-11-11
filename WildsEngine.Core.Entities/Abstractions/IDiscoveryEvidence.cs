using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Entities.Abstractions
{
    public interface IDiscoveryEvidence
    {
        string? ExternalId { get; }

        string? Name { get; }

        IEntity Entity { get; }

        IReadOnlyDictionary<string, object?>
            Conditions { get; }

        object? Outcome { get; }

        double Strength { get; }

        double NormalizedStrength { get; }

        IReadOnlyList<IBaseObject> Neighbors { get; }

        IBaseUnit? Source { get; }

        bool IsValidated { get; }

        IProofResult ProofResult { get; }
    }
}