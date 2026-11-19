using System;using System.Collections.Generic;
namespace WildsEngine.Discoveries.Abstractions.Narratives
{
    public interface IDiscoveryCompetingNarrativesResult
    {
        Guid Id { get; }
        IDiscoveryNarrativeCompositionResult Composition { get; }
        IReadOnlyList<IDiscoveryNarrativeGroupRelationship> GroupRelationships { get; }
        DateTimeOffset EvaluatedAt { get; }
    }
}
