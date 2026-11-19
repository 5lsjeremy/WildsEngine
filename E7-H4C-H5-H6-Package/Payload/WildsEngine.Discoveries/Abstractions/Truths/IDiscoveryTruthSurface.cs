using System;using System.Collections.Generic;using WildsEngine.Discoveries.Abstractions.Narratives;
namespace WildsEngine.Discoveries.Abstractions.Truths
{
    public interface IDiscoveryTruthSurface
    {
        IDiscoveryTruthRegistration Register(IDiscoveryTruthProfile profile);
        IDiscoveryTruthCongealmentResult Congeal(IDiscoveryCompetingNarrativesResult narratives,Guid registrationId,IDiscoveryVectorSeries series,IReadOnlyList<IDiscoveryCluster> clusters);
    }
}
