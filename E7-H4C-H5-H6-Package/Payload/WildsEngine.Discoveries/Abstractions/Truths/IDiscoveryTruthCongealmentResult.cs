using System;using System.Collections.Generic;using WildsEngine.Discoveries.Abstractions.Narratives;
namespace WildsEngine.Discoveries.Abstractions.Truths
{
    public interface IDiscoveryTruthCongealmentResult
    {
        Guid Id { get; }
        IDiscoveryCompetingNarrativesResult Narratives { get; }
        IDiscoveryTruthRegistration Registration { get; }
        IDiscoveryVectorSeries Series { get; }
        IReadOnlyList<IDiscoveryCluster> Clusters { get; }
        IReadOnlyList<ITruth> Truths { get; }
        bool Congealed { get; }
        DateTimeOffset EvaluatedAt { get; }
    }
}
