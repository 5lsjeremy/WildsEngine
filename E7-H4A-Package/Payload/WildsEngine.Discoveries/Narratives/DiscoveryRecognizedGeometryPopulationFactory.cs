using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Abstractions.Services;
using WildsEngine.Discoveries.Abstractions.Surface;

namespace WildsEngine.Discoveries.Narratives
{
    public sealed class DiscoveryRecognizedGeometryPopulationFactory :
        IDiscoveryRecognizedGeometryPopulationFactory
    {
        private readonly IWildsServiceClock _clock;

        public DiscoveryRecognizedGeometryPopulationFactory(IWildsServiceClock clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public IDiscoveryRecognizedGeometryPopulation Create(
            IDiscoveryGeometryRegistration registration,
            IReadOnlyList<IDiscoveryClusterRecognitionResult> members)
        {
            return new DiscoveryRecognizedGeometryPopulation(
                Guid.NewGuid(),
                registration,
                members,
                _clock.GetUtcNow());
        }
    }
}
