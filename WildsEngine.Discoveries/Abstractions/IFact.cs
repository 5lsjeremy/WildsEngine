using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    public interface IFact :
        IResolutionThreshold<IDiscoveryEvidence>
    {
        string? ExternalId { get; }

        string Name { get; }

        object? Value { get; }

        IEntity Entity { get; }

        IEvidenceContributionPolicy ContributionPolicy { get; }

        IReadOnlyList<IDiscoveryEvidence> Evidence { get; }

        int RequiredEvidenceCount { get; }

        int CurrentEvidenceCount { get; }

        double EvidencePercentage { get; }

        bool IsProven { get; }
    }
}