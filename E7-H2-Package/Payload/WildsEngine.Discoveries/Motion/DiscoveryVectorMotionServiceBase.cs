using System;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Services;

namespace WildsEngine.Discoveries.Motion
{
    public abstract class DiscoveryVectorMotionServiceBase :
        WildsServiceBase<DiscoveryVectorMotionRequest, DiscoveryVectorMotionServiceResult>,
        IDiscoveryVectorMotionService
    {
        protected DiscoveryVectorMotionServiceBase(
            string slug,
            IWildsServiceContextFactory<IWildsServiceContext> contexts,
            IWildsServiceExecutionJournal journal,
            IWildsServiceClock clock)
            : base(slug, contexts, journal, clock) { }
    }
}
