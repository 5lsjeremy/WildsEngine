using System;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Vectorization;
using WildsEngine.Discoveries.Services;

namespace WildsEngine.Discoveries.Vectorization
{
    public abstract class DiscoveryVectorizationServiceBase :
        WildsServiceBase<DiscoveryVectorizationRequest, DiscoveryVectorizationServiceResult>,
        IDiscoveryVectorizationService
    {
        protected DiscoveryVectorizationServiceBase(
            string slug,
            IWildsServiceContextFactory<IWildsServiceContext> contexts,
            IWildsServiceExecutionJournal journal,
            IWildsServiceClock clock)
            : base(slug, contexts, journal, clock) { }

        protected DiscoveryVectorizationServiceBase(
            Guid id,
            string slug,
            IWildsServiceContextFactory<IWildsServiceContext> contexts,
            IWildsServiceExecutionJournal journal,
            IWildsServiceClock clock)
            : base(id, slug, contexts, journal, clock) { }
    }
}
